using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;
using UnityEngine.SpatialTracking;
using UnityEngine.Networking;
using Newtonsoft.Json;


/// <summary>
/// Holds player information, and instantiates the correct UI based on user type, and handles network interaction. 
/// </summary>
public class Player : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SyncVar]
    public bool isResearcher;

    public GameObject ResearcherUIPrefab; 
    public GameObject ParticipantUIPrefab;

    public Material TransitionalMaterial;
    public Material CueEnvironmentMaterial;

    //GameObject XRRig;
    //GameObject ParticipantCamera;
    //GameObject ResearcherCamera;

    GameObject LoadingScreen;

    GameObject PanoramaCamera;
    
    GameObject PanoramaSphere;

    GameObject userInterface;

    public static event Action<Player, ChatMessage> OnMessage;

    private void Awake()
    {
        LoadingScreen = GameObject.Find("LoadingScreenCanvas");

    }


    /// <summary>
    /// Instantiate the appropriate UI depending on whether the user has logged in as Researcher or Participant. Activate the UI only on the client that has ownership of it, so as to not display every UI on every client.
    /// </summary>
    [ClientRpc]
    public void InstantiateUI()
    {
        LoadingScreen.GetComponent<Canvas>().enabled = true;

        PanoramaSphere = GameObject.Find("PanoramaSphere");
        GameObject researcherCamera = GameObject.Find("ResearcherCamera");
        GameObject participantCamera = GameObject.Find("ParticipantCamera");


        if (isResearcher && hasAuthority && isLocalPlayer)
        {
            PanoramaCamera = researcherCamera;
            userInterface = Instantiate(ResearcherUIPrefab, transform);
            participantCamera.SetActive(false);
        }
        else if (hasAuthority && isLocalPlayer)
        {
            PanoramaCamera = participantCamera;
            // Instantiate Participant UI in World Space overlaid on a 3D object, and attach it to the camera so that it moves with it.
            userInterface = Instantiate(ParticipantUIPrefab);
            userInterface.transform.parent = PanoramaCamera.transform;

            PanoramaCamera.GetComponent<Camera>().enabled = true;
            PanoramaCamera.GetComponent<TrackedPoseDriver>().enabled = true;
            PanoramaCamera.GetComponent<TrackCameraMovement>().enabled = true;

            researcherCamera.SetActive(false);

            //ResearcherCamera.GetComponent<Camera>().enabled = false;
            
            // Instantiate Participant UI in Screen Space as a child of the Player object 
            //userInterface = Instantiate(ParticipantUIPrefab, transform);
        }
        else
        {
            PanoramaCamera = Camera.main.gameObject;
        }
        

        if (hasAuthority || isLocalPlayer)
        {
            userInterface.SetActive(true);
        }

        if (!isResearcher)
        {
            // Presenting the environment when a client logs in.
            LoadEnvironmentFromJSON();
        }
    }

    [Command]
    public void CmdSend(ChatMessage chatMessage)
    {
        RpcReceive(chatMessage);
    }

    [ClientRpc]
    public void RpcReceive(ChatMessage chatMessage)
    {
        OnMessage?.Invoke(this, chatMessage);
    }


    [Command]
    public void RotateCamera(Vector3 rotation)
    {
        ReceiveCameraRotation(rotation);
    }

    [ClientRpc]
    public void ReceiveCameraRotation(Vector3 rotation)
    {        
        PanoramaCamera.transform.eulerAngles = rotation;        
    }

    public void LoadEnvironmentFromJSON()
    {
        string filePathAddition = "/Environments";

        string filePath = Application.streamingAssetsPath + filePathAddition;

        // Loading files from StreamingAssets folder
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        FileInfo[] AllFilesInFolder = directoryInfo.GetFiles("*.*");

        var jsonString = File.ReadAllText(Application.streamingAssetsPath + "/EnvironmentToPresent.json");
        //string environmentToPresentName = JsonReader.
        //string environmentToPresentName = JsonConvert.DeserializeObject<string>(jsonString);

        string environmentToPresentName = jsonString.Substring(jsonString.IndexOf("{ \"") + 3);
        environmentToPresentName = environmentToPresentName.Substring(0, jsonString.IndexOf("\" }") - 3);
        //string environmentToPresentName = jsonString;


        foreach (var file in AllFilesInFolder)
        {
            //Debug.Log(file.Name);
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

    public void PresentEnvironment(Texture2D texture, AudioClip audioClip) 
    {
        PanoramaSphere.GetComponent<Renderer>().material = CueEnvironmentMaterial;
        PanoramaSphere.GetComponent<Renderer>().material.mainTexture = texture;

        AudioSource audioSource = PanoramaSphere.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.Play();

        //PanoramaCamera.GetComponent<AudioSource>().Play();

        LoadingScreen.SetActive(false);
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
        {
            if (hasAuthority || isLocalPlayer)
            {
                if (isResearcher)
                {
                    if (userInterface.GetComponent<Canvas>())
                    {
                        userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
                    }
                    
                }
                else
                {
                    // Uncomment the line below if using Screen-Space UI
                    //userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
                    if (userInterface.GetComponent<MeshRenderer>())
                    {
                        userInterface.GetComponent<MeshRenderer>().enabled = !userInterface.GetComponent<MeshRenderer>().enabled;
                    }
                    if (userInterface.transform.GetChild(0).GetComponent<Canvas>())
                    {
                        userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled = !userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled;
                    }
                    
                }
                    
            }

        }


        if (isResearcher || !hasAuthority)
            return;

    }


    /// <summary>
    /// Load the texture and audio, and add them to the EnvironmentData object list
    /// </summary>
    /// <param name="textureFile"></param>
    /// <returns></returns>
    IEnumerator LoadEnvironmentFiles(string textureFileName, string audioClipFileName)
    {
        string wwwTextureFilePath = "file://" + textureFileName;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(wwwTextureFilePath);
        yield return webRequest.SendWebRequest();
        Texture2D importedTexture = DownloadHandlerTexture.GetContent(webRequest);

        string wwwAudioFilePath = "file://" + audioClipFileName;
        webRequest = UnityWebRequestMultimedia.GetAudioClip(wwwAudioFilePath, AudioType.WAV);
        yield return webRequest.SendWebRequest();
        AudioClip importedAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        PresentEnvironment(importedTexture, importedAudioClip);
    }
}
