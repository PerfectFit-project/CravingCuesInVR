using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;
using UnityEngine.SpatialTracking;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Collections;


/// <summary>
/// Holds player information, and instantiates the correct UI based on user type, and handles network interaction. 
/// </summary>
public class Player : NetworkBehaviour
{
    NetworkManager NetManager;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();
    public NetworkVariable<bool> IsResearcher = new NetworkVariable<bool>();

    public string UserName;

    bool EnvironmentLoaded;
    bool EnvironmentPresented;

    public GameObject ResearcherUIPrefab;
    public GameObject ParticipantUIPrefab;

    public Material TransitionalMaterial;
    public Material CueEnvironmentMaterial;

    Texture2D textureToPresent;
    AudioClip audioClipToPlay;

    GameObject LoadingScreen;

    GameObject PanoramaCamera;

    GameObject PanoramaSphere;

    GameObject userInterface;

    public static event Action<Player, ChatMessage> OnMessage;

    private void Awake()
    {
        Debug.Log("AWAKE HAPPENS");
        LoadingScreen = GameObject.Find("LoadingScreenCanvas");
    }


    [ServerRpc]
    public void InitPlayerServerRpc(string userName, bool isResearcher)
    {
        Debug.Log("CUSTOM START HAPPENS");
        NetManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        EnvironmentLoaded = false;
        EnvironmentPresented = false;

        IsResearcher = new NetworkVariable<bool>(isResearcher);

        UserName = userName;

        Debug.Log("IS RESEARCHER: " + isResearcher);

        InstantiateUIClientRpc();
    }

    /// <summary>
    /// Instantiate the appropriate UI depending on whether the user has logged in as Researcher or Participant. Activate the UI only on the client that has ownership of it, so as to not display every UI on every client.
    /// </summary>
    [ClientRpc]
    public void InstantiateUIClientRpc()
    {
        LoadingScreen.GetComponent<Canvas>().enabled = true;

        PanoramaSphere = GameObject.Find("PanoramaSphere");
        GameObject researcherCamera = GameObject.Find("ResearcherCamera");
        GameObject participantCamera = GameObject.Find("ParticipantCamera");


        if (IsResearcher.Value && IsOwner)
        {
            PanoramaCamera = researcherCamera;
            userInterface = Instantiate(ResearcherUIPrefab, transform);
            userInterface.GetComponent<SetupResearcherUI_NTW>().InitializeResearcherUI();
            participantCamera.SetActive(false);
        }
        else if (IsOwner)
        {
            PanoramaCamera = participantCamera;
            // Instantiate Participant UI in World Space overlaid on a 3D object, and attach it to the camera so that it moves with it.
            userInterface = Instantiate(ParticipantUIPrefab);
            userInterface.transform.parent = PanoramaCamera.transform;

            userInterface.transform.localPosition = new Vector3(0f, -0.8f, 1.4f);
            userInterface.transform.localEulerAngles = new Vector3(-10f, 180f, 0f);

            PanoramaCamera.GetComponent<Camera>().enabled = true;
            PanoramaCamera.GetComponent<TrackedPoseDriver>().enabled = true;
            PanoramaCamera.GetComponent<TrackCameraMovement>().enabled = true;

            researcherCamera.SetActive(false);
        }
        else
        {
            PanoramaCamera = Camera.main.gameObject;
        }

        if (IsOwner)
        {
            userInterface.SetActive(true);
            LoadEnvironmentFromJSON();
        }

    }

    [ServerRpc]
    public void CmdSendServerRpc(ChatMessage chatMessage)
    {
        RpcReceiveClientRpc(chatMessage);
    }

    [ClientRpc]
    public void RpcReceiveClientRpc(ChatMessage chatMessage)
    {
        OnMessage?.Invoke(this, chatMessage);
    }


    [ServerRpc]
    public void RotateCameraServerRpc(Vector3 rotation)
    {
        ReceiveCameraRotationClientRpc(rotation);
    }

    [ClientRpc]
    public void ReceiveCameraRotationClientRpc(Vector3 rotation)
    {
        if (IsHost)
        {
            PanoramaCamera.transform.eulerAngles = rotation;
        }

    }

