using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InstructionsUISimBehavior : MonoBehaviour
{
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
       

        if (objId == CorrectButtonId && ChatLogSVContent.transform.GetChild(CorrectSliderId).GetComponentInChildren<Slider>().value == CorrectSliderValue)
        {
            testPassed = true;

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
