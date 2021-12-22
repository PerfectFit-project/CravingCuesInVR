using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Class holding a message and its acceptable responses.
/// </summary>
[System.Serializable]
public class ChatMessage : INetworkSerializable
{
    // Would rather have these private and accessible via the methods below, but as far as I've found, it interferes with loading message templates from the JSON file.
    public int senderID;
    public string messageContent;
    public string[] messageResponses;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref messageContent);

        int responsesLength = 0;
        if (!serializer.IsReader)
        {
            responsesLength = messageResponses.Length;
        }

        serializer.SerializeValue(ref responsesLength);

        if (serializer.IsReader)
        {
            messageResponses = new string[responsesLength];
        }

        for (int n = 0; n < responsesLength; ++n)
        {
            serializer.SerializeValue(ref messageResponses[n]);
        }
    }

    /// <summary>
    /// Create a new empty ChatMessage. Default number of responses is 4.
    /// </summary>
    public ChatMessage()
    {
        messageContent = "";
        messageResponses = new string[4];
    }

    /// <summary>
    /// Create a new empty ChatMessage given a specific amount of responses.
    /// </summary>
    /// <param name="responsesCount"></param>
    public ChatMessage(int responsesCount)
    {
        messageContent = "";
        messageResponses = new string[responsesCount];
    }

    /// <summary>
    /// Create a new ChatMessage given a specific message and acceptable responses.
    /// </summary>
    /// <param name="mContent"></param>
    /// <param name="mRresponses"></param>
    public ChatMessage(string mContent, string[] mRresponses)
    {
        messageContent = mContent;
        messageResponses = mRresponses;
    }


    public string MessageContent
    {
        get { return messageContent; }
        set { messageContent = value; }
    }

    public string[] MessageResponses
    {
        get { return messageResponses; }
        set { messageResponses = value; }
    }

    /// <summary>
    /// Extend amount of responses to the specified size.
    /// </summary>
    /// <param name="newSize"></param>
    private void ExtendResponses(int newSize)
    {
        System.Array.Resize(ref messageResponses, newSize);
    }

    /// <summary>
    /// Add a new acceptable response to the given message. Automatically adjusts the size of the responses array first.
    /// </summary>
    /// <param name="responseToAdd"></param>
    public void AddMessageResponse(string responseToAdd)
    {
        ExtendResponses(messageResponses.Length + 1);
        messageResponses[messageResponses.Length - 1] = responseToAdd;
    }

    /// <summary>
    /// Print the message content followed by each of the acceptable responses on the console.
    /// </summary>
    public void PrintMessage()
    {
        Debug.Log("Message: " + messageContent);
        int count = 1;
        foreach (string response in messageResponses)
        {
            Debug.Log("Response " + count + ": " + response);
            count++;
        }
    }

}

/// <summary>
/// Class holding an array of ChatMessage. 
/// </summary>
[System.Serializable]
public class ChatMessages
{
    public ChatMessage[] chatMessages;
}
