using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SyncVar]
    public bool isResearcher;

    public GameObject ResearcherUIPrefab; 
    public GameObject ParticipantUIPrefab;

    GameObject PanoramaCamera;

    GameObject userInterface;

    public static event Action<Player, ChatMessage> OnMessage;



    /// <summary>
    /// Instantiate the appropriate UI depending on whether the user has logged in as Researcher or Participant. Activate the UI only on the client that has ownership of it, so as to not display every UI on every client.
    /// </summary>
    [ClientRpc]
    public void InstantiateUI()
    {
        PanoramaCamera = GameObject.Find("PanoramaCamera");

        if (isResearcher)
        {
            userInterface = Instantiate(ResearcherUIPrefab, transform);
        }
        else
        {          
            userInterface = Instantiate(ParticipantUIPrefab, transform);
        }
        

        if (hasAuthority || isLocalPlayer)
        {
            userInterface.SetActive(true);
        }
    }

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


    [Command]
    public void RotateCamera(Vector3 rotation)
    {
        ReceiveCameraRotation(rotation);
    }

    [ClientRpc]
    public void ReceiveCameraRotation(Vector3 rotation)
    {
        PanoramaCamera.transform.eulerAngles = rotation;
    }


    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
        }


        if (isResearcher || !hasAuthority)
            return;
        else
        {
            PanoramaCamera.GetComponent<CameraMovementNTW>().MoveCamera();
        }
    }        
}
