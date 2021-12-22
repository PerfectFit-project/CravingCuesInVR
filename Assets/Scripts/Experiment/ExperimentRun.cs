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

        //Camera.main.stereoTargetEye = StereoTargetEyeMask.None;
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
    }


    void TakeAction()
    {
        switch (CurrentExperimentState)
        {
            case (short)ExperimentState.StartExperiment:
                StartExperiment();
                break;
            case (short)ExperimentState.ShowLogin:
                ShowLogin();
                break;
            case (short)ExperimentState.ShowInstructions:
                ShowInstructions();
                break;
            case (short)ExperimentState.ShowEnvironment:
                ShowEnvironment();
                break;
            case (short)ExperimentState.ShowQuestionnaire:
                ShowQuestionnaire();
                break;
            case (short)ExperimentState.ShowTransitionalEnvironment:
                ShowTransitionalEnvironment();
                break;
            case (short)ExperimentState.ShowEnding:
                ShowEnding();
                break;
        }
    }

    public void NextExperimentState()
    {
        switch (CurrentExperimentState)
        {
            case (short)ExperimentState.StartExperiment:
                CurrentExperimentState = (short)ExperimentState.ShowLogin;
                break;
            case (short)ExperimentState.ShowLogin:
                CurrentExperimentState = (short)ExperimentState.ShowInstructions;
                break;
            case (short)ExperimentState.ShowInstructions:
                CurrentExperimentState = (short)ExperimentState.ShowEnvironment;
                break;
            case (short)ExperimentState.ShowEnvironment:
                CurrentExperimentState = (short)ExperimentState.ShowQuestionnaire;
                break;
            case (short)ExperimentState.ShowQuestionnaire:
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
                CurrentExperimentState = (short)ExperimentState.ShowEnvironment;
                break;
            case (short)ExperimentState.ShowEnding:
                CurrentExperimentState = (short)ExperimentState.EndExperiment;
                break;                
        }
    }


    void StartExperiment()
    {
        // Wait for environments to be loaded.        
        LoadingScreenCanvas.SetActive(true);

    }

    void ShowLogin()
    {
        LoadingScreenCanvas.SetActive(false);
        // Show login screen
        LoginCanvas.SetActive(true);
    }

    void ShowEnvironment()
    {
        // Show the appropriate environment based on the order defined in the JSON file.
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().NextEnvironment(TimePerCueEnv);
        QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().PresentRemainingTimer(TimePerCueEnv);
    }

    void ShowQuestionnaire()
    {
        if (NoEnvironmentsLeft)
        {
            QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().LastEnvironment = true;
        }


        StartCoroutine(QuestionnaireCanvasParent.transform.GetChild(0).GetComponent<LocalExperimentUIBehavior>().StartQuestionnairePresentation(1f));
    }

    void ShowInstructions()
    {
        Camera.main.stereoTargetEye = StereoTargetEyeMask.Both;
        PanoramaCameraObj.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = true;
        PanoramaCameraObj.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>().enabled = true;
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowInstructionsEnvironment();
    }

    void ShowTransitionalEnvironment()
    {
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowTransitionalEnvironment(TimePerTransitionEnv);

    }

    void ShowEnding()
    {
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowEndingEnvironment();
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
        LoginCanvas.SetActive(false);
        UserName = userName;
        UpdateExperimentState();
    }


    public void UpdateExperimentState()
    {
        UpdateState = !UpdateState;
    }

    public void NoMoreEnvironments()
    {
        NoEnvironmentsLeft = true;
    }
}