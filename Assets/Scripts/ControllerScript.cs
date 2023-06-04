using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

public class ControllerScript : MonoBehaviour
{
    private enum ControllerState
    {
        ReadyForUserInput,
        ReadyForUserContinue,
        ReadyForPrintLine,
        PrintingLine,
    }
    private enum MessageType
    {
        StdOutput,
        StdError,
    }
    private struct Message
    {
        [ReadOnly]
        public MessageType type;
        public string message;
    }
    private InputField inputField;
    private Text outputField;
    private Image outputImage;
    private Queue<Message> messageBuffer = new Queue<Message>();
    TerminalProcess terminalProcess;
    private string currentLine = "";
    public float repeatRate;
    private ControllerState controllerState = ControllerState.ReadyForUserInput;
    public Color standardOutputColor = Color.white;
    public Color standardErrorColor = Color.red;
    public List<Sprite> standardOutputCharacterSprites;
    public List<Sprite> standardErrorCharacterSprites;
    public static T GetRandomListElement<T>(List<T> list)
    {
        if (list.Count == 0) { return default(T); }
        int idx = Mathf.FloorToInt(Random.value * list.Count);
        if (idx == list.Count) { idx -= 1; } // Random.value is 1 inclusive
        return list[idx];
    }

    private void Awake()
    {
        inputField = GameObject.FindGameObjectWithTag("TextInput").GetComponent<InputField>();
        outputField = GameObject.FindGameObjectWithTag("TextOutput").GetComponent<Text>();
        outputImage = GameObject.FindGameObjectWithTag("Player").GetComponent<Image>();
        inputField.onEndEdit.AddListener(HandleInputFieldInput);

        terminalProcess = new TerminalProcess();
        terminalProcess.StandardOutputReceived += HandleStandardOutputReceived;
        terminalProcess.StandardErrorReceived += HandleStandardErrorReceived;
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
        lock (messageBuffer)
        {
            messageBuffer.Enqueue(new Message{type = MessageType.StdOutput, message = standardOutputString});
        }
    }

    private void HandleStandardErrorReceived(object sender, string standardErrorString)
    {
        if (controllerState == ControllerState.ReadyForUserInput) { controllerState = ControllerState.ReadyForPrintLine; }
        lock (messageBuffer)
        {
            messageBuffer.Enqueue(new Message{ type = MessageType.StdError, message = standardErrorString });
        }
    }

    private IEnumerator DisplayCurrentLine(int idx)
    {
        if (idx >= currentLine.Length) {
            currentLine = "";
            controllerState = ControllerState.ReadyForUserContinue;
            yield break;
        }
        
        outputField.text += currentLine[idx];
        yield return new WaitForSeconds(repeatRate);
        yield return DisplayCurrentLine(idx + 1);
    }

    private void DisplayCurrentLine()
    {
        if (messageBuffer.Count == 0)
        {
            controllerState = ControllerState.ReadyForUserInput;
            return;
        }

        controllerState = ControllerState.PrintingLine;
        outputField.text = "";
        Message currentMessage;
        lock (messageBuffer)
        {
            currentMessage = messageBuffer.Dequeue();
        }
        currentLine = currentMessage.message;
        outputField.color = currentMessage.type == MessageType.StdOutput ? standardOutputColor : standardErrorColor;
        outputImage.sprite = currentMessage.type == MessageType.StdOutput 
            ? GetRandomListElement<Sprite>(standardOutputCharacterSprites) 
            : GetRandomListElement<Sprite>(standardErrorCharacterSprites);
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
            if (messageBuffer.Count == 0)
            {
                controllerState = Input.GetKey("space") ? ControllerState.ReadyForUserContinue : ControllerState.ReadyForUserInput;
                return;
            }

            controllerState = Input.GetKey("space") ? ControllerState.ReadyForPrintLine : ControllerState.ReadyForUserContinue;
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
