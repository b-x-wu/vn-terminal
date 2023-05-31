using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
public class DisplayTextManager : MonoBehaviour
{
    private Text displayTextObject;
    public string currentText;

    private void Awake()
    {
        this.displayTextObject = gameObject.GetComponent<Text>();
    }


    public void PrintStringToDisplay(string displayString)
    {
        this.currentText = displayString;
        Task.Run(() => {
            this.currentText = displayString;
        });
    }

    void Update()
    {
        if (this.displayTextObject != null)
        {
            this.displayTextObject.text = this.currentText;
        }
    }
}
