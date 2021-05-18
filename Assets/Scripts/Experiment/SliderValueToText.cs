using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class SliderValueToText : MonoBehaviour
{
    public Slider slider;

    public void UpdateSliderValueText()
    {
        this.GetComponent<Text>().text = slider.value.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
