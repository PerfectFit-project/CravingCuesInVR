using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Store questionnaire responses to a local dictionary, and to a JSON file.
/// </summary>
public class SaveCollectedDataLC : MonoBehaviour
{
    Dictionary<string, Dictionary<string, Dictionary<int, int>>> QuestionnaireResponses;

    private void Start()
    {
        QuestionnaireResponses = new Dictionary<string, Dictionary<string, Dictionary<int, int>>>();
    }

    /// <summary>
    /// Save questionnaire responses to gloal Dictionary.
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="qId"></param>
    /// <param name="responses"></param>
    public void StoreDataToCollection(string envId, string qId, Dictionary<int, int> responses)
    {
        Dictionary<string, Dictionary<int, int>> responseQs = new Dictionary<string, Dictionary<int, int>>();
        responseQs.Add(qId, responses);

        if (QuestionnaireResponses.ContainsKey(envId))
        {
            QuestionnaireResponses[envId].Add(qId, responses);
        }
        else
        {
            QuestionnaireResponses.Add(envId, responseQs);
        }

        SaveDataToFile(transform.GetComponent<ExperimentRun>().UserName);
    }

    /// <summary>
    /// Save global Dictionary data to JSON file.
    /// </summary>
    /// <param name="userName"></param>
    public void SaveDataToFile(string userName)
    {
        string headerTextToWrite = "env_ID, ";
        bool headerFinished = false;
        string contentTextToWrite = "";

        
        foreach (string env in QuestionnaireResponses.Keys)
        {
            int qTypesRead = 0;

            contentTextToWrite = contentTextToWrite + env + ", ";

            foreach (string qType in QuestionnaireResponses[env].Keys)
            {
                int questionsRead = 0;
                
                foreach (int question in QuestionnaireResponses[env][qType].Keys)
                {
                    // Construct the header based on question numbers, the first time we iterate inside the dictionary. Assuming that the same questions are asked in each environment. Will cause issues otherwise.
                    if (questionsRead < QuestionnaireResponses[env][qType].Keys.Count - 1)
                    {
                        contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][qType][question].ToString() + ", ";

                        if (!headerFinished)
                        {
                            headerTextToWrite = headerTextToWrite + qType + "_Q_" + question.ToString() + ", ";
                        }
                    }
                    else
                    {
                        if (qTypesRead >= QuestionnaireResponses[env].Keys.Count - 1)
                        {
                            contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][qType][question].ToString() + "\n";

                            if (!headerFinished)
                            {
                                headerTextToWrite = headerTextToWrite + qType + "_Q_" + question.ToString() + "\n";
                                headerFinished = true;
                            }
                        }
                        else
                        {
                            contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][qType][question].ToString() + ", ";

                            if (!headerFinished)
                            {
                                headerTextToWrite = headerTextToWrite + qType + "_Q_" + question.ToString() + ", ";
                            }
                        }
                    }
                    questionsRead++;
                }
                qTypesRead++;
            }

            
        }

        Debug.Log(headerTextToWrite);
        Debug.Log(contentTextToWrite);

        string filePath = Application.streamingAssetsPath + "/SavedData/" + userName + "_responses" + ".csv";

        FileInfo fileToWrite = new FileInfo(filePath);
        fileToWrite.Directory.Create();


        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine(headerTextToWrite + contentTextToWrite);
        writer.Flush();
        writer.Close();

    }

  

}
