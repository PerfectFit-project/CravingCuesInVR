using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LogInManager : MonoBehaviour
{
    public GameObject NetworkManager;

    public GameObject DropdownMenu;
    public GameObject UserNameInputField;
    public GameObject PasswordLabel;
    public GameObject PasswordInputField;
    public GameObject UserNameWarningLabel;
    public GameObject PasswordWarningLabel;

    public string password;

    private void Start()
    {

    }

    public void CheckDropDownSelection()
    {
        Debug.Log("Checking Dropdown selection.");
        Debug.Log("Current value: " + DropdownMenu.GetComponent<TMP_Dropdown>().value);

        Debug.Log(DropdownMenu.transform.GetChild(0).GetComponent<TMP_Text>().text);

        if (DropdownMenu.transform.GetChild(0).GetComponent<TMP_Text>().text.Equals("Researcher")) 
        {
            PasswordLabel.SetActive(true);
            PasswordInputField.SetActive(true);
        }
        else if (PasswordLabel.activeSelf == true)
        {
            PasswordLabel.SetActive(false);
            PasswordInputField.SetActive(false);
            PasswordWarningLabel.SetActive(false);
        }
    }

    public void LogInUser()
    {
        bool error = false;

        string userName = UserNameInputField.GetComponent<TMP_InputField>().text;
        bool isResearcher = (PasswordInputField.activeSelf) ? true : false;

        if (String.IsNullOrWhiteSpace(userName))
        {
            // Add username checking if necessary 
            UserNameWarningLabel.SetActive(true);
            error = true;
        }

        if (PasswordInputField.activeSelf && (!PasswordInputField.GetComponent<TMP_InputField>().text.Equals(password) || String.IsNullOrWhiteSpace(PasswordInputField.GetComponent<TMP_InputField>().text)))
        {
            PasswordWarningLabel.GetComponent<TMP_Text>().text = "Invalid Password";
            
            if (String.IsNullOrWhiteSpace(PasswordInputField.GetComponent<TMP_InputField>().text))
            {
                PasswordWarningLabel.GetComponent<TMP_Text>().text = "Cannot Be Empty";
            }

            PasswordWarningLabel.SetActive(true);
            error = true;
        }
        
        if (error)
        {
            return;
        }

        NetworkManager.GetComponent<CCVRNetworkManager>().ResolvePlayerLogIn(userName, isResearcher);
        

        transform.gameObject.SetActive(false);
    }

    //public void ResolvePlayerLogIn(string playerName, bool isResearcher)
    //{
    //    PlayerName = playerName;

    //    //userInterfacePrefab = (isResearcher) ? researcherUIPrefab : participantUIPrefab;

    //    if (isResearcher)
    //    {
    //        Debug.Log("IS RESEARCHER");
    //        userInterfacePrefab = researcherUIPrefab;
    //        StartClient();
    //    }
    //    else
    //    {
    //        Debug.Log("IS PARTICIPANT");
    //        userInterfacePrefab = participantUIPrefab;
    //        StartHost();
    //    }
    //}


}
