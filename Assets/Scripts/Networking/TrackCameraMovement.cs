using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Class that tracks how much a camera controlled by a participant has rotated and if more than the threshold value, updates the researcher camera rotation.
/// </summary>
public class TrackCameraMovement : MonoBehaviour
{
    public float MinCameraMovementToTrack;

    Transform AttachedCamera;
    Vector3 PreviousCameraRotation;
    Vector3 CurrentCameraRotation;


    void Start()
    {
        if (MinCameraMovementToTrack < 0.00001)
        {
            MinCameraMovementToTrack = 0.1f;
        }
        AttachedCamera = GetComponent<Transform>();
        PreviousCameraRotation = AttachedCamera.eulerAngles;
        CurrentCameraRotation = AttachedCamera.eulerAngles;
    }

    void Update()
    {
        CurrentCameraRotation = AttachedCamera.eulerAngles;

        if (Vector3.Distance(CurrentCameraRotation, PreviousCameraRotation) > MinCameraMovementToTrack)
        {
            UpdateCameraRotation();
        }

        PreviousCameraRotation = CurrentCameraRotation;
    }

    void UpdateCameraRotation()
    {
        NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>().RotateCameraServerRpc(AttachedCamera.transform.eulerAngles);
    }
}
