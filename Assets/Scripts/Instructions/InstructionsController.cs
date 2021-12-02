using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;


enum InstructionsState : short
{
    ShowWelcomeMessage,
    CameraMovementInstructions,
    CameraMovementTest,
    UIControlInstructions,
    UIControllerTest,
    ShowEndingMessage,
    EndInstructionsProcess
}


public class InstructionsController : MonoBehaviour
{
    public GameObject InstructionsUIObj;

    public GameObject SmartphoneUIObject;    
    public GameObject[] UIInteractionTestVisualsDisplayOrder;
    
    public GameObject[] ObjectsToCheckIfVisible;
    public GameObject[] DirectionArrowImagePlanes;
    public GameObject CheckMarkImagePlane;

    GameObject SmartphoneUICanvas;
    GameObject ObjectToFind;
    GameObject LookDirectionArrow;

    Gamepad Gamepad;
    bool GamepadActive;

    short CurrentState;
    bool UpdateState;

    bool TriggerMessageProgress = false;
    string ActiveMessageCategory;
    bool SkippableMessages;
    short MessagePresentIndex;
    string MessageToPresent;

    bool CheckObjectVisibility;
    bool UIInteractionTestActive;


    Dictionary<string, List<string>> MessagesDict = new Dictionary<string, List<string>>
    {
        { "welcome", new List<string>
        {
            { "Hello, and welcome to this introduction." },
            { "1. You will find out how to look around in the virtual envivironments, and \n2. You will learn how to use the gamepad controller to interact with menus." },
            { "Please put on the head-mounted display and press the A button on your gamepad controller to begin." }
        } },
        { "camera instructions", new List<string>
        {
            { "You can move your head in the direction you want to look in." },

        } },
        { "camera test", new List<string>
        {
            { "Let's try a small test: Find the small smiley face icon in the environment by moving your head to shift your view around. The arrow is there to help guide you in the shortest direction." },
            { "Great, good job! Let's try that again, find the smiley face again." },
            { "Nice! One last time, you know what to do." },
            { "Fantastic, you seem to have the hang of this! You should have no issues experiencing the virtual environments we will present you." }
        } },
        { "controls instructions", new List<string>
        {
            { "Now let's look at how you can use the gamepad controller to navigate the user interface." },
            { "This is a sample user interface." },
            { "The option highlighted in blue is the currently selected one." },
            { "You can use up/down keys on your gamepad controller's D-Pad, to navigate through the options." },
            { "If you select a slider, you can use the right/left keys on the D-Pad to cycle through the options available." },
            { "If you select a button, you can press the A button on your gamepad controller to confirm your selection." },
            { "Finally, you can open or close the user interface by pressing the Y button on your gamepad controller." }
        } },
        { "controls test", new List<string>
        {
            { "Let's try a small test:\n In the menu below,\n1. Navigate to \"Slider C\",\n 2. Change and keep its value to 4,\n 3. Find the button saying \"Button B\", and confirm your selection." },
            { "Sorry, that is not correct.\n1. Navigate to \"Slider C\",\n 2. Change and keep its value to 4,\n 3. Find the button saying \"Button B\", and confirm your selection." },
            { "Fantastic, you got it!" },
        } },
        { "ending", new List<string>
        {
            { "This concludes our brief introduction process." },
            { "Please take off the head-mounted display and follow the researcher's instructions." },
        } },
    };

    /* Instructions Process Outline
     * ----------------------------
     * 
     * 1. Inform user they can look around by moving their head towards the location in which they want to.
     * 2. Instruct them to look towards a certain location until a certain object is visible.
     * 3. Then instruct them to look towards another location until a different object is visible.
     * 4. Inform them about UI controls showing a picture of a gamepad controller and what different buttons do
     *    a. Show them a sample list and inform them how they can traverse it using the D-Pad.
     *    b. Show them a sample button and inform them how they can select it.
     *    c. The above two can be combined by presenting them with a sample chat interaction.
     *    d. Show them how they can "close" the UI by pressing the relevant button.
     * 
     */


    void Start()
    {
        SmartphoneUICanvas = SmartphoneUIObject.transform.GetChild(0).gameObject;

        Gamepad = Gamepad.current;
        if (Gamepad != null)
            GamepadActive = true;

        CurrentState = (short)InstructionsState.ShowWelcomeMessage;
        UpdateState = false;
        TakeAction();

    }

    void Update()
    {
        if (GamepadActive)
        {
            if (Gamepad.buttonSouth.wasPressedThisFrame)
            {
                TriggerMessageProgress = true;
            }


            if (Gamepad.buttonNorth.wasPressedThisFrame)
            {              
                SmartphoneUIObject.GetComponent<MeshRenderer>().enabled = !SmartphoneUIObject.GetComponent<MeshRenderer>().enabled;
                SmartphoneUICanvas.GetComponent<Canvas>().enabled = !SmartphoneUICanvas.GetComponent<Canvas>().enabled;
            }

        }
        

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerMessageProgress = true;
        }


