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

    GameObject ResponsesContainerScrollBar;

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
        ResponsesContainerScrollBar = newMessageWithResponses.transform.GetChild(0).GetChild(1).GetChild(1).gameObject;
        //ContentContainer = newMessageWithResponses.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).gameObject;

        //ContentContainer = newMessageWithResponses.transform.GetChild(0).GetChild(1).gameObject;

        ContentContainer = newMessageWithResponses.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(0).gameObject;

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
        if (WaitingForMessageResponse && ResponsesContainerScrollBar)
        {
            //ContainerScrollBar.GetComponent<Scrollbar>().Select();
            ResponsesContainerScrollBar.GetComponent<Scrollbar>().Select();
        }

        var gamepad = Gamepad.current;

        if (gamepad != null)
            GamepadDetected = true;


        if (!GamepadActive || !GamepadDetected)        
            return;

        // Left Trigger on the Gamepad makes the UI appear / disappear.
        if (gamepad.yButton.wasPressedThisFrame)
        {
            // Making the relevant objects invisible instead of disabling them, so that the attached scripts, this one included, continue to work even when the objects aren't visible.
            transform.parent.transform.GetComponent<MeshRenderer>().enabled = !transform.parent.transform.GetComponent<MeshRenderer>().enabled;
            transform.parent.transform.GetChild(0).GetComponent<Canvas>().enabled = !transform.parent.transform.GetChild(0).GetComponent<Canvas>().enabled;
        }

        // Navigate up or down on the ScrollRect without changing object selection
        if (gamepad.leftStick.up.wasPressedThisFrame && !gamepad.dpad.up.wasPressedThisFrame)
        {
            ContainerScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition += 0.5f * Time.deltaTime;
        }
        else if (gamepad.leftStick.down.wasPressedThisFrame && !gamepad.dpad.down.wasPressedThisFrame)
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
                WaitingForMessageResponse = false;
                ResponsesContainerScrollBar = null;
                selectedObject.GetComponentInChildren<Button>().onClick.Invoke();                
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
    /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
    /// </summary>
    /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    private int CountCornersVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }

    /// <summary>
    /// Determines if this RectTransform is fully visible from the specified camera.
    /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public bool IsFullyVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
    }

    /// <summary>
    /// Determines if this RectTransform is at least partially visible from the specified camera.
    /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public bool IsVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
    }


    /// <summary>
    /// Adjusts the position of the scrollbar so that the currently selected Gameobject is visible.
    /// </summary>
    private void AdjustScrollBarPosition()
    {
        Canvas.ForceUpdateCanvases();

        //if (IsFullyVisibleFrom(selectedObject.GetComponent<RectTransform>(), Camera.current))
        // return;

        RectTransform contentContainer = ContentContainer.GetComponent<RectTransform>();

        //RectTransform contentContainer = ContentContainer.transform.parent.parent.parent.GetComponent<RectTransform>();

        RectTransform selectedRectTransform = selectedObject.GetComponent<RectTransform>();
       // ScrollRect scrollRect = ContainerScrollView.GetComponent<ScrollRect>();
       // RectTransform scrollWindow = scrollRect.GetComponent<RectTransform>();

        ScrollRect scrollRect = ContentContainer.transform.parent.parent.GetComponent<ScrollRect>();
        RectTransform scrollWindow = scrollRect.GetComponent<RectTransform>();
        //RectTransform scrollWindow = ContentContainer.transform.parent.parent.GetComponent<RectTransform>();

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


    //public IEnumerator WaitForSeconds(float seconds)
    //{
    //    yield return new WaitForSeconds(seconds);

    //    ContainerScrollView.GetComponent<ScrollRect>().verticalScrollbar.value = 1;
    //    ContainerScrollView.transform.GetChild(2).GetComponent<Scrollbar>().value = 1;

    //    ContainerScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    //}

}