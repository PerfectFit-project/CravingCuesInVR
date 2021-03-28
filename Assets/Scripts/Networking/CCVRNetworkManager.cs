using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 
/// </summary>
public class CCVRNetworkManager : NetworkManager
{
    public GameObject LogInCanvas;

    string PlayerName;
    bool IsResearcher;

    GameObject playerObject;

    /// <summary>
    /// If the user has logged in as researcher, connect as host. Otherwise, connect as client. Used to determine which UI to display for each user. 
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="isResearcher"></param>
    public void ResolvePlayerLogIn(string playerName, bool isResearcher)
    {
        PlayerName = playerName;
        IsResearcher = isResearcher;

        if (isResearcher)
        {
            StartHost();
        }
        else
        {            
            StartClient();
        }
    }


    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public struct CreatePlayerMessage : NetworkMessage
    {
        public string name;
        public bool isResearcher;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMessage>(OnCreatePlayer);
    }
   
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // Tell the server to create a player with these values
        conn.Send(new CreatePlayerMessage { name = PlayerName, isResearcher = IsResearcher });
    }

    /// <summary>
    /// TODO: Disconnect behavior needs fixing. If host is disconnected (by terminating the application), client can log back in, but if logging in as researcher (host), messages do not appear on that UI.
    /// TODO: Maybe we don't need a way to reconnect, just a graceful way to disconnect and terminate the application.
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        LogInCanvas.SetActive(true);
    }

    void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        // Create a Player gameobject using the name and isResearcher values supplied by client
        playerObject = Instantiate(playerPrefab);
        playerObject.GetComponent<Player>().playerName = createPlayerMessage.name;
        playerObject.GetComponent<Player>().isResearcher = createPlayerMessage.isResearcher;
                
        // Set it as the player
        NetworkServer.AddPlayerForConnection(connection, playerObject);

        playerObject.GetComponent<Player>().InstantiateUI();
    }

}
