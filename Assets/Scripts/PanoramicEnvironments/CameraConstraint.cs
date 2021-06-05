using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraConstraint : MonoBehaviour
{
    public float LowerVerticalRotationLimit;
    public float HigherVerticalRotationLimit;

    private void Update()
    {
        
        ConstrainCameraRotation();

    }


    public void ConstrainCameraRotation()
    {
        float verticalRotation = transform.localRotation.eulerAngles.x;
        float horizontalRotation = transform.localRotation.eulerAngles.y;
        
       

        Debug.Log("VERTICAL ROTATION: " + verticalRotation);
        Debug.Log("HORIZONTAL ROTATION: " + horizontalRotation);

        float angle = (transform.localRotation.eulerAngles.x > 180) ? transform.localRotation.eulerAngles.x - 360 : transform.localRotation.eulerAngles.x;

        // Constraining vertical rotation so it does not go outside the intended boundaries. X-Rotation is reversed (Up is - angle and down is + angle)
        if (angle > LowerVerticalRotationLimit)
            verticalRotation = LowerVerticalRotationLimit;
        else if (angle < HigherVerticalRotationLimit)
            verticalRotation = HigherVerticalRotationLimit;


        //transform.localEulerAngles = new Vector3(verticalRotation, horizontalRotation, 0f);

        transform.localRotation.eulerAngles.Set(verticalRotation, horizontalRotation, 0f);
    }
}
