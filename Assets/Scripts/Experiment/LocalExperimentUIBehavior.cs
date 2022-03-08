using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;


public class LocalExperimentUIBehavior : MonoBehaviour
{
    public GameObject PlayerObj;
    public GameObject SphereObj;

    public GameObject ChatScrollView;
    public GameObject ChatLogSVContent;
    public GameObject QDescriptionText;
    public GameObject SliderObjPrefab;
    public GameObject SubmitButton;

    public GameObject FlowingTextCanvas;
    public GameObject FlowingTextText;

    [SerializeField]
    public bool PresentingQuestionnaire { get; set; }
    
    [SerializeField]
    public bool TimerActive { get; set; }

    Dictionary<string, string> qJsonFileData;
    List<string> qOrderList;
    string currentQId;
    int questionnairesPresented;
    float currentEnvironmentDisplayTime;
    float timeToPresentEnvironment;

    Dictionary<string, List<string>> NotificationTextDict;
    private bool MoreNotificationsToDisplay;
    private string NextNotificationKey;
    private float NextNotificationTime;
    private float NotificationDisplayStartTime;
    private bool DisplayingNotification;

    public int SecondsToDisplayNotification;

    [SerializeField]
    public bool LastEnvironment { get; set; }

    void Start()
    {
        qOrderList = new List<string>();
        qOrderList.Add("Craving");
        qJsonFileData = new Dictionary<string, string>();

        // Load questionnaires from file
        qJsonFileData.Add(qOrderList[0], File.ReadAllText(Application.streamingAssetsPath + "/craving_questionnaire.json"));
  
        questionnairesPresented = 0;
        currentQId = qOrderList[questionnairesPresented];
        Questionnaire firstQuestionnaire = JsonUtility.FromJson<Questionnaire>(qJsonFileData[currentQId]);
        LoadQuestionnaireToUI(firstQuestionnaire);

        transform.GetComponent<UIGamepadInteraction>().InitializeUI();

        PresentingQuestionnaire = false;
        TimerActive = false;

        var notificationPromptsJsonString = File.ReadAllText(Application.streamingAssetsPath + "/notification_text.json");
        NotificationTextDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(notificationPromptsJsonString);

        NextNotificationKey = "1";
        NextNotificationTime = float.Parse(NotificationTextDict[NextNotificationKey][1]);
        MoreNotificationsToDisplay = true;

    }

