using System.Collections;
using System.Collections.Generic;
using Mirror;
using System;
using UnityEngine;
using UnityEngine.XR.LegacyInputHelpers;
using UnityEngine.SpatialTracking;

/// <summary>
/// Holds player information, and instantiates the correct UI based on user type, and handles network interaction. 
/// </summary>
public class Player : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    [SyncVar]
    public bool isResearcher;

    public GameObject ResearcherUIPrefab; 
    public GameObject ParticipantUIPrefab;

    public Material TransitionalMaterial;
    public Material CueEnvironmentMaterial;

    //GameObject XRRig;
    //GameObject ParticipantCamera;
    //GameObject ResearcherCamera;

    GameObject PanoramaCamera;
    
    GameObject PanoramaSphere;

    GameObject userInterface;

    public static event Action<Player, ChatMessage> OnMessage;



    /// <summary>
    /// Instantiate the appropriate UI depending on whether the user has logged in as Researcher or Participant. Activate the UI only on the client that has ownership of it, so as to not display every UI on every client.
    /// </summary>
    [ClientRpc]
    public void InstantiateUI()
    {
        PanoramaSphere = GameObject.Find("PanoramaSphere");
        GameObject researcherCamera = GameObject.Find("ResearcherCamera");
        GameObject participantCamera = GameObject.Find("ParticipantCamera");


        if (isResearcher && hasAuthority && isLocalPlayer)
        {
            PanoramaCamera = researcherCamera;
            userInterface = Instantiate(ResearcherUIPrefab, transform);
            participantCamera.SetActive(false);
        }
        else if (hasAuthority && isLocalPlayer)
        {
            PanoramaCamera = participantCamera;
            // Instantiate Participant UI in World Space overlaid on a 3D object, and attach it to the camera so that it moves with it.
            userInterface = Instantiate(ParticipantUIPrefab);
            userInterface.transform.parent = PanoramaCamera.transform;

            PanoramaCamera.GetComponent<Camera>().enabled = true;
            PanoramaCamera.GetComponent<TrackedPoseDriver>().enabled = true;
            PanoramaCamera.GetComponent<TrackCameraMovement>().enabled = true;

            researcherCamera.SetActive(false);

            //ResearcherCamera.GetComponent<Camera>().enabled = false;
            
            // Instantiate Participant UI in Screen Space as a child of the Player object 
            //userInterface = Instantiate(ParticipantUIPrefab, transform);
        }
        else
        {
            PanoramaCamera = Camera.main.gameObject;
        }
        

        if (hasAuthority || isLocalPlayer)
        {
            userInterface.SetActive(true);
        }

        if (!isResearcher)
        {
            // Presenting the environment when a client logs in.
            PresentEnvironment();
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
        if (isResearcher)
        {
            PanoramaCamera.transform.eulerAngles = rotation;
        }
        
    }

    public void PresentEnvironment()
    {
        // TODO: Maybe implement Splash Screen where users log in, separate from the environments themselves. 
        // For now, starting audio when both users are logged in.
        PanoramaSphere.GetComponent<Renderer>().material = CueEnvironmentMaterial;
        //PanoramaCamera.GetComponent<AudioSource>().Play();
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
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
                    // Uncomment the line below if using Screen-Space UI
                    //userInterface.GetComponent<Canvas>().enabled = !userInterface.GetComponent<Canvas>().enabled;
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

        }


        if (isResearcher || !hasAuthority)
            return;

    }        
}
