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
        
        //Dictionary<string, int> loadedEnvOrder = JsonUtility.FromJson<Dictionary<string, int>>(jsonString);
        LoadedEnvsOrder = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);
        //Debug.Log("PRINTING LOADED ENV ORDER");
        //Debug.Log(loadedEnvOrder.Keys.Count);


        Debug.Log("PRINTING LOADED ENVIRONMENTS");
        foreach (string key in LoadedEnvsOrder.Keys)
        {
            Debug.Log(key + " " + LoadedEnvsOrder[key]);
        }

        EnvironmentsCount = LoadedEnvsOrder.Count;
        LoadedEnvironmentsCount = 0;

        // Loading files from StreamingAssets folder
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath + "/Environments");
        AllFilesInFolder = directoryInfo.GetFiles("*.*");

       LoadedEnvironments = new List<EnvironmentData>();
        
        
        foreach (var file in AllFilesInFolder)
        {
            // Avoid .meta and .json files, and files containing "audio" to prevent double loading
            if (!file.Name.Contains("meta") && !file.Name.Contains("json") && !file.Name.Contains("audio"))
            {
                //Debug.Log("TEXTURE FILE NAME: " + file.Name);
                string fileNameToCheck = file.Name.Substring(0, file.Name.IndexOf("_image"));

                foreach (var file2 in AllFilesInFolder)
                {
                    // Once a texture file has been found, locate the appropriate audio file for that environment, and send both to the coroutine to be loaded.
                    // Mandatory for texture and audio files to have the same name, followed by either "_image" or "_audio", followed by the file extension.
                    if (!file2.Name.Contains("meta") && !file2.Name.Contains("json") && !file2.Name.Contains("image"))
                    {                        
                        //Debug.Log("CHECKING FOR STRING: " + fileNameToCheck);
                        if (file2.Name.Contains(fileNameToCheck))
                        {
                            //Debug.Log("ABOUT TO SEND: " + fileNameToCheck + " TO BE LOADED");
                            //Debug.Log("AUDIO FILE NAME: " + file2.Name);
                            StartCoroutine(LoadEnvironmentFiles(file.FullName, file2.FullName));
                            //LoadEnvironmentFiles2(file.FullName, file2.FullName);
                        }
                    }
                }                
            } 
            
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

        //Debug.Log("ABOUT TO PRINT ALL ENV INFO");
        ////foreach (int i in EnvironmentsInDisplayOrder.Keys)
        ////{
        ////    Debug.Log("Environment: " + i);
        ////    Debug.Log("Texture Name: " + EnvironmentsInDisplayOrder[i].envTexture);
        ////    Debug.Log("AudioClip Name: " + EnvironmentsInDisplayOrder[i].envAudioClip);
        ////}

        //Debug.Log("Environment 1:");
        //Debug.Log("Name: " + EnvironmentsInDisplayOrder[1].envName);
        //Debug.Log("Texture: " + EnvironmentsInDisplayOrder[1].envTexture);
        //Debug.Log("Audio: " + EnvironmentsInDisplayOrder[1].envAudioClip);
        //Debug.Log("Environment 2:");
        //Debug.Log("Name: " + EnvironmentsInDisplayOrder[2].envName);
        //Debug.Log("Texture: " + EnvironmentsInDisplayOrder[2].envTexture);
        //Debug.Log("Audio: " + EnvironmentsInDisplayOrder[2].envAudioClip);
        //Debug.Log("Environment 3:");
        //Debug.Log("Name: " + EnvironmentsInDisplayOrder[3].envName);
        //Debug.Log("Texture: " + EnvironmentsInDisplayOrder[3].envTexture);
        //Debug.Log("Audio: " + EnvironmentsInDisplayOrder[3].envAudioClip);
        //Debug.Log("Environment 4:");
        //Debug.Log("Name: " + EnvironmentsInDisplayOrder[4].envName);
        //Debug.Log("Texture: " + EnvironmentsInDisplayOrder[4].envTexture);
        //Debug.Log("Audio: " + EnvironmentsInDisplayOrder[4].envAudioClip);

        Debug.Log("FINISHED SETTING UP");
    }

    private void Update()
    {        
        if (DisplayingEnvironment)
        {
            CurrentEnvironmentDisplayTime += Time.deltaTime;

            if (CurrentEnvironmentDisplayTime >= EnvironmentDisplayTime)
            {
                Debug.Log("ENVIRONMENT TIMER TRIGGER");
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

        string commonFileName = textureFileName.Substring(0, textureFileName.IndexOf("_image"));
        //Debug.Log("FILE NAME: " + commonFileName);
        commonFileName = commonFileName.Substring(commonFileName.IndexOf("Environments\\") + 13);
        //Debug.Log("FILE NAME: " + commonFileName);

        Debug.Log("CAME IN THE COROUTINE");

        LoadedEnvironments.Add(new EnvironmentData(commonFileName, importedTexture, importedAudioClip));

        LoadedEnvironmentsCount++;

        Debug.Log("JUST LOADED: " + commonFileName);

        //Debug.Log("LOADED: " + LoadedEnvironmentsCount + " SO FAR");

        if (LoadedEnvironmentsCount == EnvironmentsCount)
        {
            SetupEnvDict();
        }
    }

    void LoadEnvironmentFiles2(string textureFileName, string audioClipFileName)
    {
        string wwwTextureFilePath = "file://" + textureFileName;
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(wwwTextureFilePath);
        webRequest.SendWebRequest();
        Texture2D importedTexture = DownloadHandlerTexture.GetContent(webRequest);

        string wwwAudioFilePath = "file://" + audioClipFileName;
        webRequest = UnityWebRequestMultimedia.GetAudioClip(wwwAudioFilePath, AudioType.WAV);
        webRequest.SendWebRequest();
        AudioClip importedAudioClip = DownloadHandlerAudioClip.GetContent(webRequest);

        string commonFileName = textureFileName.Substring(0, textureFileName.IndexOf("_image"));

        LoadedEnvironments.Add(new EnvironmentData(commonFileName, importedTexture, importedAudioClip));

        //Debug.Log("Loaded Environment with Details: " + commonFileName + " " + importedTexture.name + " " + importedAudioClip.name);

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
    }

    private void SetAudioClip(AudioClip audioClip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
    }

    public void NextEnvironment(float timeToDisplay)
    {
        //if (CurrentEnvironmentIndex + 1 <= EnvironmentsCount)
        //{
        //    CurrentEnvironmentIndex++;
        //}
        //else
        //{
        //    // Telling the FSM that this is the last cue environment
        //    transform.parent.GetComponent<ExperimentRun>().NoMoreEnvironments();
        //}

        CurrentEnvironmentIndex++;
        if (CurrentEnvironmentIndex == EnvironmentsCount)
        {
            transform.parent.GetComponent<ExperimentRun>().NoMoreEnvironments();
        }


        //Debug.Log("About to set environment: " + CurrentEnvironmentIndex);
        //Debug.Log("ABOUT TO PRINT ENV IN DISP ORD");
        //foreach (int i in EnvironmentsInDisplayOrder.Keys)
        //{
            //Debug.Log(EnvironmentsInDisplayOrder[i]);
        //}
        //Debug.Log("PRINTED");
        SetTexture(EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envTexture);
        EnvironmentDisplayTime = timeToDisplay;
        CurrentEnvironmentDisplayTime = 0f;
        DisplayingEnvironment = true;
    }

    public void ShowTransitionalEnvironment(float timeToDisplay)
    {
        // TODO
        GetComponent<Renderer>().material = TransitionalMaterial;
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