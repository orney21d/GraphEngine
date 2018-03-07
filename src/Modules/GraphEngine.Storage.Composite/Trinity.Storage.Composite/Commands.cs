﻿using System;
using System.Diagnostics;
using System.IO;
using Trinity.Diagnostics;
using Trinity.Utilities;

namespace Trinity.Storage.Composite
{
    // Command tool configurations
    public static class Commands
    {
        //private static string c_codegen_cmd => Path.Combine(AssemblyUtility.MyAssemblyPath, "..", "..", "runtime", "Trinity.TSL.CodeGen");
        private static string c_codegen_cmd => "Trinity.TSL.CodeGen";
        private static string c_dotnet_cmd => "dotnet";

        public static bool TSLCodeGenCmd(string arguments)
        {
            try
            {
                var nuget_proc = _System("dotnet", "nuget locals global-packages -l");
                CmdCall(c_codegen_cmd, arguments);
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error, "{0}", e.Message);
                return false;
            }
            return true;
        }

        public static bool DotNetBuildCmd(string arguments)
        {
            try
            {
                CmdCall(c_dotnet_cmd, arguments);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private static void CmdCall(string cmd, string arguments)
        {
            Log.WriteLine("command:  " + cmd + " " + arguments);
            Process proc = _System(cmd, arguments);
            proc.OutputDataReceived += OnChildStdout;
            proc.ErrorDataReceived += OnChildStderr;
            proc.BeginOutputReadLine();
            proc.WaitForExit();
        }

        private static Process _System(string cmd, string arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();
            return proc;
        }

        private static void OnChildStdout(object sender, DataReceivedEventArgs e)
            => OnChildOutputImpl(sender as Process, e.Data, LogLevel.Info);

        private static void OnChildStderr(object sender, DataReceivedEventArgs e)
            => OnChildOutputImpl(sender as Process, e.Data, LogLevel.Error);

        private static void OnChildOutputImpl(Process process, string data, LogLevel logLevel)
        {
            string name = "N/A";
            try { name = process?.ProcessName; } catch { };
            Log.WriteLine(logLevel, $"{nameof(Commands)}: {name}: {{0}}", data);
        }
    }

    // Settings of storage path
    internal class PathHelper
    {
        private const string FolderName = "composite-helper";
        public static string Directory => FileUtility.CompletePath(Path.Combine(TrinityConfig.StorageRoot, FolderName), create_nonexistent: true);
        public static string VersionRecorders => Path.Combine(Directory, "VersionRecorders.bin");
        public static string CellTypeIDs => Path.Combine(Directory, "CellTypeIDs.bin");
        public static string IDIntervals => Path.Combine(Directory, "IDIntervals.bin");
        public static string DLL(string dllName) => Path.Combine(Directory, dllName);
    }
}
