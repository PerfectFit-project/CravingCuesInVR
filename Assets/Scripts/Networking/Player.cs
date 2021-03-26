using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string playerName;
    public bool isResearcher;

    public static event Action<Player, ChatMessage> OnMessage;

    [Command]
    public void CmdSend(ChatMessage chatMessage)
    {
        RpcReceive(chatMessage);
    }

    [ClientRpc]
    public void RpcReceive(ChatMessage chatMessage)
    {
        OnMessage?.Invoke(this, chatMessage);
    }
}
