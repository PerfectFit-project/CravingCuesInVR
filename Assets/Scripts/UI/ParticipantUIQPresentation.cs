using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using Unity.Netcode;



public class ParticipantUIQPresentation : MonoBehaviour
{
    public GameObject SliderObjPrefab;
    public GameObject ChatLogSVContent;
    public GameObject ResponseMessageButtonPrefab;

    GameObject submitButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PresentQuestionnaire()
    {
        Questionnaire q = JsonUtility.FromJson<Questionnaire>(File.ReadAllText(Application.streamingAssetsPath + "/questionnaire.json"));
        LoadQuestionnaireToUI(q);
    }

    void LoadQuestionnaireToUI(Questionnaire questionnaire)
    {
        foreach (QQuestion question in questionnaire.qQuestions)
        {
            GameObject newQuestionObject = Instantiate(SliderObjPrefab, ChatLogSVContent.transform);
            newQuestionObject.transform.GetChild(0).GetComponent<TMP_Text>().text = question.descriptionText;
            newQuestionObject.transform.GetChild(1).GetComponent<TMP_Text>().text = question.questionText;

            Slider newQObjSLider = newQuestionObject.transform.GetChild(2).transform.GetChild(0).GetComponent<Slider>();
            newQObjSLider.minValue = int.Parse(question.acceptableResponseRange[0]);
            newQObjSLider.maxValue = int.Parse(question.acceptableResponseRange[1]);
            newQObjSLider.value = Mathf.RoundToInt(((newQObjSLider.maxValue + newQObjSLider.minValue) / 2));

            newQuestionObject.transform.GetChild(2).transform.GetChild(1).transform.GetChild(0).GetComponent<TMP_Text>().text = question.extremeRangeLabels[0];
            newQuestionObject.transform.GetChild(2).transform.GetChild(1).transform.GetChild(1).GetComponent<TMP_Text>().text = question.extremeRangeLabels[1];
            newQuestionObject.transform.GetChild(2).transform.GetChild(1).transform.GetChild(2).GetComponent<TMP_Text>().text = question.extremeRangeLabels[2];

        }

        submitButton = Instantiate(ResponseMessageButtonPrefab, ChatLogSVContent.transform);
        submitButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Submit Responses";
        submitButton.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        submitButton.GetComponent<Button>().onClick.AddListener(() => SaveQuestionnaireDataToFile());

        transform.GetComponent<ChatMessagesGamepadInteraction>().InitializeQuestionnaireControl();
    }

    void SaveQuestionnaireDataToFile()
    {
        submitButton.GetComponent<Button>().interactable = false;

        Dictionary<int, int> qResponses = new Dictionary<int, int>();

        for (int i = 0; i < ChatLogSVContent.transform.childCount; i++)
        {
            Slider selectedSlider;

            if (ChatLogSVContent.transform.GetChild(i).GetComponentInChildren<Slider>())
            {
                selectedSlider = ChatLogSVContent.transform.GetChild(i).transform.GetChild(2).transform.GetChild(0).GetComponent<Slider>();
            }
            else continue;

            // Hardcoding that the Slider object is the second child of the composite slider object
            qResponses[i] = (int)selectedSlider.value;
        }

        bool dataSaveSuccess = false;
        try
        {
            transform.GetComponent<SaveCollectedDataLC>().StoreDataToCollection("env", "", qResponses);
            dataSaveSuccess = true;
        }
        catch { }


        NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>().QSubmissionResponseServerRpc(dataSaveSuccess);

    }


}
