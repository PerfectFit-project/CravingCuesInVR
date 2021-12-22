using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

/// <summary>
/// Handle creating and positioning of messages on a Canvas chat log.
/// Used on Participant_Canvas_NTW and Researcher_Canvas_NTW objects, which have compatible chat logs.
/// </summary>
public class ChatLogBehaviour : MonoBehaviour
{
    public GameObject ChatScrollView;
    public GameObject ChatLogSVContent;
    public GameObject ChatObjPrefab;
    public GameObject ChatObjWResponsesPrefab;
    public GameObject ResponseMessageButtonPrefab;

    public void Awake()
    {
        Player.OnMessage += OnPlayerMessage;
    }


    /// <summary>
    /// When a message is received, call the relevant method to display it at the appropriate location on the chat log.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="chatMessage"></param>
    void OnPlayerMessage(Player player, ChatMessage chatMessage)
    {
        DisplayMessage(player.IsLocalPlayer, player.IsOwnedByServer, chatMessage);
    }

    /// <summary>
    /// Have the Player object which is the parent of the canvas the message is originating from, to send the message over the network.
    /// </summary>
    /// <param name="chatMessage"></param>
    public void OnSend(ChatMessage chatMessage)
    {
        Player player;

        if (NetworkManager.Singleton.LocalClient != null)
        {
            player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>();
        }
        else
        {
            player = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.gameObject.GetComponent<Player>();
        }



        player.CmdSendServerRpc(chatMessage);

    }

    /// <summary>
    /// Create a chat log message object and display it in the appropriate position based on the sender.
    /// If the message is sent from the researcher, the object created can also contain acceptable responses.
    /// </summary>
    /// <param name="ownMessage"></param>
    /// <param name="sentFromResearcher"></param>
    /// <param name="chatMessage"></param>
    public void DisplayMessage(bool ownMessage, bool sentFromResearcher, ChatMessage chatMessage)
    {
        GameObject newChatLogGameObject;

        RectTransform newChatLogGameObjectTransform = new RectTransform();

        float xPos = 18f;
        float yPos = 0f;
        TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineRight;
        float colorNormalizer = 255f;
        Color backgroundColor;

        if (ownMessage)
        {
            newChatLogGameObject = Instantiate(ChatObjPrefab, ChatLogSVContent.transform);
            backgroundColor = new Color(70f / colorNormalizer, 255f / colorNormalizer, 65f / colorNormalizer, 95f / colorNormalizer); // Hardcoding a green-ish color and normalizing each RBG value cause that's what Unity likes.
        }
        else
        {
            if (sentFromResearcher && chatMessage.messageResponses != null)
            {
                if (!transform.GetComponent<Canvas>().enabled)
                {
                    transform.GetComponent<AudioSource>().Play();
                }

                newChatLogGameObject = Instantiate(ChatObjWResponsesPrefab, ChatLogSVContent.transform);

                if (chatMessage.messageResponses.Length < 1)
                    Destroy(newChatLogGameObject.transform.GetChild(0).GetChild(1).gameObject);
                else
                {
                    newChatLogGameObject.transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 110f);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());


                foreach (string response in chatMessage.messageResponses)
                {
                    GameObject newResponseButton = Instantiate(ResponseMessageButtonPrefab, newChatLogGameObject.transform.GetChild(0).transform.GetChild(1).GetChild(0).GetChild(0));

                    newResponseButton.transform.GetChild(0).GetComponent<TMP_Text>().text = response;

                    newResponseButton.GetComponent<Button>().onClick.AddListener(() => ProcessResponse(newResponseButton));

                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>());
                }
                transform.GetComponent<ChatMessagesGamepadInteraction>().SetNewContainer(newChatLogGameObject);
            }
            else
            {
                newChatLogGameObject = Instantiate(ChatObjPrefab, ChatLogSVContent.transform);
            }

            xPos = -xPos;
            textAlignment = TextAlignmentOptions.MidlineLeft;
            backgroundColor = new Color(65f / colorNormalizer, 245f / colorNormalizer, 255f / colorNormalizer, 95f / colorNormalizer); // Hardcoding a blue-ish color and normalizing each RBG value cause that's what Unity likes.
        }

        newChatLogGameObject.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = chatMessage.messageContent;
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


    /// <summary>
    /// Displaying the selected response sent by the participant, and destroying the list of acceptable responses under the researcher message.
    /// </summary>
    /// <param name="selectedResponseButton"></param>
    void ProcessResponse(GameObject selectedResponseButton)
    {
        string response = selectedResponseButton.transform.GetChild(0).GetComponent<TMP_Text>().text;

        GameObject parentChatObject = selectedResponseButton.transform.parent.transform.parent.transform.parent.parent.parent.gameObject;

        Destroy(selectedResponseButton.transform.parent.parent.parent.gameObject);

        LayoutRebuilder.ForceRebuildLayoutImmediate(parentChatObject.transform.GetComponent<RectTransform>());

        parentChatObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(parentChatObject.transform.GetComponent<RectTransform>().sizeDelta.x, parentChatObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y + 10f);
        parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().transform.localPosition = new Vector3(parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition.x, 0, 0f);

        string[] dummyResponses = new string[0];
        ChatMessage chatMessage = new ChatMessage(response, dummyResponses);
        OnSend(chatMessage);
    }








}