    public void LoadEnvironmentFromJSON()
    {
        string filePathAddition = "/Environments";

        string filePath = Application.streamingAssetsPath + filePathAddition;

        // Loading files from StreamingAssets folder
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        FileInfo[] AllFilesInFolder = directoryInfo.GetFiles("*.*");

        var jsonString = File.ReadAllText(Application.streamingAssetsPath + "/EnvironmentToPresent.json");

        string environmentToPresentName = jsonString.Substring(jsonString.IndexOf("{ \"") + 3);
        environmentToPresentName = environmentToPresentName.Substring(0, jsonString.IndexOf("\" }") - 3);


        foreach (var file in AllFilesInFolder)
        {
            // Avoid .meta and .json files, and also files containing "audio" to prevent double loading
            if (file.Name.Contains(environmentToPresentName + "_image") && !file.Name.Contains("meta") && !file.Name.Contains("audio"))
            {
                foreach (var file2 in AllFilesInFolder)
                {
                    // Once a texture file has been found, locate the appropriate audio file for that environment, and send both to the coroutine to be loaded.
                    // Using the String Contains method, which will return true if the name of a file is a subset of another. Make sure that file names are distinct enough.
                    // Mandatory for texture and audio files to have the same name, followed by either "_image" or "_audio", followed by the file extension.
                    if (!file2.Name.Contains("meta") && !file2.Name.Contains("json") && !file2.Name.Contains("image"))
                    {
                        if (file2.Name.Contains(environmentToPresentName))
                        {
                            StartCoroutine(LoadEnvironmentFiles(file.FullName, file2.FullName));
                        }
                    }
                }
            }
        }

    }

    [ClientRpc]
    public void ShowEnvironmentClientRpc()
    {
        StartEnvironmentPresentation();
    }

    public void StartEnvironmentPresentation()
    {
        PanoramaSphere.GetComponent<Renderer>().material = CueEnvironmentMaterial;
        PanoramaSphere.GetComponent<Renderer>().material.mainTexture = textureToPresent;

        AudioSource audioSource = PanoramaSphere.GetComponent<AudioSource>();
        audioSource.clip = audioClipToPlay;
        audioSource.Play();

        LoadingScreen.SetActive(false);

        EnvironmentPresented = true;
    }

    public void PresentEnvironment(Texture2D texture, AudioClip audioClip)
    {
        PanoramaSphere.GetComponent<Renderer>().material = CueEnvironmentMaterial;
        PanoramaSphere.GetComponent<Renderer>().material.mainTexture = texture;

        AudioSource audioSource = PanoramaSphere.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();

        LoadingScreen.SetActive(false);
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
        {
            if (IsOwner)
            {
                if (IsResearcher.Value)
                {
                    if (userInterface.GetComponent<Canvas>())
                    {
                        userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
                    }

                }
                else
                {
                    if (userInterface.GetComponent<MeshRenderer>())
                    {
                        userInterface.GetComponent<MeshRenderer>().enabled = !userInterface.GetComponent<MeshRenderer>().enabled;
                    }
                    if (userInterface.transform.GetChild(0).GetComponent<Canvas>())
                    {
                        userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled = !userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled;
                    }
                    userInterface.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = !userInterface.transform.GetChild(1).GetComponent<MeshRenderer>().enabled;
                }
            }
        }

    }


    /// <summary>
    /// Load the texture and audio, and add them to the EnvironmentData object list
    /// </summary>
    /// <param name="textureFile"></param>
    /// <returns></returns>
    IEnumerator LoadEnvironmentFiles(string textureFileName, string audioClipFileName)
    {
        Debug.Log("LOADENVIRONMENTFILES");
        string wwwTextureFilePath = "file://" + textureFileName;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(wwwTextureFilePath);
        yield return webRequest.SendWebRequest();
        Texture2D importedTexture = DownloadHandlerTexture.GetContent(webRequest);

        string wwwAudioFilePath = "file://" + audioClipFileName;
        webRequest = UnityWebRequestMultimedia.GetAudioClip(wwwAudioFilePath, AudioType.WAV);
        yield return webRequest.SendWebRequest();
        AudioClip importedAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        textureToPresent = importedTexture;
        audioClipToPlay = importedAudioClip;
        EnvironmentLoaded = true;
    }



    private void Update()
    {
        if (IsOwner && IsLocalPlayer)
        {
            if (!EnvironmentPresented && EnvironmentLoaded)
            {
                if (IsResearcher.Value)
                {
                    if (NetManager.ConnectedClients.Count > 1)
                    {
                        StartEnvironmentPresentation();
                    }
                }
                else
                {
                    StartEnvironmentPresentation();
                }
            }
        }

    }
}
