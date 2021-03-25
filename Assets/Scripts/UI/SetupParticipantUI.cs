using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Demonstrating functionality for adding objects on the chat log of the participant's UI. Some methods used only for demonstration purposes,
/// redundancies with the researcher UI functionality, as well as some hard-coded variables will be refactored once networking is implemented. 
/// </summary>
public class SetupParticipantUI : MonoBehaviour
{
    public GameObject ChatScrollView;
    public GameObject ChatLogSVContent;
    public GameObject ParticipantChatObjectPrefab;
    public GameObject ResearcherChatObjectPrefab;
    public GameObject ResponseMessageButtonPrefab;


    public void AddParticipantChatObject()
    {
        SendMessageToParticipantChatLog(false, "Hello!", null);
    }


    public void AddResearcherChatObject(bool onResearcherUI)
    {
        if (onResearcherUI)
        {
            SendMessageToParticipantChatLog(false, "Hello there! :)", null);
        }
        else
        {
            string[] responses = new string[] { "First response", "Second response", "Third response Third response Third response Third response", "You get it by now", "No" };
            SendMessageToParticipantChatLog(true, "Hello there! :)", responses);
        }

    }

    /// <summary>
    /// Displaying the selected response sent by the participant, and destroying the list of acceptable responses under the researcher message.
    /// </summary>
    /// <param name="selectedResponseButton"></param>
    void ProcessResponse(GameObject selectedResponseButton)
    {
        string response = selectedResponseButton.transform.GetChild(0).GetComponent<Text>().text;
        SendMessageToParticipantChatLog(false, response, null);

        GameObject parentChatObject = selectedResponseButton.transform.parent.transform.parent.transform.parent.gameObject;
        
        Destroy(selectedResponseButton.transform.parent.gameObject);

        parentChatObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(parentChatObject.transform.GetComponent<RectTransform>().sizeDelta.x, parentChatObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y + 10f);
        parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().transform.localPosition = new Vector3(parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition.x, 0, 0f);

        ChatMessage chatMessage = new ChatMessage(response, null);
        this.transform.parent.GetComponent<ChatBehaviour>().Send(chatMessage);
    }

    /// <summary>
    /// Displays a message on the chat log. Responsible for placing messages correctly.
    /// </summary>
    /// <param name="sentFromResearcher"></param>
    /// <param name="message"></param>
    /// <param name="responses"></param>
    public void SendMessageToParticipantChatLog(bool sentFromResearcher, string message, string[] responses)
    {
        float padding = 5f;

        GameObject newChatLogGameObject;

        RectTransform newChatLogGameObjectTransform = new RectTransform();
        float xPos = -18;
        float yPos = 0f;

        float height = 0f;

        TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineRight;
        Color backgroundColor;
        float colorNormalizer = 255f;


        if (sentFromResearcher)
        {
            newChatLogGameObject = Instantiate(ResearcherChatObjectPrefab, ChatLogSVContent.transform);
            textAlignment = TextAlignmentOptions.MidlineLeft;
            backgroundColor = new Color(65f / colorNormalizer, 245f / colorNormalizer, 255f / colorNormalizer, 95f / colorNormalizer); // Hardcoding a blue-ish color and normalizing each RBG value cause that's what Unity likes.

            if (responses != null)
            {
                foreach (string response in responses)
                {
                    GameObject newResponseButton = Instantiate(ResponseMessageButtonPrefab, newChatLogGameObject.transform.GetChild(0).transform.GetChild(1));
                    newResponseButton.transform.GetChild(0).GetComponent<Text>().text = response;
                    newResponseButton.GetComponent<Button>().onClick.AddListener(() => ProcessResponse(newResponseButton));

                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>());
                }                
            }
        }
        else
        {
            newChatLogGameObject = Instantiate(ParticipantChatObjectPrefab, ChatLogSVContent.transform);
            xPos = -xPos;
            backgroundColor = new Color(70f / colorNormalizer, 255f / colorNormalizer, 65f / colorNormalizer, 95f / colorNormalizer); // Hardcoding a green-ish color and normalizing each RBG value cause that's what Unity likes.
        }

        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().alignment = textAlignment;
        newChatLogGameObject.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;

        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>());

        newChatLogGameObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(newChatLogGameObject.transform.GetComponent<RectTransform>().sizeDelta.x, newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

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

    public void HandleMessage(ChatMessage chatMessage)
    {
        SendMessageToParticipantChatLog(true, chatMessage.messageContent, chatMessage.messageResponses);
    }

}
