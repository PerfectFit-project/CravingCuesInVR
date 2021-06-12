using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIControllerInteraction : MonoBehaviour
{
    public GameObject ContentContainer;
    public GameObject ContainerScrollView;
    public GameObject ContainerScrollBar;
    
    [SerializeField]
    private bool ControllerActive;

    List<Transform> gameObjectsToTraverse;
    int selectedObjIndex;

    private GameObject selectedObject;

    private void Start()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null)
        {
            ControllerActive = false;
            //Debug.Log("NO GAMEPAD");
            return;
        }
        else
        {
            ControllerActive = true;
        }

        //gameObjectsToTraverse = GetComponentsInChildren<Transform>();
        int childrenCount = ContentContainer.transform.childCount;
        gameObjectsToTraverse = new List<Transform>();

        for (int i = 0; i < childrenCount; i++)
        {
            gameObjectsToTraverse.Add(ContentContainer.transform.GetChild(i));
        }

        selectedObjIndex = 0;
        SetSelected(0, selectedObjIndex);

    }

    private void Update()
    {
        if (!ControllerActive)        
            return;        


        var gamepad = Gamepad.current;

        if (gamepad.dpad.down.wasPressedThisFrame)
        {
            Debug.Log("DPAD DOWN PRESSED");
            int previousSelectedObjIndex = selectedObjIndex;

            selectedObjIndex++;

            if (selectedObjIndex >= gameObjectsToTraverse.Count)
            {
                selectedObjIndex = 0;
            }

            SetSelected(previousSelectedObjIndex, selectedObjIndex);
        }
        else if (gamepad.dpad.up.wasPressedThisFrame)
        {
            Debug.Log("DPAD UP PRESSED");

            int previousSelectedObjIndex = selectedObjIndex;

            selectedObjIndex--;

            if (selectedObjIndex < 0)
            {
                selectedObjIndex = gameObjectsToTraverse.Count - 1;
            }

            SetSelected(previousSelectedObjIndex, selectedObjIndex);

        }
        else if (gamepad.dpad.left.wasPressedThisFrame)
        {
            Debug.Log("DPAD LEFT PRESSED");
            if (selectedObject.GetComponentInChildren<Slider>())
            {
                selectedObject.GetComponentInChildren<Slider>().value = selectedObject.GetComponentInChildren<Slider>().value - 1;
            }
        }
        else if (gamepad.dpad.right.wasPressedThisFrame)
        {
            Debug.Log("DPAD RIGHT PRESSED");
            if (selectedObject.GetComponentInChildren<Slider>())
            {
                selectedObject.GetComponentInChildren<Slider>().value = selectedObject.GetComponentInChildren<Slider>().value + 1;
            }
        }
        else if (gamepad.aButton.wasPressedThisFrame)
        {
            Debug.Log("A ButtonPRESSED");
            if (selectedObject.GetComponentInChildren<Button>())
            {
                selectedObject.GetComponentInChildren<Button>().onClick.Invoke();
                ResetView();
            }
        }

    }

    private void ResetView()
    {
        // TODO RESET SELECTION BEFORE BEING DISABLED SO THAT THE NEXT TIME THE SCRIPT IS ACTIVATED THE FIRST OBJECT IS SELECTED
        int previousSelectedObjIndex = selectedObjIndex;
        selectedObjIndex = 0;
        SetSelected(previousSelectedObjIndex, selectedObjIndex);
    }

    private void AdjustScrollBarPosition()
    {
        RectTransform selection = selectedObject.GetComponent<RectTransform>();

        ScrollRect scrollRect = ContainerScrollView.GetComponent<ScrollRect>();
        RectTransform scrollWindow = scrollRect.GetComponent<RectTransform>();

        float selectionPosition = -selection.anchoredPosition.y - (selection.rect.height * (1 - selection.pivot.y));

        float elementHeight = selection.rect.height;
        float maskHeight = scrollWindow.rect.height;
        float listAnchorPosition = scrollRect.content.anchoredPosition.y;

        float offlimitsValue = 0;
        if (selectionPosition < listAnchorPosition + (elementHeight / 2))
        {
            offlimitsValue = (listAnchorPosition + maskHeight) - (selectionPosition - elementHeight);
        }
        else if (selectionPosition + elementHeight > listAnchorPosition + maskHeight)
        {
            offlimitsValue = (listAnchorPosition + maskHeight) - (selectionPosition + elementHeight);
        }

        // move the target scroll rect
        scrollRect.verticalNormalizedPosition +=
            (offlimitsValue / scrollRect.content.rect.height) * Time.deltaTime * 5000f;

        if (scrollRect.verticalNormalizedPosition < 0)
            scrollRect.verticalNormalizedPosition = 0;

        if (scrollRect.verticalNormalizedPosition > 1)
            scrollRect.verticalNormalizedPosition = 1;

    }

    private void SetSelected(int previousIndex, int index)
    {
        DeselectObject();

        selectedObject = gameObjectsToTraverse[index].gameObject;

        selectedObject.GetComponent<Image>().color = new Color(0f, 60/255f, 1f, 70/255f);

        AdjustScrollBarPosition();
    }

    private void DeselectObject()
    {
        if (selectedObject == null)
            return;

        Color currentColor = selectedObject.GetComponent<Image>().color;
        selectedObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        
        selectedObject = null;
    }


}