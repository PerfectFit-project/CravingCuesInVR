using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Local camera movement using mouse.
/// </summary>
public class CameraMovementLC : MonoBehaviour
{
    public float LowerHorizontalRotationLimit;
    public float HigherHorizontalRotationLimit;
    public float LowerVerticalRotationLimit;
    public float HigherVerticalRotationLimit;
    [SerializeField] private Vector2 RotationSensitivity;
    [SerializeField] private Vector2 RotationAcceleration;
    [SerializeField] private float InputLagPeriod;
    [SerializeField] private bool LimitHorizontalRotation;

    public float MouseTurnSpeed;

    Vector2 velocity;
    Vector2 rotation;
    Vector2 lastInputEvent;
    float inputLagTimer;
    
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

    private Vector2 GetInput()
    {
        inputLagTimer += Time.deltaTime;

        Vector2 input = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
            );

        if ((Mathf.Approximately(0, input.x) && Mathf.Approximately(0, input.y)) == false || inputLagTimer >= InputLagPeriod)
        {
            lastInputEvent = input;
            inputLagTimer = 0;
        }
        return lastInputEvent;
    }

    public void MoveCamera()
    {
        // Using mouse

        //Vector2 input = new Vector2(
        //    Input.GetAxis("Mouse X"),
        //    Input.GetAxis("Mouse Y")
        //    );

        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, horizontal * (-1), 0f) * Time.deltaTime * MouseTurnSpeed);



        //Vector2 rawVelocity = GetInput() * RotationSensitivity;

        //velocity = new Vector2(
        //    Mathf.MoveTowards(velocity.x, rawVelocity.x, RotationAcceleration.x * Time.deltaTime),
        //    Mathf.MoveTowards(velocity.y, rawVelocity.y, RotationAcceleration.y * Time.deltaTime)
        //    );

        //rotation += velocity * Time.deltaTime;

        //transform.eulerAngles = new Vector3(rotation.y, rotation.x, 0f);





        //Debug.Log("Vertical ROT: " + transform.rotation.eulerAngles.x);
        //Debug.Log("Horizontal ROT: " + transform.rotation.eulerAngles.y);

        //container.Rotate(new Vector3(vertical, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);

        //Transform temp = container;
        //temp.Rotate(new Vector3(vertical, horizontal * (-1), 0f) * Time.deltaTime * turnSpeedMouse);

        //ConstrainCameraRotation();

        //container.eulerAngles = new Vector3(temp.rotation.eulerAngles.x, temp.rotation.eulerAngles.y, temp.rotation.eulerAngles.z);

        // Not rotating on X or Z, which the line below allows, to prevent looking outside the vertical bounds of the image.
        // Might want to enable some Y and Z rotation to make viewing more realistic with a VR headset.
        //transform.Rotate(new Vector3(vertical, 0, 0) * Time.deltaTime * turnSpeedMouse);        
    }

    //public void ConstrainCameraRotation()
    //{
    //    float verticalRotation = transform.localRotation.x;
    //    float horizontalRotation = transform.localRotation.eulerAngles.y;        

    //    if (LimitHorizontalRotation)
    //    {
    //        // Constraining horizontal rotation so it does not go outside the intended boundaries.
    //        if (horizontalRotation < LowerHorizontalRotationLimit)
    //            horizontalRotation = LowerHorizontalRotationLimit;
    //        else if (horizontalRotation > HigherHorizontalRotationLimit)
    //            horizontalRotation = HigherHorizontalRotationLimit;
    //    }

    //    Debug.Log("VERTICAL ROTATION: " + verticalRotation);
    //    Debug.Log("HORIZONTAL ROTATION: " + horizontalRotation);
    //    // eulerAngles returns a positive number, so doing this to get a negative value to check whether the rotation falls within the vertical boundaries.
        
    //    float angle = (transform.localRotation.eulerAngles.x > 180) ? transform.localRotation.eulerAngles.x - 360 : transform.localRotation.eulerAngles.x;

    //    // Constraining vertical rotation so it does not go outside the intended boundaries. X-Rotation is reversed (Up is - angle and down is + angle)
    //    if (angle > LowerVerticalRotationLimit)
    //        verticalRotation = LowerVerticalRotationLimit;
    //    else if (angle < HigherVerticalRotationLimit)
    //        verticalRotation = HigherVerticalRotationLimit;


    //    transform.localEulerAngles = new Vector3(verticalRotation, horizontalRotation, 0f);


    //}



}
