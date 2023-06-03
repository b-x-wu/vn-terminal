using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class ControllerScript : MonoBehaviour
{
    private enum ControllerState
    {
        ReadyForUserInput,
        ReadyForUserContinue,
        PrintingLine,
    }
    private InputField inputField;
    private Text outputField;
    private Queue<string> outputBuffer = new Queue<string>();
    TerminalProcess terminalProcess;
    private string currentText = "";
    public float repeatRate;
    private ControllerState controllerState = ControllerState.ReadyForUserInput;

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
        Debug.Log($"Got input line: [{inputString}]");
        terminalProcess.WriteInput(inputString);
        inputField.text = "";
    }

    private void HandleStandardOutputReceived(object sender, string standardOutputString)
    {
        lock (outputBuffer)
        {
            outputBuffer.Enqueue(standardOutputString);
        }
    }

    private IEnumerator DisplayCurrentLine(int idx)
    {
        if (idx >= currentText.Length) {
            currentText = "";
            controllerState = ControllerState.ReadyForUserContinue;
            yield break;
        }
        
        outputField.text += currentText[idx];
        yield return new WaitForSeconds(repeatRate);
        yield return DisplayCurrentLine(idx + 1);
    }

    private void DisplayCurrentLine()
    {
        controllerState = ControllerState.PrintingLine;
        outputField.text = "";
        lock (outputBuffer)
        {
            currentText = outputBuffer.Dequeue();
        }
        StartCoroutine(DisplayCurrentLine(0));
    }

    private void Update()
    {
        if (outputField == null) { return; }
        if (outputBuffer.Count == 0) { return; }
        if (currentText != "") { return; } // current text printing already invoked

        DisplayCurrentLine();
    }

    private void OnApplicationQuit()
    {
        terminalProcess.Exit();
    }
}
