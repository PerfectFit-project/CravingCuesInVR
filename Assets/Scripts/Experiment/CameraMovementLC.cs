using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementLC : MonoBehaviour
{
    public float LowerHorizontalRotationLimit;
    public float HigherHorizontalRotationLimit;
    public float LowerVerticalRotationLimit;
    public float HigherVerticalRotationLimit;
    public int turnSpeedMouse;

    float horizontal;
    float vertical;
    Transform container;

    void Start()
    {
        container = GetComponent<Transform>();
    }
    
    private void Update()
    {
        MoveCamera();
    }

    //private void LateUpdate()
    //{
    //    ConstrainCameraRotation();
    //}

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

        container.Rotate(new Vector3(vertical, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);

        Transform temp = container;
        temp.Rotate(new Vector3(vertical, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);

        ConstrainCameraRotation(temp);

        container.eulerAngles = new Vector3(temp.rotation.eulerAngles.x, temp.rotation.eulerAngles.y, temp.rotation.eulerAngles.z);

        // Not rotating on X or Z, which the line below allows, to prevent looking outside the vertical bounds of the image.
        // Might want to enable some Y and Z rotation to make viewing more realistic with a VR headset.
        //transform.Rotate(new Vector3(vertical, 0, 0) * Time.deltaTime * turnSpeedMouse);        
    }

    public void ConstrainCameraRotation(Transform transform)
    {
        float horizontalRotation = transform.rotation.eulerAngles.y;
        float verticalRotation = transform.rotation.eulerAngles.x;

        // Constraining horizontal rotation so it does not go outside the intended boundaries.
        if (transform.rotation.eulerAngles.y < LowerHorizontalRotationLimit)
            horizontalRotation = LowerHorizontalRotationLimit;
        else if (transform.rotation.eulerAngles.y > HigherHorizontalRotationLimit)
            horizontalRotation = HigherHorizontalRotationLimit;

        // eulerAngles returns a positive number, so doing this to get a negative value to check whether the rotation falls within the vertical boundaries.
        float angle = (transform.rotation.eulerAngles.x > 180) ? transform.rotation.eulerAngles.x - 360 : transform.rotation.eulerAngles.x;

        // Constraining vertical rotation so it does not go outside the intended boundaries. X-Rotation is reversed (Up is - angle and down is + angle)
        if (angle > LowerVerticalRotationLimit)
            verticalRotation = LowerVerticalRotationLimit;
        else if (angle < HigherVerticalRotationLimit)
            verticalRotation = HigherVerticalRotationLimit;


        transform.eulerAngles = new Vector3(verticalRotation, horizontalRotation, 0f);
    }
}
