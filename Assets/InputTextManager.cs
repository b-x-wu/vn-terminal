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
        // TODO: is there a prettier way to do this?
        this.displayTextManager = GameObject.FindGameObjectWithTag("DisplayText").GetComponent<DisplayTextManager>();
        this.inputField = this.GetComponent<InputField>();
        this.inputField.onEndEdit.AddListener(this.AcceptStringInput);

        this.terminalProcess = new TerminalProcess();
        this.terminalProcess.StandardOutputReceived += this.StandardOutputReceivedHandler;
        this.terminalProcess.Start();
    }

    void AcceptStringInput(string userInput)
    {
        this.terminalProcess.WriteInput(userInput);

        this.inputField.text = "";
        this.inputField.ActivateInputField();
    }

    void StandardOutputReceivedHandler(object sender, string message)
    {
        this.displayTextManager.ReceiveOutput(message);
    }

    void OnApplicationQuit()
    {
        this.terminalProcess.Exit();
    }
}
