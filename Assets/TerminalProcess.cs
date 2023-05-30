using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class TerminalProcess
{
    private Process process;
    public bool started;
    public bool exited;

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
        this.process = new Process
        {
            StartInfo = startInfo
        };
    }

    // TODO: create extra constructors

    public async void Start()
    {
        this.started = true;
        await Task.Run(() => {
            this.process.Start();
            UnityEngine.Debug.Log("Starting process...");
        });
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

        this.process.Close();
        this.exited = true;
        return true;
    }
}
