using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Collects username and initiates the local experiment login process.
/// </summary>
public class LogInManagerLC : MonoBehaviour
{
    public GameObject UserNameInputField;
    public GameObject UserNameWarningLabel;

    public GameObject PlayerLC;

    public void LogInUser()
    {
        bool error = false;

        string userName = UserNameInputField.GetComponent<TMP_InputField>().text;

        if (String.IsNullOrWhiteSpace(userName))
        {
            // Add username checking if necessary 
            UserNameWarningLabel.SetActive(true);
            error = true;
        }              

        if (error)
        {
            return;
        }

        PlayerLC.GetComponent<ExperimentRun>().LogUserIn(userName);
    }


}
