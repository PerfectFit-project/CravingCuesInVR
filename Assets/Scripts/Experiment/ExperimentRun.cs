using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SpatialTracking;

enum ExperimentState: short
{
    StartExperiment,
    ShowLogin,
    ShowInstructions,
    ShowEnvironment,
    ShowQuestionnaire,
    ShowTransitionalEnvironment,
    ShowEnding,
    EndExperiment
}

/// <summary>
/// Class housing the FSM that controls running our local experiment.
/// </summary>
public class ExperimentRun : MonoBehaviour
{
    //public GameObject Camera;
    public GameObject LoadingScreenCanvas;
    public GameObject LoginCanvas;
    public GameObject PanoramaCameraObj;
    public GameObject QuestionnaireCanvasParent;

    // In seconds
    public float TimePerCueEnv;
    public float TimePerTransitionEnv;

    short CurrentExperimentState;
    bool UpdateState;
    bool NoEnvironmentsLeft;

    public string UserName
    {
        get; private set;
    }

    void Start()
    {        
        NoEnvironmentsLeft = false;
        CurrentExperimentState = (short)ExperimentState.StartExperiment;
        UpdateState = false; 
        TakeAction();

        // 1. Present login screen: prompt user to enter name/identifier
        // 2. Present brief instructions screen
        // 3. Present environment for a few minutes
        // 4. After timer finishes, prompt user to respond to questionnaire
        // 5. Upon submitting the questionnaire, present user with transitional environment for a few seconds
        // 6. Repeat from 3. until there are no more environments to present.
        // 7. Present ending environment (Maybe a thank you screen?)
        Camera.main.stereoTargetEye = StereoTargetEyeMask.None;
    }

    void Update()
    {
        OVRInput.Update();

        if (UpdateState && CurrentExperimentState != (short)ExperimentState.EndExperiment)
        {
            UpdateState = false;
            NextExperimentState();
            TakeAction();
        }


        //OVRManager.HMDMounted += HandleHMDMounted;
        //OVRManager.HMDUnmounted += HandleHMDUnmounted;
    }

   
    
 
    void HandleHMDMounted()
    {
        // Do stuff
        Debug.Log("HMD MOUNTED");
    }

    void HandleHMDUnmounted()
    {
        // Do stuff
        Debug.Log("HMD UNMOUNTED");
    }

    void TakeAction()
    {
        switch (CurrentExperimentState)
        {
            case (short)ExperimentState.StartExperiment:
                Debug.Log("Triggered: TakeAction: StartExperiment");
                StartExperiment();
                break;
            case (short)ExperimentState.ShowLogin:
                Debug.Log("Triggered: TakeAction: ShowLogin");
                ShowLogin();
                break;
            case (short)ExperimentState.ShowInstructions:
                Debug.Log("Triggered: TakeAction: ShowInstructions");
                ShowInstructions();
                break;
            case (short)ExperimentState.ShowEnvironment:
                Debug.Log("Triggered: TakeAction: ShowEnvironment");
                ShowEnvironment();
                break;
            case (short)ExperimentState.ShowQuestionnaire:
                Debug.Log("Triggered: TakeAction: ShowQuestionnaire");
                ShowQuestionnaire();
                break;
            case (short)ExperimentState.ShowTransitionalEnvironment:
                Debug.Log("Triggered: TakeAction: ShowTransitionalEnvironment");
                ShowTransitionalEnvironment();
                break;
            case (short)ExperimentState.ShowEnding:
                Debug.Log("Triggered: TakeAction: ShowEnding");
                ShowEnding();
                break;
                //case (short)ExperimentState.EndExperiment:
                //    EndExperiment();
                //    break;
        }
    }

    public void NextExperimentState()
    {
        switch (CurrentExperimentState)
        {
            case (short)ExperimentState.StartExperiment:
                Debug.Log("Triggered: NextExperimentState: StartExperiment");
                CurrentExperimentState = (short)ExperimentState.ShowLogin;
                break;
            case (short)ExperimentState.ShowLogin:
                Debug.Log("Triggered: NextExperimentState: ShowLogin");
                CurrentExperimentState = (short)ExperimentState.ShowInstructions;
                break;
            case (short)ExperimentState.ShowInstructions:
                Debug.Log("Triggered: NextExperimentState: ShowInstructions");
                CurrentExperimentState = (short)ExperimentState.ShowEnvironment;
                break;
            case (short)ExperimentState.ShowEnvironment:
                Debug.Log("Triggered: NextExperimentState: ShowEnvironment");
                CurrentExperimentState = (short)ExperimentState.ShowQuestionnaire;
                break;
            case (short)ExperimentState.ShowQuestionnaire:
                Debug.Log("Triggered: NextExperimentState: ShowQuestionnaire");
                // If there are environments left to show, show transition, else show ending.
                if (!NoEnvironmentsLeft)
                {
                    CurrentExperimentState = (short)ExperimentState.ShowTransitionalEnvironment;
                }
                else
                {
                    CurrentExperimentState = (short)ExperimentState.ShowEnding;
                }
                break;
            case (short)ExperimentState.ShowTransitionalEnvironment:
                Debug.Log("Triggered: NextExperimentState: ShowTransitionalEnvironment");
                CurrentExperimentState = (short)ExperimentState.ShowEnvironment;
                break;
            case (short)ExperimentState.ShowEnding:
                Debug.Log("Triggered: NextExperimentState: ShowEnding");
                CurrentExperimentState = (short)ExperimentState.EndExperiment;
                break;                
        }
    }


