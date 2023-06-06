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
    public StringBuilder outputBuilder { get; private set; }
    public event EventHandler<string> StandardOutputReceived;
    public event EventHandler<string> StandardErrorReceived;

    public TerminalProcess(string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = @"C:\Windows\System32\cmd.exe",
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        this.outputBuilder = new StringBuilder();
        this.process = new Process
        {
            StartInfo = startInfo
        };
        this.process.OutputDataReceived += this.StandardOutputReceivedHandler;
        this.process.ErrorDataReceived += this.StandardErrorReceivedHandler;
    }

    // TODO: create extra constructors

    public async void Start()
    {
        this.started = true;
        await Task.Run(() => {
            try
            {
                this.process.Start();
                this.process.BeginOutputReadLine();
                this.process.BeginErrorReadLine();
                UnityEngine.Debug.Log("Starting process...");    
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                string ERROR_MESSAGE = "Failed to start process. Invalid config.";
                UnityEngine.Debug.LogError(ERROR_MESSAGE);
                UnityEngine.Debug.LogError(e.Message);
                started = false;
                StandardErrorReceived?.Invoke(this, ERROR_MESSAGE);
                StandardErrorReceived?.Invoke(this, e.Message);
            }
        });
    }

    public void WriteInput(string inputString)
    {
        if (!String.IsNullOrEmpty(inputString))
        {
            this.process.StandardInput.WriteLine(inputString);
        }
    }

    private void StandardOutputReceivedHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            this.outputBuilder.Append(Environment.NewLine + outLine.Data);
            StandardOutputReceived?.Invoke(this, outLine.Data);
        }
    }

    private void StandardErrorReceivedHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            this.outputBuilder.Append(Environment.NewLine + outLine.Data);
            StandardErrorReceived?.Invoke(this, outLine.Data);
        }
    }

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
        this.process.Dispose();
        UnityEngine.Debug.Log(this.outputBuilder);
        return true;
    }
}
