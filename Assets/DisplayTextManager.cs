using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class DisplayTextManager : MonoBehaviour
{
    private Text displayTextObject;
    private string currentText = "";
    private Queue<string> outputBuffer;
    public float repeatRate;

    private void Awake()
    {
        this.displayTextObject = gameObject.GetComponent<Text>();
        this.outputBuffer = new Queue<string>();
    }


    public void ReceiveOutput(string displayString)
    {
        Task.Run(() => {
            lock (this.outputBuffer)
            {
                this.outputBuffer.Enqueue(displayString);
            }
        });
    }

    private void DisplayCurrentCharacter()
    {
        if (this.currentText == "") { return; }
        string characterToDisplay = this.currentText.Substring(0, 1);
        this.displayTextObject.text += characterToDisplay;
        this.currentText = this.currentText.Remove(0, 1);
    }

    void Update()
    {
        if (this.displayTextObject == null) { return; }
        if (this.outputBuffer.Count == 0) { return; }
        if (this.currentText != "") { return; }

        this.displayTextObject.text = "";
        this.currentText = this.outputBuffer.Dequeue();
        InvokeRepeating("DisplayCurrentCharacter", 0.0f, repeatRate);

        // this.displayTextObject.text = this.currentText;
    }
}
