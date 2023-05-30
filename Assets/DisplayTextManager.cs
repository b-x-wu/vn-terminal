using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTextManager : MonoBehaviour
{
    Text displayText;

    private void Awake()
    {
        this.displayText = GetComponent<Text>();
    }
    public void PrintStringToDisplay(string displayString)
    {
        this.displayText.text = displayString;
    }
}
