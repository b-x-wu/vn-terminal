using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputTextManager : MonoBehaviour
{
    InputField inputField;
    GameObject displayTextObject;
    DisplayTextManager displayTextManager;

    private void Awake()
    {
        this.displayTextObject = GameObject.FindGameObjectWithTag("DisplayText");
        this.displayTextManager = this.displayTextObject.GetComponent<DisplayTextManager>();
        this.inputField = this.GetComponent<InputField>();
        this.inputField.onEndEdit.AddListener(this.AcceptStringInput);
    }

    void AcceptStringInput(string userInput)
    {
        displayTextManager.PrintStringToDisplay(userInput);
        this.inputField.text = "";
        this.inputField.ActivateInputField();
    }
}
