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
    private Image continueArrow;
    private Queue<Message> messageBuffer = new Queue<Message>();
    private TerminalProcess terminalProcess;
    private string currentLine = "";
    public float repeatRate;
    private ControllerState controllerState = ControllerState.ReadyForUserInput;
    public Color standardOutputColor = Color.white;
    public Color standardErrorColor = Color.red;
    public List<Sprite> standardOutputCharacterSprites;
    public List<Sprite> standardErrorCharacterSprites;
    public Sprite canvasBackgroundSprite;
    private string workingDirectory;

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
        continueArrow = GameObject.Find("ContinueArrow").GetComponent<Image>();
        continueArrow.color = Color.clear;
        inputField.onEndEdit.AddListener(HandleInputFieldInput);

        ReadConfig();
        GameObject.Find("CanvasBackground").GetComponent<Image>().sprite = canvasBackgroundSprite;

        terminalProcess = new TerminalProcess(workingDirectory);
        terminalProcess.StandardOutputReceived += HandleStandardOutputReceived;
        terminalProcess.StandardErrorReceived += HandleStandardErrorReceived;
        terminalProcess.Start();
    }

    private void ReadConfig()
    {
        ConfigManager.ConfigData configData = ConfigManager.ReadData();
        repeatRate = configData.repeatRate;
        standardOutputColor = configData.standardOutputColor;
        standardErrorColor = configData.standardErrorColor;
        if (configData.standardOutputCharacterSprites != null && configData.standardOutputCharacterSprites.Count > 0)
        {
            standardOutputCharacterSprites = configData.standardOutputCharacterSprites;
        }
        if (configData.standardErrorCharacterSprites != null && configData.standardErrorCharacterSprites.Count > 0)
        {
            standardErrorCharacterSprites = configData.standardErrorCharacterSprites;
        }
        canvasBackgroundSprite = configData.canvasBackgroundSprite ?? canvasBackgroundSprite;
        workingDirectory = configData.workingDirectory;
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
            continueArrow.color = Color.clear;
            return;
        }

        inputField.DeactivateInputField();
        if (controllerState == ControllerState.PrintingLine) {
            continueArrow.color = Color.clear;
            return;
        }

        if (controllerState == ControllerState.ReadyForUserContinue)
        {
            continueArrow.color = Color.white;
            if (messageBuffer.Count == 0)
            {
                controllerState = Input.GetKey("space") ? ControllerState.ReadyForUserContinue : ControllerState.ReadyForUserInput;
                return;
            }

            controllerState = Input.GetKey("space") ? ControllerState.ReadyForPrintLine : ControllerState.ReadyForUserContinue;
            return;
        }

        // controllerState == ControllerState.ReadyForPrintLine
        continueArrow.color = Color.clear;
        DisplayCurrentLine();
    }

    private void WriteConfig()
    {
        ConfigManager.ConfigData configData = new ConfigManager.ConfigData()
        {
            repeatRate = repeatRate,
            standardOutputColor = standardOutputColor,
            standardErrorColor = standardErrorColor,
            standardOutputCharacterSprites = standardOutputCharacterSprites,
            standardErrorCharacterSprites = standardErrorCharacterSprites,
            canvasBackgroundSprite = canvasBackgroundSprite,
            workingDirectory = workingDirectory,
        };
        ConfigManager.WriteData(configData);
    }

    private void OnApplicationQuit()
    {
        terminalProcess.Exit();
        WriteConfig();
    }
}
