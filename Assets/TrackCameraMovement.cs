using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TrackCameraMovement : MonoBehaviour
{
    public float MinCameraMovementToTrack;

    Transform AttachedCamera;
    Vector3 PreviousCameraRotation;
    Vector3 CurrentCameraRotation;


    // Start is called before the first frame update
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

    // Update is called once per frame
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
        Player player = NetworkClient.connection.identity.GetComponent<Player>();
        //player.RotateCamera(CameraToMove.transform.eulerAngles);
        player.RotateCamera(AttachedCamera.transform.eulerAngles);
    }
}
