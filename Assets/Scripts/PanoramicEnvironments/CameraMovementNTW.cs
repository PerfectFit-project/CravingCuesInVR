using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

        // Constraining horizontal rotation so it does not go outside the intended boundaries.
        if (container.rotation.eulerAngles.y < LowerHorizontalRotationLimit)
            container.eulerAngles = new Vector3(container.rotation.eulerAngles.x, LowerHorizontalRotationLimit, container.rotation.eulerAngles.z);

        if (container.rotation.eulerAngles.y > HigherHorizontalRotationLimit)
            container.eulerAngles = new Vector3(container.rotation.eulerAngles.x, HigherHorizontalRotationLimit, container.rotation.eulerAngles.z);

        // Not rotating on X or Z, which the line below allows, to prevent looking outside the vertical bounds of the image.
        // Might want to enable some Y and Z rotation to make viewing more realistic with a VR headset.
        //transform.Rotate(new Vector3(vertical, 0, 0) * Time.deltaTime * turnSpeedMouse);

        // Sending the camera rotation to all users.
        Player player = NetworkClient.connection.identity.GetComponent<Player>();
        player.RotateCamera(container.eulerAngles);
    }
}
