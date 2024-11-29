using UnityEngine;
using TMPro;

public class DisplayValue : MonoBehaviour
{
    public TMP_Text textToChange;
    public string suffix = "";
    public string decimalPlaces = "0.0";

    public void ChangeDisplay(float value)
    {
        textToChange.text = value.ToString(decimalPlaces) + suffix;
    }
}