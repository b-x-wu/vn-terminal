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
        ReadyForPrintLine,
        PrintingLine,
    }
    private InputField inputField;
    private Text outputField;
    private Queue<string> outputBuffer = new Queue<string>();
    TerminalProcess terminalProcess;
    private string currentLine = "";
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
        terminalProcess.WriteInput(inputString);
        inputField.text = "";
    }

    private void HandleStandardOutputReceived(object sender, string standardOutputString)
    {
        if (controllerState == ControllerState.ReadyForUserInput) { controllerState = ControllerState.ReadyForPrintLine; }
        lock (outputBuffer)
        {
            outputBuffer.Enqueue(standardOutputString);
        }
    }

    private IEnumerator DisplayCurrentLine(int idx)
    {
        if (idx >= currentLine.Length) {
            currentLine = "";
            controllerState = outputBuffer.Count == 0 ? ControllerState.ReadyForUserInput : ControllerState.ReadyForUserContinue;
            yield break;
        }
        
        outputField.text += currentLine[idx];
        yield return new WaitForSeconds(repeatRate);
        yield return DisplayCurrentLine(idx + 1);
    }

    private void DisplayCurrentLine()
    {
        controllerState = ControllerState.PrintingLine;
        outputField.text = "";
        lock (outputBuffer)
        {
            currentLine = outputBuffer.Dequeue();
        }
        StartCoroutine(DisplayCurrentLine(0));
    }

    private void Update()
    {
        if (outputField == null) { return; }

        if (controllerState == ControllerState.ReadyForUserInput) {
            inputField.ActivateInputField();
            return;
        }

        inputField.DeactivateInputField();
        if (controllerState == ControllerState.PrintingLine) {
            return;
        }

        if (controllerState == ControllerState.ReadyForUserContinue)
        {
            controllerState = Input.GetKey("space") ? ControllerState.ReadyForPrintLine : controllerState;
            return;
        }

        // controllerState == ControllerState.ReadyForPrintLine
        DisplayCurrentLine();
    }

    private void OnApplicationQuit()
    {
        terminalProcess.Exit();
    }
}
