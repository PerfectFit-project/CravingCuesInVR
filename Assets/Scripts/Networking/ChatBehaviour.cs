using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine;

public class ChatBehaviour : NetworkBehaviour
{
    [SerializeField] private GameObject participantUIPrefab;
    [SerializeField] private GameObject researcherUIPrefab;

    private GameObject userInterfacePrefab;
    private GameObject userInterface;

    private static event Action<ChatMessage> OnMessage;

    bool init = false;

    void Start()
    {
        if (isClientOnly)
        {
            userInterfacePrefab = participantUIPrefab;
        }
        else if (isServer)
        {
            userInterfacePrefab = researcherUIPrefab;
        }

        //userInterfacePrefab = (isClientOnly) ? participantUIPrefab : researcherUIPrefab;
        userInterface = Instantiate(userInterfacePrefab, this.transform);

        //if (userInterfacePrefab != null)
        //{
        //    userInterface = Instantiate(userInterfacePrefab, this.transform);
        //}
        //else
        //{

        //}
        
        init = true;
    }

    public override void OnStartAuthority()
    {
        Debug.Log(this.transform.parent.name + " got authority.");

        if (!init)
        {
            Start();
        }        

        OnMessage += HandleNewMessage;

        userInterface.gameObject.SetActive(true); 
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority) { return; }

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(ChatMessage chatMessage)
    {
        if (isClientOnly)
        {
            //send message to participant UI
            userInterface.GetComponent<SetupParticipantUI>().HandleMessage(chatMessage);
        }
        else if (isServer)
        {
            //send message to researcher UI
            userInterface.GetComponent<SetupResearcherUI>().HandleMessage(chatMessage);
        }
    }

    [Client]
    public void Send(ChatMessage chatMessage)
    {
        Debug.Log("Send Invoked with message: " + chatMessage.messageContent);
        CmdSendMessage(chatMessage);
    }

    [Command]
    private void CmdSendMessage(ChatMessage chatMessage)
    {
        RpcHandleMessage(chatMessage);
    }

    [ClientRpc]
    private void RpcHandleMessage(ChatMessage chatMessage)
    {
        OnMessage?.Invoke(chatMessage);
    }


}
