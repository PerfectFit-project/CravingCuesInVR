using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deprecated: Initial solution to adjusting the tiling of panoramic textures, so that they appear correctly. 
/// </summary>
public class FixTextureTiling : MonoBehaviour
{
    public float scaleFactor = 100f;

    public void DoIt()
    {
        Debug.Log(GetComponent<Renderer>().material.mainTextureScale.x + " " + GetComponent<Renderer>().material.mainTextureScale.y);
        Debug.Log(GetComponent<Renderer>().material.mainTexture.dimension);
        Debug.Log(GetComponent<Renderer>().material.mainTexture.width);
        Debug.Log(GetComponent<Renderer>().material.mainTexture.height);
        //GetComponent<Renderer>().material.mainTextureScale = new Vector2(transform.localScale.x / scaleFactor, transform.localScale.z / scaleFactor);
        // GetComponent<Renderer>().material.mainTextureScale = new Vector2(transform.localScale.x / GetComponent<Renderer>().material.mainTextureScale.x, transform.localScale.y / GetComponent<Renderer>().material.mainTextureScale.y);
    }
}
