using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;

/// <summary>
/// Class that loads environment files from streamingassets, and presents them when called for the specified amount of time.
/// </summary>
public class EnvironmentManagerLC : MonoBehaviour
{
    public Camera PanoramaCamera;
    public GameObject ManuallyAdvanceEnvObj;

    public Material CueEnvironmentMaterial;
    public Material TransitionalMaterial;

    private FileInfo[] AllFilesInFolder;

    Dictionary<string, int> LoadedEnvsOrder;
    private List<EnvironmentData> LoadedEnvironments;
    public Dictionary<int, EnvironmentData> EnvironmentsInDisplayOrder;
    
    private int CurrentEnvironmentIndex;
    private int EnvironmentsCount;
    private int LoadedEnvironmentsCount;

    // In seconds
    private float EnvironmentDisplayTime;
    private float CurrentEnvironmentDisplayTime;

    [SerializeField]
    public bool DisplayingEnvironment { get; private set; }

    [SerializeField]
    public bool DisplayingTransitionalEnvironment { get; private set; }


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
                string fileNameToCheck = file.Name.Substring(0, file.Name.IndexOf("_image"));

                if (!LoadedEnvsOrder.ContainsKey(fileNameToCheck))
                {
                    continue;
                }

                foreach (var file2 in AllFilesInFolder)
                {
                    // Once a texture file has been found, locate the appropriate audio file for that environment, and send both to the coroutine to be loaded.
                    // Using the String Contains method, which will return true if the name of a file is a subset of another. Make sure that file names are distinct enough.
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

        DisplayingEnvironment = false;
        DisplayingTransitionalEnvironment = false;
    }


    private void Update()
    {        
        // Display an environment for the amount of time specified.
        if (DisplayingEnvironment)
        {
            CurrentEnvironmentDisplayTime += Time.deltaTime;

            if (CurrentEnvironmentDisplayTime >= EnvironmentDisplayTime)
            {
                if (!DisplayingTransitionalEnvironment)
                {
                    DisplayingEnvironment = false;
                    transform.parent.GetComponent<ExperimentRun>().UpdateExperimentState();
                }
                else
                {
                    ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
                    ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(true);
                    ManuallyAdvanceEnvObj.GetComponent<ExitTransitionalEnvironment>().ReadyToProceed = true;
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

        if (LoadedEnvironmentsCount == EnvironmentsCount)
        {
            SetupEnvDict();
        }
    }


    /// <summary>
    /// Sets up the dictionary holding loaded environments along with their order to be presented.
    /// </summary>
    void SetupEnvDict()
    {
        EnvironmentsInDisplayOrder = new Dictionary<int, EnvironmentData>();

        foreach (EnvironmentData envData in LoadedEnvironments)
        {
            if (LoadedEnvsOrder.ContainsKey(envData.envName))
            {
                EnvironmentsInDisplayOrder.Add(LoadedEnvsOrder[envData.envName], envData);
            }
            
        }

        CurrentEnvironmentIndex = 0;

        // Informing the Player object that environments files have been loaded
        transform.parent.GetComponent<ExperimentRun>().UpdateExperimentState();

    }

    /// <summary>
    /// Present the next environment
    /// </summary>
    /// <param name="timeToDisplay"></param>
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
        // Slightly hacky implementation of having the baseline measurements environment have the questionnaire pop-up immediately, rather than implementing a special type of environment and FMS state just for that.
        if (EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envName.Contains("Baseline"))
        {
            EnvironmentDisplayTime = 1;
        }
        CurrentEnvironmentDisplayTime = 0f;
        DisplayingEnvironment = true;

    }

    /// <summary>
    /// Sets the given texture on the material used by the Renderer of the Gameobject this script is attached to.
    /// </summary>
    /// <param name="texture"></param>
    private void SetTexture(Texture2D texture)
    {
        Material material = GetComponent<Renderer>().material;
        material.mainTexture = texture;
    }

    /// <summary>
    /// Assigns the given AudioClip to the AudioSource on the Camera, and plays it.
    /// </summary>
    /// <param name="audioClip"></param>
    private void SetAudioClip(AudioClip audioClip)
    {
        AudioSource audioSource = PanoramaCamera.GetComponent<AudioSource>(); 
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    /// <summary>
    /// Returns the name of the environment being displayed.
    /// </summary>
    /// <returns></returns>
    public string GetCurrentEnvironmentName()
    {
        return EnvironmentsInDisplayOrder[CurrentEnvironmentIndex].envName;
    }

    /// <summary>
    /// Present the transitional environment.
    /// </summary>
    /// <param name="timeToDisplay"></param>
    public void ShowTransitionalEnvironment(float timeToDisplay)
    {
        GetComponent<Renderer>().material = TransitionalMaterial;
        AudioSource audioSource = PanoramaCamera.GetComponent<AudioSource>();
        audioSource.Stop();

        EnvironmentDisplayTime = timeToDisplay;
        CurrentEnvironmentDisplayTime = 0f;
        DisplayingEnvironment = true;
        DisplayingTransitionalEnvironment = true;
        ManuallyAdvanceEnvObj.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.GetComponent<ExitTransitionalEnvironment>().ReadyToProceed = false;
    }

    /// <summary>
    /// Stop presenting the transitional environment
    /// </summary>
    public void ExitTransitionalEnvironment()
    {
        DisplayingEnvironment = false;
        DisplayingTransitionalEnvironment = false;
        ManuallyAdvanceEnvObj.SetActive(false);
        transform.parent.GetComponent<ExperimentRun>().UpdateExperimentState();
    }

    /// <summary>
    /// Present the welcome message and basic instructions using the transitional environment.
    /// </summary>
    public void ShowInstructionsEnvironment()
    {
        GetComponent<Renderer>().material = TransitionalMaterial;
        AudioSource audioSource = PanoramaCamera.GetComponent<AudioSource>();
        audioSource.Stop();
                
        ManuallyAdvanceEnvObj.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.GetComponent<TMP_Text>().text = "Hello, this experiment will have you experience virtual environments and then  answer a few questions.\n\n Please put on the head-mounted display and press the A button on your gamepad controller to begin.";

        ManuallyAdvanceEnvObj.GetComponent<ExitTransitionalEnvironment>().ReadyToProceed = true;
    }

    /// <summary>
    /// Present the ending message using the transitional environment.
    /// </summary>
    public void ShowEndingEnvironment()
    {
        GetComponent<Renderer>().material = TransitionalMaterial;
        AudioSource audioSource = PanoramaCamera.GetComponent<AudioSource>();
        audioSource.Stop();

        ManuallyAdvanceEnvObj.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(1).gameObject.SetActive(false);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.SetActive(true);
        ManuallyAdvanceEnvObj.transform.GetChild(0).GetChild(0).GetChild(2).gameObject.GetComponent<TMP_Text>().text = "This concludes our experiment, thank you for participating!\n You may now remove the head-mounted display.";

        ManuallyAdvanceEnvObj.GetComponent<ExitTransitionalEnvironment>().ReadyToProceed = false;
    }

}

/// <summary>
/// Class holding environment data, including name, texture, and audio clip.
/// </summary>
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