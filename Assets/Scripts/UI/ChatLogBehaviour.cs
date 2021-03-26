using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

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

    void OnPlayerMessage(Player player, ChatMessage chatMessage)
    {
        //player.isLocalPlayer
        //player.isParticipant
        DisplayMessage(player.isLocalPlayer, player.isResearcher, chatMessage);

    }

    public void OnSend(ChatMessage chatMessage)
    {
        Player player = NetworkClient.connection.identity.GetComponent<Player>();

        player.CmdSend(chatMessage);
    }


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
                newChatLogGameObject = Instantiate(ChatObjWResponsesPrefab, ChatLogSVContent.transform);

                foreach (string response in chatMessage.messageResponses)
                {
                    GameObject newResponseButton = Instantiate(ResponseMessageButtonPrefab, newChatLogGameObject.transform.GetChild(0).transform.GetChild(1));
                    newResponseButton.transform.GetChild(0).GetComponent<Text>().text = response;
                    newResponseButton.GetComponent<Button>().onClick.AddListener(() => ProcessResponse(newResponseButton));

                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetComponent<RectTransform>());
                    LayoutRebuilder.ForceRebuildLayoutImmediate(newChatLogGameObject.transform.GetChild(0).GetComponent<RectTransform>());
                }
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
        string response = selectedResponseButton.transform.GetChild(0).GetComponent<Text>().text;
        DisplayMessage(true, false, new ChatMessage(response, null));

        GameObject parentChatObject = selectedResponseButton.transform.parent.transform.parent.transform.parent.gameObject;

        Destroy(selectedResponseButton.transform.parent.gameObject);

        parentChatObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(parentChatObject.transform.GetComponent<RectTransform>().sizeDelta.x, parentChatObject.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y + 10f);
        parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().transform.localPosition = new Vector3(parentChatObject.transform.GetChild(0).GetComponent<RectTransform>().localPosition.x, 0, 0f);

        ChatMessage chatMessage = new ChatMessage(response, null);
        //this.transform.parent.GetComponent<ChatBehaviour>().Send(chatMessage);
        OnSend(chatMessage);
    }


  





}
