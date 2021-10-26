using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Performs form-checking and initializes the network user login process.
/// </summary>
public class LogInManager : MonoBehaviour
{
    public GameObject NetworkManager;

    public GameObject DropdownMenu;
    public GameObject UserNameInputField;
    public GameObject PasswordLabel;
    public GameObject PasswordInputField;
    public GameObject UserNameWarningLabel;
    public GameObject PasswordWarningLabel;

    // For simple checking so that participants can't log in as researchers. Shouldn't be an issue for the intended experiments, but probably replace with a more secure solution if wanting to implement a remote application.
    public string password;

    /// <summary>
    /// Enable the password field if the Researcher dropdown menu option is selected.
    /// </summary>
    public void CheckDropDownSelection()
    {
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

    /// <summary>
    /// Perform form checking and if everything is in order, have the Network Manager initiate a Player object creation process.
    /// </summary>
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


}
