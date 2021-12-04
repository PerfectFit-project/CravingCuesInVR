using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Collections;


/// <summary>
/// Performs form-checking and initializes the network user login process.
/// </summary>
public class LogInManager : MonoBehaviour
{
    public NetworkManager NetManager;

    [SerializeField]
    NetworkTransport m_IpHostTransport;

    [SerializeField]
    NetworkTransport m_UnityRelayTransport;

    /// <summary>
    /// The transport used when hosting the game on an IP address.
    /// </summary>
    public NetworkTransport IpHostTransport => m_IpHostTransport;

    /// <summary>
    /// The transport used when hosting the game over a unity relay server.
    /// </summary>
    public NetworkTransport UnityRelayTransport => m_UnityRelayTransport;

    //NetworkTransport Unity_Relay_Transport;

    public GameObject DropdownMenu;
    public GameObject UserNameInputField;
    public GameObject PasswordLabel;
    public GameObject PasswordInputField;
    public GameObject RelayIDLabel;
    public GameObject RelayIDInputField;
    public GameObject UserNameWarningLabel;
    public GameObject PasswordWarningLabel;
    public GameObject RelayIDWarningLabel;
    public GameObject LocalHostToggle;

    public GameObject RelayIDDisplayText;

    // For simple checking so that participants can't log in as researchers. Shouldn't be an issue for the intended experiments, but probably replace with a more secure solution if wanting to implement a remote application.
    public string password;

    bool calledStartClient = false;
    bool calledPlayerInit = false;
    string userNameToPass;

    private void Start()
    {
        NetManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        

        //Ask Unity Services to allocate a Relay server that will handle eight players
        //Allocation allocation = await Unity.Services.Relay.Relay.Instance.CreateAllocationAsync(8);

        //NetworkManager.Singleton.NetworkConfig.NetworkTransport = NetworkManager.Singleton.gameObject.GetComponent<>


    }

    /// <summary>
    /// Enable the password field if the Researcher dropdown menu option is selected.
    /// </summary>
    public void CheckDropDownSelection()
    {
        if (DropdownMenu.transform.GetChild(0).GetComponent<TMP_Text>().text.Equals("Researcher"))
        {
            PasswordLabel.SetActive(true);
            PasswordInputField.SetActive(true);

            //RelayIDLabel.SetActive(false);
            //RelayIDInputField.SetActive(false);
            //RelayIDWarningLabel.SetActive(false);
        }
        else if (DropdownMenu.transform.GetChild(0).GetComponent<TMP_Text>().text.Equals("Participant"))
        {
           
            //RelayIDLabel.SetActive(true);
            //RelayIDInputField.SetActive(true);


            PasswordLabel.SetActive(false);
            PasswordInputField.SetActive(false);
            PasswordWarningLabel.SetActive(false);
        }
        CheckLHToggle();
    }

    public void CheckLHToggle()
    {
        if (LocalHostToggle.GetComponent<Toggle>().isOn)
        {
            RelayIDInputField.SetActive(false);
            RelayIDLabel.SetActive(false);
            RelayIDWarningLabel.SetActive(false);
            RelayIDDisplayText.SetActive(false);
        }
        else
        {
            if (DropdownMenu.transform.GetChild(0).GetComponent<TMP_Text>().text.Equals("Researcher"))
            {
                RelayIDDisplayText.SetActive(true);
                RelayIDInputField.SetActive(false);
                RelayIDLabel.SetActive(false);
            }
            else
            {
                RelayIDDisplayText.SetActive(false);
                RelayIDInputField.SetActive(true);
                RelayIDLabel.SetActive(true);
            }
        }
    }
    

    /// <summary>
    /// Perform form checking and if everything is in order, have the Network Manager initiate a Player object creation process.
    /// </summary>
    public void LogInUser()
    {
        bool error = false;

        string userName = UserNameInputField.GetComponent<TMP_InputField>().text;
        bool isResearcher = (PasswordInputField.activeSelf) ? true : false;

        if (String.IsNullOrWhiteSpace(userName))
        {
            // Add username checking if necessary 
            UserNameWarningLabel.SetActive(true);
            error = true;
        }

        if (PasswordInputField.activeSelf && (!PasswordInputField.GetComponent<TMP_InputField>().text.Equals(password) || String.IsNullOrWhiteSpace(PasswordInputField.GetComponent<TMP_InputField>().text)))
        {
            PasswordWarningLabel.GetComponent<TMP_Text>().text = "Invalid Password";

            if (String.IsNullOrWhiteSpace(PasswordInputField.GetComponent<TMP_InputField>().text))
            {
                PasswordWarningLabel.GetComponent<TMP_Text>().text = "Cannot Be Empty";
            }

            PasswordWarningLabel.SetActive(true);
            error = true;
        }

        if (LocalHostToggle.GetComponent<Toggle>().isOn)
        {
            if (RelayIDInputField.activeSelf)
            {
                RelayIDInputField.SetActive(false);
                RelayIDLabel.SetActive(false);
                RelayIDWarningLabel.SetActive(false);
                RelayIDDisplayText.SetActive(false);
            }
        }

        if (RelayIDInputField.activeSelf && String.IsNullOrWhiteSpace(RelayIDInputField.GetComponent<TMP_InputField>().text))
        {
            RelayIDWarningLabel.GetComponent<TMP_Text>().text = "Cannot Be Empty";

            RelayIDWarningLabel.SetActive(true);
            error = true;
        }

        if (error)
        {
            return;
        }

        //string ipAddressToConnect = IPAddressInputField.GetComponent<TMP_InputField>().text;
        //if (String.IsNullOrWhiteSpace(ipAddressToConnect))
        //{
        //    ipAddressToConnect = "localhost";
        //}

        //NetworkManager.GetComponent<CCVRNetworkManager>().ResolvePlayerLogIn(userName, isResearcher, ipAddressToConnect);

        

        ResolveLogin(isResearcher, userName);


        //transform.gameObject.SetActive(false);
    }




