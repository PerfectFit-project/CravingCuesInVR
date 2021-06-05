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

    public GameObject PanoramaCamera;

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
            // Instantiate Participant UI in World Space overlaid on a 3D object, and attach it to the camera so that it moves with it.
            userInterface = Instantiate(ParticipantUIPrefab);
            userInterface.transform.parent = PanoramaCamera.transform;

            // Instantiate Participant UI in Screen Space as a child of the Player object 
            //userInterface = Instantiate(ParticipantUIPrefab, transform);
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
            if (hasAuthority || isLocalPlayer)
            {
                if (isResearcher)
                {
                    if (userInterface.GetComponent<Canvas>())
                    {
                        userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
                    }
                    
                }
                else
                {
                    if (userInterface.GetComponent<MeshRenderer>())
                    {
                        userInterface.GetComponent<MeshRenderer>().enabled = !userInterface.GetComponent<MeshRenderer>().enabled;
                    }
                    if (userInterface.transform.GetChild(0).GetComponent<Canvas>())
                    {
                        userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled = !userInterface.transform.GetChild(0).GetComponent<Canvas>().enabled;
                    }
                    
                }
                    
            }

            //if (isResearcher)
            //{
            //    userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
            //}
            //else
            //    //userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
            //    userInterface.SetActive(!userInterface.activeSelf);

        }


        if (isResearcher || !hasAuthority)
            return;
        else
        {
            PanoramaCamera.GetComponent<CameraMovementNTW>().MoveCamera();
        }
    }        
}
