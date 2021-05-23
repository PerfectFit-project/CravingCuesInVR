using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveCollectedDataLC : MonoBehaviour
{
    string UserName;
    // < Environemnt identifier, < Question number, Question response >> 
    Dictionary<string, Dictionary<int, int>> QuestionnaireResponses;

    private void Start()
    {
        QuestionnaireResponses = new Dictionary<string, Dictionary<int, int>>();
    }

    public void StoreDataToCollection(string envId, Dictionary<int, int> responses)
    {
        QuestionnaireResponses.Add(envId, responses);
    }

    public void SaveDataToFile(string userName)
    {
        string headerTextToWrite = "environment ID, ";
        bool headerFinished = false;
        string contentTextToWrite = "";
        int entriesCurrentlyInHeader = 0;

        foreach (string env in QuestionnaireResponses.Keys)
        {
            contentTextToWrite = contentTextToWrite + env + ", ";

            foreach (int question in QuestionnaireResponses[env].Keys)
            {
                // Construct the header based on question numbers, the first time we iterate inside the dictionary. Assuming that the same questions are asked in each environment. Will cause issues otherwise.
                if (!headerFinished)
                {
                    //if (question != QuestionnaireResponses[env][QuestionnaireResponses[env].Keys.Count])
                    //{
                    //    headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + ", ";
                    //}
                    //else
                    //{
                    //    headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + "\n";
                    //    headerFinished = true;
                    //}

                    if (entriesCurrentlyInHeader < QuestionnaireResponses[env].Keys.Count - 1)
                    {
                        headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + ", ";
                    }
                    else
                    {
                        headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + "\n";
                        headerFinished = true;
                    }
                    entriesCurrentlyInHeader++;

                }

                //if (question != QuestionnaireResponses[env][QuestionnaireResponses[env].Keys.Count])
                //{
                //    contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][question].ToString() + ", ";
                //} 
                //else
                //{
                //    contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][question].ToString() + "\n";
                //}
                contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][question].ToString() + ", ";
            }
        }

        Debug.Log(headerTextToWrite);
        Debug.Log(contentTextToWrite);

        //string filePath = Application.dataPath + "/SavedData/" + userName + "_responses.csv";
        string filePath = Application.streamingAssetsPath + "/SavedData/" + userName + "_responses.csv";

        FileInfo fileToWrite = new FileInfo(filePath);
        fileToWrite.Directory.Create();


        StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine(headerTextToWrite + contentTextToWrite);
        writer.Flush();
        writer.Close();
    }

  

}