    public void ResolveLogin(bool isResearcher, string userName)
    {
        if (LocalHostToggle.GetComponent<Toggle>().isOn)
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = IpHostTransport;

            if (isResearcher)
            {
                NetManager.StartHost();
                NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<Player>().InitPlayerServerRpc(userName, true);
                transform.gameObject.SetActive(false);
            }
            else
            {
                NetManager.StartClient();
                calledStartClient = true;
                userNameToPass = userName;
                //NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>().InitPlayerServerRpc(userName, false);
            }
            //transform.gameObject.SetActive(false);
        }
        else
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = UnityRelayTransport;

            //Unity_Relay_Transport = NetManager.GetComponent<UnityTransport>();
            if (isResearcher)
            {
                StartUnityRelayHost(userName);
                transform.gameObject.SetActive(false);
            }
            else
            {
                string joinCode = RelayIDInputField.GetComponent<TMP_InputField>().text;
                StartClientUnityRelayModeAsync(joinCode, userName);
            }
        }

            


    }


    public async void StartUnityRelayHost(string userName)
    {
        //var chosenTransport = NetworkManager.Singleton.gameObject.GetComponent<TransportPicker>().UnityRelayTransport;
        NetManager.NetworkConfig.NetworkTransport = UnityRelayTransport;

        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                var playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log(playerId);
            }

            // we now need to get the joinCode?
            var serverRelayUtilityTask = RelayUtility.AllocateRelayServerAndGetJoinCode(4);
            await serverRelayUtilityTask;
            // we now have the info from the relay service
            var (ipv4Address, port, allocationIdBytes, connectionData, key, joinCode) = serverRelayUtilityTask.Result;

            RelayJoinCode.Code = joinCode;

            UnityTransport utp = (UnityTransport)UnityRelayTransport;

            // we now need to set the RelayCode somewhere :P
            utp.SetRelayServerData(ipv4Address, port, allocationIdBytes, key, connectionData);

            RelayIDDisplayText.SetActive(true);
            RelayIDDisplayText.GetComponent<TMP_Text>().text = RelayIDDisplayText.GetComponent<TMP_Text>().text + " " + joinCode;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat($"{e.Message}");
            throw;
        }

        NetManager.StartHost();

        NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<Player>().InitPlayerServerRpc(userName, true);
        // NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<NewPlayer>().playerName = new NetworkVariable<FixedString64Bytes>(userName);

    }


    public async void StartClientUnityRelayModeAsync(string joinCode, string userName)
    {
        NetManager.NetworkConfig.NetworkTransport = UnityRelayTransport;

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log(AuthenticationService.Instance);

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                var playerId = AuthenticationService.Instance.PlayerId;
                Debug.Log(playerId);
            }

            var clientRelayUtilityTask = RelayUtility.JoinRelayServerFromJoinCode(joinCode);
            await clientRelayUtilityTask;
            var (ipv4Address, port, allocationIdBytes, connectionData, hostConnectionData, key) = clientRelayUtilityTask.Result;
            UnityTransport utp = (UnityTransport)UnityRelayTransport;
            utp.SetRelayServerData(ipv4Address, port, allocationIdBytes, key, connectionData, hostConnectionData);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat($"{e.Message}");
            throw;
        }

        //ConnectClient(portal);

        NetManager.StartClient();

        calledStartClient = true;
        userNameToPass = userName;
        //while (NetworkManager.Singleton.LocalClient == null) { }
        // NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<NewPlayer>().InitPlayerClientRpc(userName, false);

        //StartCoroutine(WaitUntilClientStarted(userName));

        //NetManager.LocalClient.PlayerObject.gameObject.GetComponent<NewPlayer>().InitPlayerClientRpc(userName, false);

        //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<NewPlayer>().InitPlayer(userName, false);

        //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<NewPlayer>().playerName = new NetworkVariable<FixedString64Bytes>(userName);
        
    }

   

    private void Update()
    {
        if (!calledPlayerInit && calledStartClient)
        {
            if (NetworkManager.Singleton.LocalClient != null)
            {
                Debug.Log("NOT NULL");
                NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>().InitPlayerServerRpc(userNameToPass, false);
                calledPlayerInit = true;
                transform.gameObject.SetActive(false);
            }
        }
    }


}
