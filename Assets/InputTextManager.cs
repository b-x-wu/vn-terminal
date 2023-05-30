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

    private void Awake()
    {
        this.displayTextManager = GameObject.FindGameObjectWithTag("DisplayText").GetComponent<DisplayTextManager>();
        this.inputField = this.GetComponent<InputField>();
        this.inputField.onEndEdit.AddListener(this.AcceptStringInput);
    }

    void AcceptStringInput(string userInput)
    {
        displayTextManager.PrintStringToDisplay("Working on the process. Input Text: [" + this.inputField.text + "]");

        string output = this.ExecuteProcessTerminal("ls");

        UnityEngine.Debug.Log(output);
        displayTextManager.PrintStringToDisplay("Valid and worked. Output: [" + output + "]");
        this.inputField.text = "";
        this.inputField.ActivateInputField();
    }

    private string ExecuteProcessTerminal(string argument)
    {
        try
        {
            UnityEngine.Debug.Log("============== Start Executing [" + argument + "] ===============");
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "C:\\Program Files\\Git\\git-cmd.exe",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                // CreateNoWindow = true,
                Arguments = " -c \"" + argument + " \""
            };
            Process myProcess = new Process
            {
                StartInfo = startInfo
            };
            myProcess.Start();
            UnityEngine.Debug.Log("Right after start");
            string output = myProcess.StandardOutput.ReadToEnd();
            myProcess.WaitForExit();
            UnityEngine.Debug.Log("============== End ===============");

            return output;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
            return null;
        }
    }
}