    void StartExperiment()
    {
        Debug.Log("Triggered: StartExperiment");
        // Wait for environments to be loaded.
        
        LoadingScreenCanvas.SetActive(true);
        // Show instructions
        //CurrentExperimentState = (short)ExperimentState.ShowLogin;

    }

    void ShowLogin()
    {
        LoadingScreenCanvas.SetActive(false);
        Debug.Log("Triggered: ShowLogin");
        // Show login screen
        LoginCanvas.SetActive(true);
    }

    void ShowEnvironment()
    {
        Debug.Log("Triggered: ShowEnvironment");
        // Show the appropriate environment
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().NextEnvironment(TimePerCueEnv);
        QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().PresentRemainingTimer(TimePerCueEnv);
    }

    void ShowQuestionnaire()
    {
        Debug.Log("Triggered: ShowQuestionnaire");
        //QuestionnaireCanvasParent.SetActive(true);

        if (NoEnvironmentsLeft)
        {
            QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().LastEnvironment = true;
        }

        //StartCoroutine(StartQuestionnairePresentation(1f));

        StartCoroutine(QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().StartQuestionnairePresentation(1f));

        //QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().ToggleQuestionnaire();

        //QuestionnaireCanvasParent.transform.GetComponent<MeshRenderer>().enabled = !QuestionnaireCanvasParent.transform.GetComponent<MeshRenderer>().enabled;
        //QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<Canvas>().enabled = !QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<Canvas>().enabled;
        

    }

    //public IEnumerator StartQuestionnairePresentation(float seconds)
    //{        
    //    if (Gamepad.current != null)
    //    {
    //        Gamepad.current.SetMotorSpeeds(0.25f, 0.75f);
    //    }
    //    PanoramaCameraObj.transform.parent.GetComponent<AudioSource>().Play();
    //    yield return new WaitForSeconds(seconds);

    //    if (Gamepad.current != null)
    //    {
    //        Gamepad.current.SetMotorSpeeds(0f, 0f);
    //    }
    //    QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().ToggleQuestionnaire();

    //}

    void ShowInstructions()
    {
        Debug.Log("Triggered: ShowInstructions");
        Camera.main.stereoTargetEye = StereoTargetEyeMask.Both;
        PanoramaCameraObj.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = true;
        PanoramaCameraObj.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().enabled = true;
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowInstructionsEnvironment();
    }

    void ShowTransitionalEnvironment()
    {
        Debug.Log("Triggered: ShowTransitionalEnvironment");
        //QuestionnaireCanvasParent.SetActive(false);
        //QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().ToggleQuestionnaire();
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowTransitionalEnvironment(TimePerTransitionEnv);

    }

    void ShowEnding()
    {
        Debug.Log("Triggered: ShowEnding");
        //QuestionnaireCanvasParent.SetActive(false);
        QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().ToggleQuestionnaire();
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowEndingEnvironment();

        //transform.GetComponent<SaveCollectedDataLC>().SaveDataToFile(UserName);
    }


    /*
     * Pseudo-Observers
     * 
     * Unity allows objects to interact with each other by providing references via the GUI. 
     * So instead of manually defining observers for when different things happen in other objects,
     * we can simply have the object in question call the relevant method when the appropriate event is triggered.
     */

    public void LogUserIn(string userName)
    {
        Debug.Log("Triggered: LogUserIn");
        LoginCanvas.SetActive(false);
        UserName = userName;
        //do relevant stuff with user name
        UpdateExperimentState();
    }


    public void UpdateExperimentState()
    {
        Debug.Log("Triggered: UpdateExperimentState");
        UpdateState = !UpdateState;
    }

    public void NoMoreEnvironments()
    {
        Debug.Log("Triggered: NoMoreEnvironments");
        NoEnvironmentsLeft = true;
    }
}