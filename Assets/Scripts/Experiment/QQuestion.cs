using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QQuestion
{
    public string descriptionText;
    public string questionText;
    public string[] acceptableResponseRange;
    public string[] extremeRangeLabels;

    /// <summary>
    /// Create a new empty QQuestion. 
    /// </summary>
    public QQuestion()
    {
        descriptionText = "";
        questionText = "";
        acceptableResponseRange = new string[2];
        extremeRangeLabels = new string[2];
    }

    public QQuestion(string dText, string qText, string[] range, string[] rangeLabels)
    {
        descriptionText = dText;
        questionText = qText;
        acceptableResponseRange = range;
        extremeRangeLabels = rangeLabels;
    }
}

/// <summary>
/// Class holding an array of ChatMessage. 
/// </summary>
[System.Serializable]
public class Questionnaire
{
    public string qTitle;
    public QQuestion[] qQuestions;
}