        if (TriggerMessageProgress)
        {
            if (SkippableMessages)
            {
                PresentMessage();
            }
            TriggerMessageProgress = false;
        }


        if (CheckObjectVisibility)
        {
            if (ObjectToFind.transform.GetChild(0).GetComponent<Renderer>().isVisible)
            {
                CheckObjectVisibility = false;

                CheckMarkImagePlane.gameObject.SetActive(true);

                StartCoroutine(ObjDisplayDelay(LookDirectionArrow, 0.5f));
                StartCoroutine(ObjDisplayDelay(ObjectToFind, 1.3f));
                StartCoroutine(ObjDisplayDelay(CheckMarkImagePlane, 1.6f));

                if (ObjectToFind.Equals(ObjectsToCheckIfVisible[0]))
                {
                    ObjectsToCheckIfVisible[1].SetActive(true);
                    ObjectToFind = ObjectsToCheckIfVisible[1];

                    LookDirectionArrow = DirectionArrowImagePlanes[1];
                    StartCoroutine(ObjDisplayDelay(LookDirectionArrow, 2f));
                    CheckObjectVisibility = true;
                }
                else if (ObjectToFind.Equals(ObjectsToCheckIfVisible[1]))
                {
                    ObjectsToCheckIfVisible[2].SetActive(true);
                    ObjectToFind = ObjectsToCheckIfVisible[2];

                    LookDirectionArrow = DirectionArrowImagePlanes[0];
                    StartCoroutine(ObjDisplayDelay(LookDirectionArrow, 2f));
                    CheckObjectVisibility = true;
                }
                else
                {
                    SkippableMessages = true;
                }

                PresentMessage();
            }
        }


