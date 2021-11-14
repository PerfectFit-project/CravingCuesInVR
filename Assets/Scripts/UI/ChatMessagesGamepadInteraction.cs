using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles Gamepad interaction with chat message Gameobjects on the Participant UI.
/// </summary>
public class ChatMessagesGamepadInteraction : MonoBehaviour
{
    public GameObject ContentContainer;
    public GameObject ContainerScrollView;
    public GameObject ContainerScrollBar;

    public List<GameObject> ListObjectsToIgnore;
    
    [SerializeField]
    public bool GamepadActive { get; set; }
    private bool GamepadDetected;

    [SerializeField]
    public bool WaitingForMessageResponse { get; set; }

    static List<GameObject> gameObjectsToTraverse;
    int selectedObjIndex;

    private GameObject selectedObject;

    private void Start()
    {
        var gamepad = Gamepad.current;

        if (gamepad == null)        
            GamepadDetected = false;
            //Debug.Log("NO GAMEPAD");
        else        
            GamepadDetected = true;
    }

    /// <summary>
    /// Takes a message with its available responses, and makes those responses the traversible list for the Gamepad.
    /// </summary>
    /// <param name="newMessageWithResponses"></param>
    public void SetNewContainer(GameObject newMessageWithResponses)
    {
        ContentContainer = newMessageWithResponses.transform.GetChild(0).GetChild(1).gameObject;
        InitializeUI();
    }


    public void InitializeUI()
    {
        int childrenCount = ContentContainer.transform.childCount;
        if (childrenCount < 1)
        {
            WaitingForMessageResponse = false;
            GamepadActive = true;
            return;
        }

        gameObjectsToTraverse = new List<GameObject>();
        gameObjectsToTraverse.Clear();

        for (int i = 0; i < childrenCount; i++)
        {
            if (!ListObjectsToIgnore.Contains(ContentContainer.transform.GetChild(i).gameObject))
            {
                gameObjectsToTraverse.Add(ContentContainer.transform.GetChild(i).gameObject);
            }            
        }

        selectedObjIndex = 0;
        SetSelected(selectedObjIndex);

        WaitingForMessageResponse = true;
        GamepadActive = true;
    }

    private void Update()
    {
        // A bit of a hacky solution to have the scrollbar as the selected object through Unity, so that it works correctly. 
        if (WaitingForMessageResponse)
        {
            ContainerScrollBar.GetComponent<Scrollbar>().Select();
        }

        var gamepad = Gamepad.current;

        if (gamepad != null)
            GamepadDetected = true;


        if (!GamepadActive || !GamepadDetected)        
            return;

        // Left Trigger on the Gamepad makes the UI appear / disappear.
        if (gamepad.leftTrigger.wasPressedThisFrame)
        {
            // Making the relevant objects invisible instead of disabling them, so that the attached scripts, this one included, continue to work even when the objects aren't visible.
            transform.parent.transform.GetComponent<MeshRenderer>().enabled = !transform.parent.transform.GetComponent<MeshRenderer>().enabled;
            transform.parent.transform.GetChild(0).GetComponent<Canvas>().enabled = !transform.parent.transform.GetChild(0).GetComponent<Canvas>().enabled;
        }

        // Navigate up or down on the ScrollRect without changing object selection
        if (gamepad.leftStick.up.wasPressedThisFrame)
        {
            ContainerScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition += 0.5f * Time.deltaTime;
        }
        else if (gamepad.leftStick.down.wasPressedThisFrame)
        {
            ContainerScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition -= 0.5f * Time.deltaTime;
        }

        // Up or Down on the DPad changes object selection.
        if (gamepad.dpad.down.wasPressedThisFrame)        
        {
            selectedObjIndex++;

            if (selectedObjIndex >= gameObjectsToTraverse.Count)
            {
                selectedObjIndex = 0;
            }

            SetSelected(selectedObjIndex);
        }
        else if (gamepad.dpad.up.wasPressedThisFrame)
        {
            selectedObjIndex--;

            if (selectedObjIndex < 0)
            {
                selectedObjIndex = gameObjectsToTraverse.Count - 1;
            }

            SetSelected(selectedObjIndex);

        }
        // Right or Left on the DPad adjust the Slider value, if a Slider object is the current selection.
        else if (gamepad.dpad.left.wasPressedThisFrame)
        {
            if (selectedObject.GetComponentInChildren<Slider>())
            {
                selectedObject.GetComponentInChildren<Slider>().value = selectedObject.GetComponentInChildren<Slider>().value - 1;
            }
        }
        else if (gamepad.dpad.right.wasPressedThisFrame)
        {
            if (selectedObject.GetComponentInChildren<Slider>())
            {
                selectedObject.GetComponentInChildren<Slider>().value = selectedObject.GetComponentInChildren<Slider>().value + 1;
            }
        }
        // A button on the Gamepad calls the Invoke function on a Button, if a Button object is the current selection.
        else if (gamepad.aButton.wasPressedThisFrame)
        {
            if (selectedObject.GetComponentInChildren<Button>())
            {
                selectedObject.GetComponentInChildren<Button>().onClick.Invoke();
                WaitingForMessageResponse = false;
                GamepadActive = false;
            }
        }       


    }
        
