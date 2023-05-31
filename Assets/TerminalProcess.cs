using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

public class TerminalProcess
{
    private readonly Process process;
    public bool started { get; private set; }
    public bool exited { get; private set; }
    private StringBuilder outputBuilder;

    public TerminalProcess()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "C:\\Program Files\\Git\\git-cmd.exe",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            // Arguments = " -c \"" + argument + " \""
        };
        this.outputBuilder = new StringBuilder();
        this.process = new Process
        {
            StartInfo = startInfo
        };
        this.process.OutputDataReceived += this.OutputHandler;
    }

    // TODO: create extra constructors

    public async void Start()
    {
        this.started = true;
        await Task.Run(() => {
            this.process.Start();
            this.process.BeginOutputReadLine();
            UnityEngine.Debug.Log("Starting process...");
        });
    }

    // TODO: the model is this. let the user write input in whenever they want
    //       set up a listener so that the display is updated whenever
    //       there's a detected change in the output stream. the two happen independently
    //       of one another

    public void WriteInput(string inputString)
    {
        if (!String.IsNullOrEmpty(inputString))
        {
            this.process.StandardInput.WriteLine(inputString);
        }
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        UnityEngine.Debug.Log("Firing OutputHandler: outLine.data = [" + outLine.Data + "]");
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            this.outputBuilder.Append(Environment.NewLine + outLine.Data);
        }
    }

    public async Task<string> ReadOutput()
    {
        string output = await this.process.StandardOutput.ReadToEndAsync();
        return output;
    }

    // returns true if the task is successfully killed. false otherwise
    public bool Exit()
    {
        if (!this.started)
        {
            return false;
        }

        if (this.exited)
        {
            return false;
        }

        this.process.StandardInput.Close();
        this.process.Close();
        this.exited = true;
        UnityEngine.Debug.Log(this.outputBuilder);
        return true;
    }
}
