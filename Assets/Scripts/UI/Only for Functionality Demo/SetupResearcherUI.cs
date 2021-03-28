using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// DEPRECATED: Chat log functionality abstracted and made redundant by the ChatLogBehavior script, which handles message creation and placement on chat log canvases. 
/// For use in the local Participant_Canvas objects, which are only for basic functionality demonstration purposes.
/// 
/// Implemented functionality for loading template messages and acceptable responses from a JSON file and presenting them in the UI.
/// Demonstrating functionality for adding objects on the chat log of the participant's UI. Some methods used only for demonstration purposes,
/// redundancies with the participant UI functionality, as well as some hard-coded variables will be refactored once networking is implemented. 
/// </summary>
public class SetupResearcherUI : MonoBehaviour
{
    public GameObject TemplateMessageSVContent;
    public GameObject TemplateMessageButtonPrefab;
    public GameObject MessageInputField;
    public GameObject MessageResponsesSVContent;
    public GameObject ResponseInputFieldPrefab;

    public GameObject ChatScrollView;
    public GameObject ChatLogSVContent;
    public GameObject ParticipantChatObjectPrefab;
    public GameObject ResearcherChatObjectPrefab;


    void Start()
    {
        PopulateResearcherUI(RetrieveChatMessagesFromJSON());      
    }

    /// <summary>
    /// Read JSON file of a specified structure corresponding with the ChatMessages class, and return a ChatMessages object.
    /// </summary>
    /// <returns></returns>
    ChatMessages RetrieveChatMessagesFromJSON()
    {
        var jsonString = File.ReadAllText(Application.streamingAssetsPath + "/messages.json");

        ChatMessages chatMessagesInJson = JsonUtility.FromJson<ChatMessages>(jsonString);

        return chatMessagesInJson;
    }

    /// <summary>
    /// Take a ChatMessages object and populate the list of template messages.
    /// </summary>
    /// <param name="chatMessages"></param>
    void PopulateResearcherUI(ChatMessages chatMessages)
    {
        foreach (ChatMessage chatMessage in chatMessages.chatMessages)
        {
            GameObject newButtonGameObject = Instantiate(TemplateMessageButtonPrefab, TemplateMessageSVContent.transform);
            Button newButton = newButtonGameObject.GetComponent<Button>();
            Text newButtonText = newButtonGameObject.GetComponentInChildren<Text>();
            newButtonText.text = chatMessage.MessageContent;

            newButton.onClick.AddListener(() => PopulateMessageEditFields(chatMessage.MessageContent, chatMessage.MessageResponses));
        }
    }

    /// <summary>
    /// Populate the message and acceptable response fields based on the selected template message in the list. 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="responses"></param>
    void PopulateMessageEditFields(string message, string[] responses)
    {       
        ResetMessageFields();

        MessageInputField.GetComponent<InputField>().text = message; 

        foreach (string response in responses)
        {
            AddResponseInputField(response);
        }
    } 

