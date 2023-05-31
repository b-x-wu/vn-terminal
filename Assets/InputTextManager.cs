using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.MPE;

public class InputTextManager : MonoBehaviour
{
    InputField inputField;
    DisplayTextManager displayTextManager;
    TerminalProcess terminalProcess;

    private void Awake()
    {
        this.displayTextManager = GameObject.FindGameObjectWithTag("DisplayText").GetComponent<DisplayTextManager>();
        this.inputField = this.GetComponent<InputField>();
        this.inputField.onEndEdit.AddListener(this.AcceptStringInput);
        this.terminalProcess = new TerminalProcess();
        this.terminalProcess.Start();
    }

    void AcceptStringInput(string userInput)
    {
        displayTextManager.PrintStringToDisplay("Working on the process. Input Text: [" + this.inputField.text + "]");

        this.terminalProcess.WriteInput(userInput);

        displayTextManager.PrintStringToDisplay("Waiting on output");
        this.inputField.text = "";
        this.inputField.ActivateInputField();
    }

    void OnApplicationQuit()
    {
        this.terminalProcess.Exit();
    }
}
