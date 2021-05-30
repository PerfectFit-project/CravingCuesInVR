using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;

public class EnvironmentManagerLC : MonoBehaviour
{
    // Separating the two main materials.
    // For cues we want the custom material that correctly displays flat panoramic photos taken by a mainstream smartphone camera.
    // For the transitional environment we want a standard material that displays standard top-bottom-stretched 360 panoramic images correctly.
    public Material CueEnvironmentMaterial;
    public Material TransitionalMaterial;

    private FileInfo[] AllFilesInFolder;

    Dictionary<string, int> LoadedEnvsOrder;
    private List<EnvironmentData> LoadedEnvironments;
    // < Order to display, < Identifier, Texture2D to apply to Material >>
    public Dictionary<int, EnvironmentData> EnvironmentsInDisplayOrder;

    private int CurrentEnvironmentIndex;
    private int EnvironmentsCount;
    private int LoadedEnvironmentsCount;

    // In seconds
    private float EnvironmentDisplayTime;
    private float CurrentEnvironmentDisplayTime;

    private bool DisplayingEnvironment;
    

    void Awake()
    {        
        var jsonString = File.ReadAllText(Application.streamingAssetsPath + "/Environments/EnvironmentOrder.json");        
        LoadedEnvsOrder = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);       

        EnvironmentsCount = LoadedEnvsOrder.Count;
        LoadedEnvironmentsCount = 0;

        string filePathAddition = "/Environments";

        string filePath = Application.streamingAssetsPath + filePathAddition;

        // Loading files from StreamingAssets folder
        DirectoryInfo directoryInfo = new DirectoryInfo(filePath);
        AllFilesInFolder = directoryInfo.GetFiles("*.*");

       LoadedEnvironments = new List<EnvironmentData>();        
        
       foreach (var file in AllFilesInFolder)
       {
            // Avoid .meta and .json files, and also files containing "audio" to prevent double loading
            if (!file.Name.Contains("meta") && !file.Name.Contains("json") && !file.Name.Contains("audio"))
            {
                Debug.Log("TEXTURE FILE NAME: " + file.Name);
                string fileNameToCheck = file.Name.Substring(0, file.Name.IndexOf("_image"));

                foreach (var file2 in AllFilesInFolder)
                {
                    // Once a texture file has been found, locate the appropriate audio file for that environment, and send both to the coroutine to be loaded.
                    // Mandatory for texture and audio files to have the same name, followed by either "_image" or "_audio", followed by the file extension.
                    if (!file2.Name.Contains("meta") && !file2.Name.Contains("json") && !file2.Name.Contains("image"))
                    {                        
                        if (file2.Name.Contains(fileNameToCheck))
                        {
                            StartCoroutine(LoadEnvironmentFiles(file.FullName, file2.FullName));
                        }
                    }
                }                
            }            
        }

    }    


    private void Update()
    {        
        if (DisplayingEnvironment)
        {
            CurrentEnvironmentDisplayTime += Time.deltaTime;

            if (CurrentEnvironmentDisplayTime >= EnvironmentDisplayTime)
            {
                DisplayingEnvironment = false;
                transform.parent.GetComponent<ExperimentRun>().UpdateExperimentState();                
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
        string wwwTextureFilePath = "file://" + textureFileName;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(wwwTextureFilePath);
        yield return webRequest.SendWebRequest();
        Texture2D importedTexture = DownloadHandlerTexture.GetContent(webRequest);

        string wwwAudioFilePath = "file://" + audioClipFileName;
        webRequest = UnityWebRequestMultimedia.GetAudioClip(wwwAudioFilePath, AudioType.WAV);
        yield return webRequest.SendWebRequest();
        AudioClip importedAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        // A slightly hacky approach to get the environment file name that is common between image and audio files.
        string commonFileName = textureFileName.Substring(0, textureFileName.IndexOf("_image"));
        commonFileName = commonFileName.Substring(commonFileName.IndexOf("Environments\\") + 13);

        LoadedEnvironments.Add(new EnvironmentData(commonFileName, importedTexture, importedAudioClip));

        LoadedEnvironmentsCount++;

        Debug.Log("JUST LOADED: " + commonFileName);

        if (LoadedEnvironmentsCount == EnvironmentsCount)
        {
            SetupEnvDict();
        }
    }


    void SetupEnvDict()
    {
        EnvironmentsInDisplayOrder = new Dictionary<int, EnvironmentData>();

        foreach (EnvironmentData envData in LoadedEnvironments)
        {
            //Debug.Log("CAAT");
            Debug.Log("ADDING " + envData.envName);
            EnvironmentsInDisplayOrder.Add(LoadedEnvsOrder[envData.envName], envData);
        }

        CurrentEnvironmentIndex = 0;

        Debug.Log("FINISHED SETTING UP");

        // Informing the Player object that environments files have been loaded
        transform.parent.GetComponent<ExperimentRun>().UpdateExperimentState();

    }


    public void NextEnvironment(float timeToDisplay)
    {
        CurrentEnvironmentIndex++;
        if (CurrentEnvironmentIndex == EnvironmentsCount)
        {
            transform.parent.GetComponent<ExperimentRun>().NoMoreEnvironments();
        }

        SetTexture(EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envTexture);
        SetAudioClip(EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envAudioClip);

        EnvironmentDisplayTime = timeToDisplay;
        CurrentEnvironmentDisplayTime = 0f;
        DisplayingEnvironment = true;
    }


    private void SetTexture(Texture2D texture)
    {
        Material material = GetComponent<Renderer>().material;
        material.mainTexture = texture;

        float dimensionRatio = material.mainTexture.width / material.mainTexture.height;

        // Adjusting the material vertical scaling to make the texture look as intended.
        if (dimensionRatio > 4)
            material.mainTextureScale = new Vector2(1f, 3f);
        else
            material.mainTextureScale = new Vector2(1.5f, 3f);

        transform.GetChild(0).GetComponent<CameraMovementLC>().UpdateCameraRotationLimits(dimensionRatio);

        Debug.Log("CURRENTLY DISPLAYING: ");
        Debug.Log(GetCurrentEnvironmentName());
    }


    private void SetAudioClip(AudioClip audioClip)
    {
        AudioSource audioSource = transform.GetChild(0).GetComponent<AudioSource>(); 
        audioSource.clip = audioClip;
        //audioSource.clip.LoadAudioData();
        audioSource.Play();
    }


    public string GetCurrentEnvironmentName()
    {
        return EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envName;
    }


    public void ShowTransitionalEnvironment(float timeToDisplay)
    {
        // TODO
        GetComponent<Renderer>().material = TransitionalMaterial;
        AudioSource audioSource = transform.GetChild(0).GetComponent<AudioSource>();
        audioSource.Stop();

        EnvironmentDisplayTime = timeToDisplay;
        CurrentEnvironmentDisplayTime = 0f;
        DisplayingEnvironment = true;
    }


    public void ShowInstructionsEnvironment()
    {
        // TODO
        ShowTransitionalEnvironment(5);
    }


    public void ShowEndingEnvironment()
    {
        // TODO
        ShowTransitionalEnvironment(60);
    }

}

public class EnvironmentData
{
    public string envName;
    public Texture2D envTexture;
    public AudioClip envAudioClip;

    public EnvironmentData()
    {
        envName = "";
        envTexture = null;
        envAudioClip = null;
    }

    public EnvironmentData(string n, Texture2D t, AudioClip a)
    {
        envName = n;
        envTexture = t;
        envAudioClip = a;
    }

}