    /// <summary>
    /// Add an empty response InputField + remove Button object in the list.
    /// </summary>
    public void AddResponseInputField()
    {
        GameObject newInputFieldGameObject = Instantiate(ResponseInputFieldPrefab, MessageResponsesSVContent.transform);
        newInputFieldGameObject.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => RemoveResponseInputField(newInputFieldGameObject));
    }

    /// <summary>
    /// Add a response InputField populated with the given string + remove Button object in the list.
    /// </summary>
    /// <param name="responseText"></param>
    void AddResponseInputField(string responseText)
    {
        GameObject newInputFieldGameObject = Instantiate(ResponseInputFieldPrefab, MessageResponsesSVContent.transform);
        newInputFieldGameObject.transform.Find("InputField").GetComponent<InputField>().text = responseText;
        newInputFieldGameObject.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => RemoveResponseInputField(newInputFieldGameObject));
    }

    /// <summary>
    /// Remove and destroy the given response InputField + remove Button object from the list.
    /// </summary>
    /// <param name="inputFieldGameObject"></param>
    void RemoveResponseInputField(GameObject inputFieldGameObject)
    {
        Destroy(inputFieldGameObject.gameObject);
    }

    /// <summary>
    /// Remove value from the message InputField and clear the responses list.
    /// </summary>
    void ResetMessageFields()
    {
        MessageInputField.GetComponent<InputField>().text = "";

        foreach (Transform inputField in MessageResponsesSVContent.transform)
        {
            Destroy(inputField.gameObject);
        }
    }

    /// <summary>
    /// Retrieve values from the message InputField and list of acceptable responses InputField, and send them to wherever they're supposed to go (TODO: Change the end part of the summary here).
    /// </summary>
    public void SendResearcherMessage()
    {
        string message = MessageInputField.GetComponent<InputField>().text;
        if (String.IsNullOrWhiteSpace(message)) return;

        foreach (Transform inputFieldGameobject in MessageResponsesSVContent.transform)
        {
            if (String.IsNullOrWhiteSpace(inputFieldGameobject.transform.Find("InputField").GetComponent<InputField>().text)) return;
            
        }

        int responsesCount = MessageResponsesSVContent.transform.childCount;

        string[] responses = new string[responsesCount];

        int count = 0;
        foreach (Transform inputFieldGameobject in MessageResponsesSVContent.transform)
        {
            responses[count] = inputFieldGameobject.transform.Find("InputField").GetComponent<InputField>().text;
            count++;
        }

        ResetMessageFields();

        SendMessageToResearcherChatLog(true, message);
    } 

    /// <summary>
    /// Instantiate a GameObject to display a given message. The X position of the GameObject depends on who sent the message.
    /// </summary>
    /// <param name="researcherMessage"></param>
    /// <param name="message"></param>
    public void SendMessageToResearcherChatLog(bool sentFromResearcher, string message)
    {
        float padding = 5f;

        GameObject newChatLogGameObject;

        RectTransform newChatLogGameObjectTransform = new RectTransform();
        float xPos = 18;
        float yPos = 0f;

        TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft;
        Color backgroundColor;
        float colorNormalizer = 255f;        
        
        if (sentFromResearcher)
        {
            newChatLogGameObject = Instantiate(ParticipantChatObjectPrefab, ChatLogSVContent.transform);
            textAlignment = TextAlignmentOptions.MidlineRight;
            backgroundColor = new Color(70f / colorNormalizer, 255f / colorNormalizer, 65f / colorNormalizer, 95f / colorNormalizer);
        }
        else
        {
            newChatLogGameObject = Instantiate(ResearcherChatObjectPrefab, ChatLogSVContent.transform);
            xPos = -xPos;
            backgroundColor = new Color(65f / colorNormalizer, 245f / colorNormalizer, 255f / colorNormalizer, 95f / colorNormalizer);
        }

        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = textAlignment;
        newChatLogGameObject.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;

        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());

        newChatLogGameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(newChatLogGameObject.transform.GetComponent<RectTransform>().sizeDelta.x, newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
        newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + padding, newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y + padding);

        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());

        if (ChatLogSVContent.transform.childCount > 1)
        {
            yPos = yPos - Math.Abs(ChatLogSVContent.transform.GetChild(ChatLogSVContent.transform.childCount - 2).GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().localPosition.y) - Math.Abs(ChatLogSVContent.transform.GetChild(ChatLogSVContent.transform.childCount - 2).GetComponent<RectTransform>().sizeDelta.y / 2f);
        }

        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().transform.localPosition = new Vector3(xPos, 0, 0f);
        newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>().transform.localPosition = new Vector3(xPos, 0, 0f);

        Canvas.ForceUpdateCanvases();
        ChatScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
        Canvas.ForceUpdateCanvases();

    }


    public void SendParticipantMessageToChatResearcherLog(string message)
    {
        SendMessageToResearcherChatLog(false, message);
    }

    public void HandleMessage(ChatMessage chatMessage)
    {
        SendParticipantMessageToChatResearcherLog(chatMessage.messageContent);
    }

}
