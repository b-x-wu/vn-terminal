using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ControllerScript : MonoBehaviour
{
    private InputField inputField;
    private Text outputField;
    private Queue<string> outputBuffer = new Queue<string>();
    TerminalProcess terminalProcess;
    private string currentText = "";
    public float repeatRate;

    private void Awake()
    {
        inputField = GameObject.FindGameObjectWithTag("TextInput").GetComponent<InputField>();
        outputField = GameObject.FindGameObjectWithTag("TextOutput").GetComponent<Text>();
        inputField.onEndEdit.AddListener(HandleInputFieldInput);

        terminalProcess = new TerminalProcess();
        this.terminalProcess.StandardOutputReceived += HandleStandardOutputReceived;
        terminalProcess.Start();
    }

    private void HandleInputFieldInput(string inputString)
    {
        terminalProcess.WriteInput(inputString);
        // TODO: set state?
        inputField.text = "";
        inputField.DeactivateInputField(); // TODO: activate it again when the output buffer frees up
    }

    private void HandleStandardOutputReceived(object sender, string standardOutputString)
    {
        // TODO: does this need to be in a task?
        lock (outputBuffer)
        {
            outputBuffer.Enqueue(standardOutputString);
        }
    }

    private void DisplayCurrentCharacter()
    {
        if (this.currentText == "") { return; }
        string characterToDisplay = currentText.Substring(0, 1);
        outputField.text += characterToDisplay;
        currentText = currentText.Remove(0, 1);
    }

    private void DisplayCurrentText()
    {
        InvokeRepeating("DisplayCurrentCharacter", 0.0f, repeatRate);
    }

    private void Update()
    {
        if (outputField == null) { return; }
        if (outputBuffer.Count == 0) { return; }
        if (currentText != "") { return; } // current text printing already invoked

        outputField.text = "";
        currentText = outputBuffer.Dequeue();
        DisplayCurrentText();
    }

    private void OnApplicationQuit()
    {
        terminalProcess.Exit();
    }
}
