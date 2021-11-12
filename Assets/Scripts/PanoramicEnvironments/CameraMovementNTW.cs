using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Move camera given mouse movement.
/// </summary>
public class CameraMovementNTW : MonoBehaviour
{
    public float LowerHorizontalRotationLimit;
    public float HigherHorizontalRotationLimit;
    public int turnSpeedMouse;

    float horizontal;
    float vertical;
    Transform container;

    void Start()
    {
        container = GetComponent<Transform>();
    }


    public void UpdateCameraRotationLimits(float textureDimensionRatio)
    {
        if (textureDimensionRatio > 4)
        {
            LowerHorizontalRotationLimit = 20f;
            HigherHorizontalRotationLimit = 223.75f;
        }
        else
        {
            LowerHorizontalRotationLimit = 75.6f;
            HigherHorizontalRotationLimit = 223.9f;
        }        
    }

    public void MoveCamera()
    {
        // Using mouse
        horizontal = Input.GetAxis("Mouse X");
        vertical = Input.GetAxis("Mouse Y");

        container.Rotate(new Vector3(0, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);

        NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.GetComponent<Player>().RotateCameraServerRpc(container.eulerAngles);
    }
}
