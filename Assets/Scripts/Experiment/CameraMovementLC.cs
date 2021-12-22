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
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, horizontal * (-1), 0f) * Time.deltaTime * MouseTurnSpeed); 
    }


}
