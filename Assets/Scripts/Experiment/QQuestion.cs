using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QQuestion
{
    public string questionText;
    public int[] acceptableResponseRange;

    /// <summary>
    /// Create a new empty QQuestion. 
    /// </summary>
    public QQuestion()
    {
        questionText = "";
        acceptableResponseRange = new int[2];
    }

    public QQuestion(string text, int[] range)
    {
        questionText = text;
        acceptableResponseRange = range;
    }
}

/// <summary>
/// Class holding an array of ChatMessage. 
/// </summary>
[System.Serializable]
public class Questionnaire
{
    public QQuestion[] qQuestions;
}