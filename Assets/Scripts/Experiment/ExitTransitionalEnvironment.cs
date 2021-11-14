using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExitTransitionalEnvironment : MonoBehaviour
{
    public GameObject PanoramaSphere;

    [SerializeField]
    public bool ReadyToProceed { get; set; }

    bool KeyPressed;
    bool GamepadActive;

    private void Update()
    {
        if (!ReadyToProceed)
        {
            return;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
            GamepadActive = true;



        if (GamepadActive)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                KeyPressed = true;
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            KeyPressed = true;
        }

        if (KeyPressed)
        {
            PanoramaSphere.GetComponent<EnvironmentManagerLC>().ExitTransitionalEnvironment();
            KeyPressed = false;
        }
    }


}
