using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Transforms the numeric Slider value to text.
/// </summary>
public class SliderValueToText : MonoBehaviour
{
    public Slider slider;

    public void UpdateSliderValueText()
    {
        this.GetComponent<TMP_Text>().text = slider.value.ToString();
    }
}
