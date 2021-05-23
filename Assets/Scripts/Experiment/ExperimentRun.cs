using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


public class ExperimentRun : MonoBehaviour
{
    public GameObject LoginCanvas;
    public GameObject PanoramaCameraObj;
    public GameObject QuestionnaireCanvas;

    // In seconds
    public float TimePerCueEnv;
    public float TimePerTransitionEnv;

    short CurrentExperimentState;
    bool UpdateState;
    bool NoEnvironmentsLeft;

    private string UserName;

    // Start is called before the first frame update
    void Start()
    {
        NoEnvironmentsLeft = false;
        CurrentExperimentState = (short)ExperimentState.StartExperiment;
        UpdateState = true;
        // 1. Present login screen: prompt user to enter name/identifier
        // 2. Present brief instructions screen
        // 3. Present environment for a few minutes
        // 4. After timer finishes, prompt user to respond to questionnaire
        // 5. Upon submitting the questionnaire, present user with transitional environment for a few seconds
        // 6. Repeat from 3. until there are no more environments to present.
        // 7. Present ending environment (Maybe a thank you screen?)

    }

    // Update is called once per frame
    void Update()
    {
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
        // Show instructions
        CurrentExperimentState = (short)ExperimentState.ShowLogin;

    }

    void ShowLogin()
    {
        Debug.Log("Triggered: ShowLogin");
        // Show login screen
        LoginCanvas.SetActive(true);
    }

    void ShowEnvironment()
    {
        Debug.Log("Triggered: ShowEnvironment");
        // Show the appropriate environment
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().NextEnvironment(TimePerCueEnv);
    }

    void ShowQuestionnaire()
    {
        Debug.Log("Triggered: ShowQuestionnaire");
        QuestionnaireCanvas.SetActive(true);
        // Present the questionnaire

    }

    void ShowInstructions()
    {
        Debug.Log("Triggered: ShowInstructions");
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowInstructionsEnvironment();
    }

    void ShowTransitionalEnvironment()
    {
        Debug.Log("Triggered: ShowTransitionalEnvironment");
        QuestionnaireCanvas.SetActive(false);
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowTransitionalEnvironment(TimePerTransitionEnv);

    }

    void ShowEnding()
    {
        Debug.Log("Triggered: ShowEnding");
        QuestionnaireCanvas.SetActive(false);
        transform.GetChild(0).GetComponent<EnvironmentManagerLC>().ShowEndingEnvironment();

        transform.GetComponent<SaveCollectedDataLC>().SaveDataToFile(UserName);
    }

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