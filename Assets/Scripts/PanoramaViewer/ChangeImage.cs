using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChangeImage : MonoBehaviour
{
    public bool LoadFromSA;
    public GameObject ImageNameLabel;

    private FileInfo[] allFilesInSA;
    private int currentImageIndex;
    private int filesCount;

    int nonTextureFileCount = 0;

    void Start()
    {
        // Loading files from StreamingAssets folder
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);
        allFilesInSA = directoryInfo.GetFiles("*.*");

        currentImageIndex = 0;
        filesCount = allFilesInSA.Length;

        // For testing purposes 
        if (LoadFromSA)
            NextTexture();
    }

    /// <summary>
    /// Load the next file found in the StreamingAssets folder. Ignoring files the names (+ file extensions) of which contain "meta", "json", "audio". 
    /// </summary>
    public void NextTexture()
    {
        // If no files found, or if every file found so far has one of the regulated keywords, return.
        if (filesCount < 1 || nonTextureFileCount >= filesCount)
            return;

        // Check if the file name contains any of the keywords, and if there are more files to check.
        if ((allFilesInSA[currentImageIndex].Name.Contains("meta") || allFilesInSA[currentImageIndex].Name.Contains("json") || allFilesInSA[currentImageIndex].Name.Contains("audio")) && filesCount > 1)
        {
            nonTextureFileCount++;

            NextTexture();
        }

        nonTextureFileCount = 0;

        StartCoroutine("SetTextureCR", allFilesInSA[currentImageIndex]);

        ImageNameLabel.GetComponent<Text>().text = allFilesInSA[currentImageIndex].Name;

        if (currentImageIndex + 1 < filesCount)
            currentImageIndex++;
        else
            currentImageIndex = 0;
    }

    /// <summary>
    /// Load the texture, assign it to the material attached to the Sphere GameObject, and adjust the material scaling based on its dimensions.
    /// </summary>
    /// <param name="textureFile"></param>
    /// <returns></returns>
    IEnumerator SetTextureCR(FileInfo textureFile)
    {
        string wwwTextureFilePath = "file://" + textureFile.FullName.ToString();
        UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(wwwTextureFilePath);

        yield return webRequest.SendWebRequest();

        Texture2D importedTexture = DownloadHandlerTexture.GetContent(webRequest);

        Material material = GetComponent<Renderer>().material;
        material.mainTexture = importedTexture;

        float dimensionRatio = material.mainTexture.width / material.mainTexture.height;

        // Adjusting the material vertical scaling to make the texture look as intended.
        if (dimensionRatio > 4)
            material.mainTextureScale = new Vector2(1f, 3f);
        else
            material.mainTextureScale = new Vector2(1.5f, 3f);

        transform.GetChild(0).GetComponent<CameraMovement>().UpdateCameraRotationLimits(dimensionRatio);
    }
        
}
