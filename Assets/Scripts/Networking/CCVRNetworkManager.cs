using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CCVRNetworkManager : NetworkManager
{
    string PlayerName;

    [SerializeField] private GameObject participantUIPrefab;
    [SerializeField] private GameObject researcherUIPrefab;

    private GameObject userInterfacePrefab;
    private GameObject userInterface;

    public void ResolvePlayerLogIn(string playerName, bool isResearcher)
    {
        PlayerName = playerName;

        //userInterfacePrefab = (isResearcher) ? researcherUIPrefab : participantUIPrefab;

        if (isResearcher)
        {
            Debug.Log("IS RESEARCHER");
            userInterfacePrefab = researcherUIPrefab;
            StartHost();
        }
        else
        {
            Debug.Log("IS PARTICIPANT");
            userInterfacePrefab = participantUIPrefab;
            StartClient();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("STARTED CLIENT");
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("STARTED HOST");
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
    
    //ISSUE HERE
     //   Figure out how to instantiate the different UIs. Current way of calling start client and start host may not work properly. Investigate.

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // tell the server to create a player with this name
        conn.Send(new CreatePlayerMessage { name = PlayerName });
    }

    void OnCreatePlayer(NetworkConnection connection, CreatePlayerMessage createPlayerMessage)
    {
        // create a gameobject using the name supplied by client
        GameObject playergo = Instantiate(playerPrefab);
        playergo.GetComponent<Player>().playerName = createPlayerMessage.name;
        playergo.GetComponent<Player>().isResearcher = createPlayerMessage.isResearcher;

        Debug.Log("Creating player");
        userInterface = Instantiate(userInterfacePrefab, playergo.transform);

        // set it as the player
        NetworkServer.AddPlayerForConnection(connection, playergo);

        userInterface.gameObject.SetActive(true);
    }


}