        if (UpdateState)
        {
            UpdateState = false;
            NextUpdateState();
        }

    }

    void NextUpdateState()
    {
        switch (CurrentState)
        {
            case (short)InstructionsState.ShowWelcomeMessage:
                CurrentState = (short)InstructionsState.CameraMovementInstructions;
                break;
            case (short)InstructionsState.CameraMovementInstructions:
                CurrentState = (short)InstructionsState.CameraMovementTest;
                break;
            case (short)InstructionsState.CameraMovementTest:
                CurrentState = (short)InstructionsState.UIControlInstructions;
                break;
            case (short)InstructionsState.UIControlInstructions:
                CurrentState = (short)InstructionsState.UIControllerTest;
                break;
            case (short)InstructionsState.UIControllerTest:
                CurrentState = (short)InstructionsState.ShowEndingMessage;
                break;
            case (short)InstructionsState.ShowEndingMessage:
                CurrentState = (short)InstructionsState.EndInstructionsProcess;
                break;
            case (short)InstructionsState.EndInstructionsProcess:
                break;
            default:
                break;
        }

        TakeAction();
    }


    void TakeAction()
    {
        switch (CurrentState)
        {
            case (short)InstructionsState.ShowWelcomeMessage:
                PresentWelcomeMessages();
                break;
            case (short)InstructionsState.CameraMovementInstructions:
                PresentCameraInstructions();
                break;
            case (short)InstructionsState.CameraMovementTest:
                PerformCameraMovementTest();
                break;
            case (short)InstructionsState.UIControlInstructions:
                PresentUIInteractionInstructions();
                break;
            case (short)InstructionsState.UIControllerTest:
                PerformUIInteractionTest();
                break;
            case (short)InstructionsState.ShowEndingMessage:
                PresentEndingMessages();
                break;
            case (short)InstructionsState.EndInstructionsProcess:
                EndInstructionsProcess();
                break;
        }

        PresentMessage();
    }


    void PresentWelcomeMessages()
    {
        ActiveMessageCategory = "welcome";
        SkippableMessages = true;
    }

    void PresentCameraInstructions()
    {
        ActiveMessageCategory = "camera instructions";
        SkippableMessages = true;
    }

    void PerformCameraMovementTest()
    {
        ActiveMessageCategory = "camera test";
        SkippableMessages = false;

        DirectionArrowImagePlanes[0].gameObject.SetActive(true);
        LookDirectionArrow = DirectionArrowImagePlanes[0];

        ObjectsToCheckIfVisible[0].SetActive(true);
        ObjectToFind = ObjectsToCheckIfVisible[0];

        ObjectToFind.transform.parent.rotation = Camera.main.transform.rotation;

        CheckObjectVisibility = true;
    }

    void PresentUIInteractionInstructions()
    {
        ActiveMessageCategory = "controls instructions";
        SkippableMessages = true;
    }


    void PerformUIInteractionTest()
    {
        ActiveMessageCategory = "controls test";
        SkippableMessages = false;

        UIInteractionTestActive = true;

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GamepadActive = true;

        int correctButtonId = 2;
        int correctSliderId = 2;
        int correctSliderVal = 4;

        SmartphoneUICanvas.GetComponent<InstructionsUISimBehavior>().DefineCorrectTestValues(correctButtonId, correctSliderId, correctSliderVal);

        SmartphoneUICanvas.GetComponent<InstructionsUISimBehavior>().EnableButtonInteractivity();
    }

    void PresentEndingMessages()
    {
        SmartphoneUICanvas.GetComponent<InstructionsUISimBehavior>().DisableButtonInteractivity();


        ActiveMessageCategory = "ending";
        SkippableMessages = true;

    }

    void EndInstructionsProcess()
    {

    }


    void PresentMessage()
    {
        if (MessagePresentIndex >= MessagesDict[ActiveMessageCategory].Count)
        {
            MessagePresentIndex = 0;
            //NextUpdateState();
            UpdateState = true;
            return;
        }

        string messageToPresent = MessagesDict[ActiveMessageCategory][MessagePresentIndex];

        InstructionsUIObj.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = messageToPresent;

        if (ActiveMessageCategory.Equals("controls instructions"))
        {
            PresentImageToMatchText();
        }

        MessagePresentIndex++;

    }

    void PresentMessage(string messageCategory, short messageIndex)
    {
        string messageToPresent = MessagesDict[messageCategory][messageIndex];

        InstructionsUIObj.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<TMP_Text>().text = messageToPresent;

        MessagePresentIndex = messageIndex;
    }

    /// <summary>
    /// Enabling / disabling specific objects based on which text is presented. Hardcoded to work with the controls instructions, to present indicators of specific buttons when the text describes them.
    /// </summary>
    void PresentImageToMatchText()
    {
        List<int> imageIndexesToEnable = new List<int>();
        List<int> imageIndexesToDisable = new List<int>();

        switch (MessagePresentIndex)
        {
            case 1:
                SmartphoneUIObject.GetComponent<Renderer>().enabled = true;
                SmartphoneUICanvas.GetComponent<Canvas>().enabled = true;

                // To demonstrate UI not moving with the camera.
                SmartphoneUIObject.transform.parent = SmartphoneUIObject.transform.parent.parent;

                break;
            case 3:
                imageIndexesToEnable.Add(0);
                imageIndexesToEnable.Add(1);
                imageIndexesToEnable.Add(2);
               // StartCoroutine(MenuNavigationDelay(1f));
                break;
            case 4:
                imageIndexesToDisable.Add(2);
                imageIndexesToEnable.Add(3);
              //  StartCoroutine(SliderDemonstrationDelay(0.5f));
                break;
            case 5:
                imageIndexesToDisable.Add(2);
                imageIndexesToDisable.Add(3);
                imageIndexesToEnable.Add(4);
                imageIndexesToEnable.Add(5);
                break;
            case 6:
                //imageIndexesToDisable.Add(5);
                imageIndexesToEnable.Add(6);
             //   StartCoroutine(SmartphoneToggleDelay(1f));
                break;
        }


        foreach (int i in imageIndexesToEnable)
        {
            UIInteractionTestVisualsDisplayOrder[i].SetActive(true);
        }

        foreach (int i in imageIndexesToDisable)
        {
            UIInteractionTestVisualsDisplayOrder[i].SetActive(false);
        }

    }

    public void UIInteractionTestResult(bool result)
    {
        if (result)
        {
            PresentMessage("controls test", 2);
            MessagePresentIndex++;

            SkippableMessages = true;
        }
        else
        {
            PresentMessage("controls test", 1);
        }

    }

    IEnumerator ObjDisplayDelay(GameObject obj, float seconds)    
    {
        yield return new WaitForSeconds(seconds);

        obj.SetActive(!obj.activeSelf);
    }

    IEnumerator MenuNavigationDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().SetSelected(SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedIndex() + 1);

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().SetSelected(SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedIndex() + 1);

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().SetSelected(SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedIndex() - 1);
    }

    IEnumerator SliderDemonstrationDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedObj().GetComponentInChildren<Slider>().value++;

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedObj().GetComponentInChildren<Slider>().value++;

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedObj().GetComponentInChildren<Slider>().value++;

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedObj().GetComponentInChildren<Slider>().value--;

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.GetComponent<UIGamepadInteraction>().GetCurrentlySelectedObj().GetComponentInChildren<Slider>().value--;
    }

    IEnumerator SmartphoneToggleDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.transform.parent.GetComponent<Renderer>().enabled = false;
        SmartphoneUICanvas.GetComponent<Canvas>().enabled = false;

        yield return new WaitForSeconds(seconds);

        SmartphoneUICanvas.transform.parent.GetComponent<Renderer>().enabled = true;
        SmartphoneUICanvas.GetComponent<Canvas>().enabled = true;
    }
    

}
