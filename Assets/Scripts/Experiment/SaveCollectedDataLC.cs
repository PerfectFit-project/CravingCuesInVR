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
        string headerTextToWrite = "env_ID, ";
        bool headerFinished = false;
        string contentTextToWrite = "";

        foreach (string env in QuestionnaireResponses.Keys)
        {
            int entriesRead = 0;
            contentTextToWrite = contentTextToWrite + env + ", ";

            foreach (int question in QuestionnaireResponses[env].Keys)
            {
                // Construct the header based on question numbers, the first time we iterate inside the dictionary. Assuming that the same questions are asked in each environment. Will cause issues otherwise.
                if (entriesRead < QuestionnaireResponses[env].Keys.Count - 1)
                {
                    contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][question].ToString() + ", ";

                    if (!headerFinished)
                    {
                        headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + ", ";
                    }
                }
                else
                {
                    contentTextToWrite = contentTextToWrite + QuestionnaireResponses[env][question].ToString() + "\n";

                    if (!headerFinished)
                    {
                        headerTextToWrite = headerTextToWrite + "Q_" + question.ToString() + "\n";
                        headerFinished = true;
                    }
                }

                entriesRead++;
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