    private void Update()
    {        
        if (TimerActive)
        {
            if (!PresentingQuestionnaire)
            {
                currentEnvironmentDisplayTime += Time.deltaTime;

                float minutesLeft = Mathf.Floor((timeToPresentEnvironment - currentEnvironmentDisplayTime) / 60);
                float secondsLeft = Mathf.RoundToInt((timeToPresentEnvironment - currentEnvironmentDisplayTime) % 60);

                string timerText = "";

                if (minutesLeft >= 1)
                {
                    timerText += minutesLeft + " minutes\n";
                }

                timerText += secondsLeft + " seconds";

                transform.GetChild(1).transform.GetChild(1).GetComponent<TMP_Text>().text = timerText; 

                if (currentEnvironmentDisplayTime >= timeToPresentEnvironment)
                {
                    if (transform.GetChild(1).gameObject.activeSelf)
                    {
                        ToggleTimer();
                    }

                    TimerActive = false;
                }
            }
            else
            {
                TimerActive = false;
            }            
        }


        if (MoreNotificationsToDisplay && !DisplayingNotification && !PresentingQuestionnaire)
        {            
            if (currentEnvironmentDisplayTime >= NextNotificationTime)
            {
                NotificationDisplayStartTime = currentEnvironmentDisplayTime;
                DisplayingNotification = true;
                //Display notification
                StartCoroutine(PresentNotification(1f, NotificationTextDict[NextNotificationKey][0]));
            }
        }

        if (DisplayingNotification)
        {
            if (currentEnvironmentDisplayTime >= NotificationDisplayStartTime + SecondsToDisplayNotification)
            {
                // Hide Notification
                transform.GetChild(1).GetChild(2).gameObject.SetActive(false);
                if (transform.GetChild(1).gameObject.activeSelf)
                {
                    HandleUIToggle();
                }

                // Advance Notification Key
                NextNotificationKey = (int.Parse(NextNotificationKey) + 1).ToString();

                // Check if more notifications
                if (NotificationTextDict.ContainsKey(NextNotificationKey))
                {
                    NextNotificationTime = float.Parse(NotificationTextDict[NextNotificationKey][1]);
                }
                else
                {
                    MoreNotificationsToDisplay = false;
                }

                DisplayingNotification = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.LeftShift))
        {
            StartCoroutine(FadeTextToFullAlpha(1f, FlowingTextText.GetComponent<TMP_Text>()));
        }
        if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftShift))
        {
            StartCoroutine(FadeTextToZeroAlpha(1f, FlowingTextText.GetComponent<TMP_Text>()));
        }


        if (Input.GetKeyDown(KeyCode.T) && Input.GetKey(KeyCode.LeftShift))
        {
            StartCoroutine(PresentNotification(1f, "Text Text"));
        }

        if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
        {
            HandleUIToggle();
        }
    }


    public IEnumerator PresentNotification(float seconds, string text)
    {
        transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
        transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = text;

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0.25f, 0.75f);
        }
        transform.parent.parent.parent.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(seconds);

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        if (!transform.GetChild(1).gameObject.activeSelf)
        {
            HandleUIToggle();
        }
    }


    public IEnumerator FadeTextToFullAlpha(float t, TMP_Text i)
    {
        FlowingTextText.SetActive(true);
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }

        FlowingTextText.transform.parent.parent.parent.GetComponent<MeshRenderer>().enabled = true;

    }

    public IEnumerator FadeTextToZeroAlpha(float t, TMP_Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
        FlowingTextText.transform.parent.parent.parent.GetComponent<MeshRenderer>().enabled = false;
        FlowingTextText.SetActive(false);
    }


    public void HandleUIToggle()
    {
        if (SphereObj.GetComponent<EnvironmentManagerLC>().DisplayingEnvironment && !SphereObj.GetComponent<EnvironmentManagerLC>().DisplayingTransitionalEnvironment && !PresentingQuestionnaire)//!SphereObj.GetComponent<EnvironmentManagerLC>().DisplayingBaselineEnvironment)
        {
            ToggleUI();
            if (TimerActive)
            {
                ToggleTimer();
            }
            else
            {
                ToggleQuestionnaire();
            }
        }
    }

    public void PresentRemainingTimer(float t)
    {
        currentEnvironmentDisplayTime = 0f;
        timeToPresentEnvironment = t;
        TimerActive = true;

        NextNotificationKey = "1";
        NextNotificationTime = float.Parse(NotificationTextDict[NextNotificationKey][1]);
        MoreNotificationsToDisplay = true;
    }

    public void ToggleTimer()
    {
        transform.GetChild(1).gameObject.SetActive(!transform.GetChild(1).gameObject.activeSelf); // Notifications panel obj
        
    }
    public IEnumerator StartQuestionnairePresentation(float seconds)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0.25f, 0.75f);
        }
        transform.parent.parent.parent.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(seconds);

        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
        ToggleQuestionnaire();
    }



    void ToggleQuestionnairePresentationSS()
    {
        // This gets called automatically at a set time, so we check whether the UI is enabled so that we don't disable it.
        if (!transform.parent.GetComponent<MeshRenderer>().enabled)
        {
            ToggleUI();
        }        

        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf); // Chat interface panel        

    }

    public void ToggleQuestionnaire()
    {
        // This gets called automatically at a set time, so we check whether the UI is enabled so that we don't disable it.
        if (!transform.parent.GetComponent<MeshRenderer>().enabled && !PresentingQuestionnaire)
        {
            ToggleUI();
        }

        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf); // Chat interface panel

        transform.GetComponent<UIGamepadInteraction>().GamepadActive = !transform.GetComponent<UIGamepadInteraction>().GamepadActive;
        PresentingQuestionnaire = !PresentingQuestionnaire;
    }

    public void ToggleUI()
    {
        transform.parent.GetComponent<MeshRenderer>().enabled = !transform.parent.GetComponent<MeshRenderer>().enabled; // Smartphone obj
        transform.GetComponent<Canvas>().enabled = !transform.GetComponent<Canvas>().enabled; // Canvas
        transform.parent.GetChild(1).GetComponent<MeshRenderer>().enabled = !transform.parent.GetChild(1).GetComponent<MeshRenderer>().enabled; // Background obj
    }

    void LoadQuestionnaireToUI(Questionnaire questionnaire)
    {
        QDescriptionText.GetComponent<TMP_Text>().text = questionnaire.qTitle;

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

        SubmitButton.transform.SetAsLastSibling();
    }

    public void TranslateSubmitButtonPress()
    {
        questionnairesPresented++;

        RecordQuestionnaireResponses(currentQId);
    }

    public void RecordQuestionnaireResponses(string qId)
    {
        // Seems a bit unnecessary to have an <int, int> dictionary here since a simple array or list would suffice, but this makes it easier to add to the overall questionnare responses file later.
        Dictionary<int, int> qResponses = new Dictionary<int, int>();
                

        for (int i = 1; i < ChatLogSVContent.transform.childCount; i++)
        {
            Slider selectedSlider;

            if (ChatLogSVContent.transform.GetChild(i).GetComponentInChildren<Slider>())
            {
                selectedSlider = ChatLogSVContent.transform.GetChild(i).transform.GetChild(2).transform.GetChild(0).GetComponent<Slider>();
            }
            else continue;

            // Hardcoding that the Slider object is the second child of the composite slider object
            qResponses[i] = (int)selectedSlider.value;
            // Resetting the Slider value
            selectedSlider.value = selectedSlider.minValue;
        }

        // Moving the page to the top
        ChatScrollView.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

        string currentEnvironmentName = SphereObj.GetComponent<EnvironmentManagerLC>().GetCurrentEnvironmentName();

        PlayerObj.GetComponent<SaveCollectedDataLC>().StoreDataToCollection(currentEnvironmentName, qId, qResponses);

        if (LastEnvironment && questionnairesPresented < qOrderList.Count)
        {
            ClearAndRepopulateUI();
        }
        else
        {
            questionnairesPresented = 0;
            ClearAndRepopulateUI();
            ToggleQuestionnaire();
            ToggleUI();
            PlayerObj.GetComponent<ExperimentRun>().UpdateExperimentState();
        }      

    }


    public void ClearAndRepopulateUI()
    {
        int questionCount = ChatLogSVContent.transform.childCount - 1;

        while (ChatLogSVContent.transform.childCount > 2)
        {
            Destroy(ChatLogSVContent.transform.GetChild(1).gameObject);
            ChatLogSVContent.transform.GetChild(1).SetParent(null);
        }

        Canvas.ForceUpdateCanvases();

        transform.GetComponent<UIGamepadInteraction>().ClearUI();

        Canvas.ForceUpdateCanvases();

        currentQId = qOrderList[questionnairesPresented];
        Questionnaire nextQuestionnaire = JsonUtility.FromJson<Questionnaire>(qJsonFileData[currentQId]);

        LoadQuestionnaireToUI(nextQuestionnaire);
        transform.GetComponent<UIGamepadInteraction>().InitializeUI();

        Canvas.ForceUpdateCanvases();
    }
    
}
