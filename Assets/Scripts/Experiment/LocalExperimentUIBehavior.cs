using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class LocalExperimentUIBehavior : MonoBehaviour
{
    // Present questionnaire
    // Deternube how to prompt the user to fill the questionnaire:
    //      1. Give them a notification and let them access the simulated smartphone of their own volition
    //      2. End the current environment interaction and present them with the simulated smartphone to use.
    //  Probably 2. since we want this to be a controlled experiment, and as such, we want to have control over how much exposure to the environment participants have.
    // 
    // Determine whether we want to present the questionnaires:
    //      1. In a neutral (transitive) environment between environments.
    //      2. Disable camera controls and focus the simulated smartphone in the same environment.
    //
    // Determine how to ask questions:    
    //      1. All questions on a continuous scrolling panel
    //      2. Different pages for each type of question (craving, SoP, or even different pages for each factor in those)

    public GameObject PlayerObj;
    public GameObject ChatScrollView;
    public GameObject ChatLogSVContent;
    public GameObject SliderObjPrefab;
    public GameObject SubmitButton;



    // Start is called before the first frame update
    void Start()
    {
        // Load questionnaires from file


        // Prentending as if we have questionnaires
        Questionnaire questionnaire = new Questionnaire();
        questionnaire.qQuestions = new QQuestion[8];
        questionnaire.qQuestions[0] = new QQuestion("This is the first sample question", new int[] { 1, 7 });
        questionnaire.qQuestions[1] = new QQuestion("This is the second sample question", new int[] { -5, 5 });
        questionnaire.qQuestions[2] = new QQuestion("This is the third sample question", new int[] { 0, 10 });
        questionnaire.qQuestions[3] = new QQuestion("This is the fourth sample question", new int[] { 1, 7 });
        questionnaire.qQuestions[4] = new QQuestion("This is the fifth sample question", new int[] { 1, 7 });
        questionnaire.qQuestions[5] = new QQuestion("This is the sixth sample question", new int[] { -5, 5 });
        questionnaire.qQuestions[6] = new QQuestion("This is the seventh sample question", new int[] { 0, 10 });
        questionnaire.qQuestions[7] = new QQuestion("This is the eighth sample question", new int[] { 1, 7 });

        PresentQuestionnaire(questionnaire);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PresentQuestionnaire(Questionnaire questionnaire)
    {
        foreach (QQuestion question in questionnaire.qQuestions)
        {
            GameObject newQuestionObject = Instantiate(SliderObjPrefab, ChatLogSVContent.transform);
            newQuestionObject.transform.GetChild(0).GetComponent<Text>().text = question.questionText;
            Slider newQObjSLider = newQuestionObject.transform.GetChild(1).GetComponent<Slider>();
            newQObjSLider.minValue = question.acceptableResponseRange[0];
            newQObjSLider.maxValue = question.acceptableResponseRange[1];
        }

        SubmitButton.transform.SetAsLastSibling();
    }

    public void RecordQuestionnaireResponses()
    {
        // Adjusting the size to account for the submit button being a child of the panel too.
        int questionCount = ChatLogSVContent.transform.childCount - 1;
        //int[] qResponses = new int[questionCount];

        // Seems a bit unnecessary to have an <int, int> dictionary here since a simple array or list would suffice, however, this makes it easier to add to the overall questionnare responses file later.
        Dictionary<int, int> qResponses = new Dictionary<int, int>();

        Debug.Log(questionCount);
        for (int i = 0; i < questionCount; i++)
        {
            // Hardcoding that the Slider object is the second child of the composite slider object
            qResponses[i+1] = (int)ChatLogSVContent.transform.GetChild(i).transform.GetChild(1).GetComponent<Slider>().value;
            // Resetting the Slider value
            ChatLogSVContent.transform.GetChild(i).transform.GetChild(1).GetComponent<Slider>().value = ChatLogSVContent.transform.GetChild(i).transform.GetChild(1).GetComponent<Slider>().minValue;
        }

        // Moving the page to the top
        ChatScrollView.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

        //foreach (int response in qResponses.Keys)
        //{
        //    Debug.Log("Question: " + response + ", Response: " + qResponses[response]);
        //}

        string currentEnvironmentName = transform.parent.GetComponentInChildren<EnvironmentManagerLC>().GetCurrentEnvironmentName();

        PlayerObj.GetComponent<SaveCollectedDataLC>().StoreDataToCollection(currentEnvironmentName, qResponses);

        PlayerObj.GetComponent<ExperimentRun>().UpdateExperimentState();

    }
    
}
