using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsUISimBehavior : MonoBehaviour
{
    // Present questionnaire
    // Determine how to prompt the user to fill the questionnaire:
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

    public GameObject InstructionsUI;

    public GameObject ChatLogSVContent;
    public GameObject SliderObjPrefab;
    public GameObject ResponseMessageButtonPrefab;

    List<GameObject> ButtonsList;

    int CorrectButtonId;
    int CorrectSliderId;
    int CorrectSliderValue;

    void Start()
    {
        // Defining example questions
        Questionnaire questionnaire = new Questionnaire();
        questionnaire.qQuestions = new QQuestion[3];
        questionnaire.qQuestions[0] = new QQuestion("", "Slider A", new string[] { "1", "7" }, new string[] { "Strongly Disagree", "Strongly Agree" });
        questionnaire.qQuestions[1] = new QQuestion("", "Slider B", new string[] { "-5", "5" }, new string[] { "Strongly Disagree", "Strongly Agree" });
        questionnaire.qQuestions[2] = new QQuestion("", "Slider C", new string[] { "0", "10" }, new string[] { "Strongly Disagree", "Strongly Agree" });

        PresentQuestionnaire(questionnaire);

        GameObject newResponseButton = Instantiate(ResponseMessageButtonPrefab, ChatLogSVContent.transform);
        newResponseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Button A";
        newResponseButton.GetComponent<Button>().onClick.AddListener(() => ConfirmButtonSelection(1));
        newResponseButton.GetComponent<Button>().interactable = false;

        ButtonsList = new List<GameObject>();
        ButtonsList.Add(newResponseButton);

        newResponseButton = Instantiate(ResponseMessageButtonPrefab, ChatLogSVContent.transform);
        newResponseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Button B";
        newResponseButton.GetComponent<Button>().onClick.AddListener(() => ConfirmButtonSelection(2));
        newResponseButton.GetComponent<Button>().interactable = false;

        ButtonsList.Add(newResponseButton);

        newResponseButton = Instantiate(ResponseMessageButtonPrefab, ChatLogSVContent.transform);
        newResponseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Button C";
        newResponseButton.GetComponent<Button>().onClick.AddListener(() => ConfirmButtonSelection(3));
        newResponseButton.GetComponent<Button>().interactable = false;

        ButtonsList.Add(newResponseButton);

        LayoutRebuilder.ForceRebuildLayoutImmediate(ChatLogSVContent.transform.GetComponent<RectTransform>());

        transform.GetComponent<UIGamepadInteraction>().InitializeUI();
    }

    void PresentQuestionnaire(Questionnaire questionnaire)
    {
        foreach (QQuestion question in questionnaire.qQuestions)
        {
            GameObject newQuestionObject = Instantiate(SliderObjPrefab, ChatLogSVContent.transform);
            newQuestionObject.transform.GetChild(0).GetComponent<TMP_Text>().text = question.questionText;
            Slider newQObjSLider = newQuestionObject.transform.GetChild(2).transform.GetChild(0).GetComponent<Slider>();
            newQObjSLider.minValue = int.Parse(question.acceptableResponseRange[0]);
            newQObjSLider.maxValue = int.Parse(question.acceptableResponseRange[1]);
        }

        transform.GetComponent<UIGamepadInteraction>().GamepadActive = true;
    }

    
    public void ConfirmButtonSelection(int objId)
    {
        bool testPassed = false;
        Debug.Log("Correct Slider ID: " + CorrectSliderId + " Correct Slider Value: " + CorrectSliderValue);
        Debug.Log("Correct Slider Actual Value: " + ChatLogSVContent.transform.GetChild(CorrectSliderId).GetComponentInChildren<Slider>().value);
        Debug.Log("Correct Button ID: " + CorrectButtonId + " " + objId);
       

        if (objId == CorrectButtonId && ChatLogSVContent.transform.GetChild(CorrectSliderId).GetComponentInChildren<Slider>().value == CorrectSliderValue)
        {
            Debug.Log("YAY!");
            testPassed = true;

        }
        else
        {
            Debug.Log("NOPE :(");            
        }

        InstructionsUI.GetComponent<InstructionsController>().UIInteractionTestResult(testPassed);

    }


    public void DefineCorrectTestValues(int buttonId, int sliderId, int sliderVal)
    {
        CorrectButtonId = buttonId;
        CorrectSliderId = sliderId;
        CorrectSliderValue = sliderVal;
    }

    public void EnableButtonInteractivity()
    {
        foreach (GameObject button in ButtonsList)
        {
            button.GetComponent<Button>().interactable = !button.GetComponent<Button>().interactable;
        }

    }

    public void DisableButtonInteractivity()
    {
        foreach (GameObject button in ButtonsList)
        {
            button.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

}