    public void ClearUI()
    {
        DeselectObject();
        selectedObject = null;
        selectedObjIndex = 0;
 
        gameObjectsToTraverse.Clear();
    }


    /// <summary>
    /// Adjusts the position of the scrollbar so that the currently selected Gameobject is visible.
    /// </summary>
    private void AdjustScrollBarPosition()
    {
        Canvas.ForceUpdateCanvases();


        RectTransform contentContainer = ContentContainer.GetComponent<RectTransform>();

        RectTransform selectedRectTransform = selectedObject.GetComponent<RectTransform>();
        ScrollRect scrollRect = ContainerScrollView.GetComponent<ScrollRect>();
        RectTransform scrollWindow = scrollRect.GetComponent<RectTransform>();

        float scrollPadding = 75f;

        float scrollViewMinY = contentContainer.anchoredPosition.y + scrollPadding;
        float scrollViewMaxY = contentContainer.anchoredPosition.y + scrollWindow.rect.height - scrollPadding;

        float selectionPadding = 10f;

        float selectedPositionY = Mathf.Abs(selectedRectTransform.anchoredPosition.y) + (selectedRectTransform.rect.height / 2);

        // If selection below scroll view
        if (selectedPositionY > scrollViewMaxY)
        {
            float newY = (selectedPositionY - scrollWindow.rect.height);
            contentContainer.anchoredPosition = new Vector2(contentContainer.anchoredPosition.x, newY + selectionPadding);
        }
        // If selection above scroll view
        else if (Mathf.Abs(selectedRectTransform.anchoredPosition.y) < scrollViewMinY)
        {
            contentContainer.anchoredPosition =
                new Vector2(contentContainer.anchoredPosition.x, Mathf.Abs(selectedRectTransform.anchoredPosition.y)
                - (selectedRectTransform.rect.height / 2) - selectionPadding);
        }

        Canvas.ForceUpdateCanvases();
    }


    /// <summary>
    /// Sets the Gameobject from the list at the incremented / decremented global index as the currently selected object.
    /// </summary>
    public void SetSelected()
    {
        DeselectObject();

        if (selectedObjIndex >= gameObjectsToTraverse.Count)
        {
            selectedObjIndex = 0;
        }
        else if (selectedObjIndex < 0)
        {
            selectedObjIndex = gameObjectsToTraverse.Count - 1;
        }

        selectedObject = gameObjectsToTraverse[selectedObjIndex].gameObject;

        selectedObject.GetComponent<Image>().color = new Color(0f, 60 / 255f, 1f, 70 / 255f);

        AdjustScrollBarPosition();
    }


    /// <summary>
    /// Sets the Gameobject from the list given an index as the currently selected object.
    /// </summary>
    /// <param name="index"></param>
    public void SetSelected(int index)
    {
        DeselectObject();

        if (index >= gameObjectsToTraverse.Count)
        {
            index = 0;
            selectedObjIndex = index;
        }
        else if (index < 0)
        {
            index = gameObjectsToTraverse.Count - 1;
            selectedObjIndex = index;
        }

        selectedObjIndex = index;
        selectedObject = gameObjectsToTraverse[selectedObjIndex];

        selectedObject.GetComponent<Image>().color = new Color(0f, 60/255f, 1f, 70/255f);

        AdjustScrollBarPosition();
    }


    /// <summary>
    /// Deselects the currently selected object.
    /// </summary>
    public void DeselectObject()
    {
        if (selectedObject == null)
            return;

        //Color currentColor = selectedObject.GetComponent<Image>().color;
        selectedObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        
        selectedObject = null;
    }

    /// <summary>
    /// Returns the index of the currently selected Gameobject
    /// </summary>
    /// <returns></returns>
    public int GetCurrentlySelectedIndex()
    {
        return selectedObjIndex;
    }


    /// <summary>
    /// Returns the currently selected Gameobject
    /// </summary>
    /// <returns></returns>
    public GameObject GetCurrentlySelectedObj()
    {
        return selectedObject;
    }


    public IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        ContainerScrollView.GetComponent<ScrollRect>().verticalScrollbar.value = 1;
        ContainerScrollView.transform.GetChild(2).GetComponent<Scrollbar>().value = 1;

        ContainerScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

}