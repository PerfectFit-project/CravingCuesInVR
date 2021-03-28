using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Loads template messages and acceptable responses from a JSON file and presenting them in the UI.
/// Utilize the ChatLogBehavior script to send messages over the network.
/// </summary>
public class SetupResearcherUI_NTW : MonoBehaviour
{
    public GameObject TemplateMessageSVContent;
    public GameObject TemplateMessageButtonPrefab;
    public GameObject MessageInputField;
    public GameObject MessageResponsesSVContent;
    public GameObject ResponseInputFieldPrefab;


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
    /// Construct a ChatMessage by retrieving values from the message InputField and list of acceptable responses InputField. 
    /// Have ChatLogBehavior send the ChatMessage to other chat logs.
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

        ChatMessage chatMessage = new ChatMessage(message, responses);
        GetComponent<ChatLogBehaviour>().OnSend(chatMessage);
    }



}
