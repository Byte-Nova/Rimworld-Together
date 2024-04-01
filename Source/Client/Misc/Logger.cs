﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Verse;
using static Shared.CommonEnumerators;

namespace GameClient
{
    public static class Logger
    {
        public static void WriteToConsole(string message, LogMode mode)
        {
            if (!ClientValues.verboseBool) return;

            string toPrint = $"[Rimworld Together] > {message}";

            switch (mode)
            { 
                case LogMode.Message:
                    Log.Message(toPrint);
                    break;

                case LogMode.Warning:
                    Log.Warning(toPrint);
                    break;

                case LogMode.Error:
                    Log.Error(toPrint);
                    break;
            }

            //Write message to (player name).log file
            //LoggerHelper.WriteMessage(message);
        }
    }

    //public static class LoggerHelper
    //{
    //    private static string LogFile;
    //    private static string LogFilePath;
    //    private static string InstanceListFile;

    //    private static void WriteMessage(string message)
    //    {
    //        using (StreamWriter streamWriter = File.AppendText(LogFilePath))
    //        {
    //            streamWriter.WriteLine(message);
    //        }
    //    }

    //    public static void prepareFileName(string ModFolder)
    //    {
    //        Log.Message("preparing Logger InstanceList");
    //        //get Instance file path
    //        InstanceListFile = Path.Combine(ModFolder, "InstanceList.txt");

    //        Log.Message("Finding free Log file");
    //        Dictionary<string, int> IdDict = new Dictionary<string, int>();
    //        if (File.Exists(InstanceListFile))
    //        {
    //            //for each file, check if another instance of rimworld is using it,
    //            //if not then set that file as the log file to write to
    //            IdDict = getLogCheckout(InstanceListFile);
    //            foreach (string file in IdDict.Keys)
    //            {

    //                //check if the process is currently running
    //                if (Process.GetProcessById(IdDict[file]).HasExited)
    //                {
    //                    Log.Message("Free log file found");
    //                    IdDict.Remove(file);
    //                    LogFile = file;
    //                    LogFilePath = Path.Combine(ModFolder, file);
    //                    break;
    //                }
    //                //check if the process which is running is a rimworld process
    //                else if (Process.GetProcessById(IdDict[file]).ProcessName != Process.GetCurrentProcess().ProcessName)
    //                {
    //                    Log.Message("Free log file found");
    //                    IdDict.Remove(file);
    //                    LogFile = file;
    //                    LogFilePath = Path.Combine(ModFolder, file);
    //                    break;
    //                }

    //            }

    //            //if no Log file is available,
    //            //set LogFilePath to a new log file
    //            if (LogFilePath == null)
    //            {
    //                Log.Message("Created a new log file");
    //                LogFile = $"Log{IdDict.Count + 1}.log";
    //                LogFilePath = Path.Combine(ModFolder, LogFile);
    //            }

    //            //update the Instance List. 
    //            Log.Message("Updating Instance List");
    //            using (StreamWriter w = File.CreateText(InstanceListFile))
    //            {
    //                foreach (string file in IdDict.Keys)
    //                {
    //                    w.WriteLine($"{file} {IdDict[file]}");
    //                }

    //                w.WriteLine($"{LogFile} {Process.GetCurrentProcess().Id}");
    //            }

    //            //erase the log file
    //            File.Delete(LogFilePath);
    //        }
    //        //if the instance file does not exist, create it and add in the first entry
    //        else
    //        {
    //            Log.Message("Creating Instance List");
    //            Log.Message("Created new log file");
    //            using (StreamWriter w = File.CreateText(InstanceListFile))
    //            {
    //                w.WriteLine($"Log1.log {Process.GetCurrentProcess().Id}");
    //            }
    //        }
    //        Log.Message("Successfully prepared log paths");
    //        Log.Message($"Log path: {LogFilePath}");
    //        return;
    //    }

    //    private static Dictionary<string, int> getLogCheckout(string InstanceListFile)
    //    {
    //        string[] Text = new String[2];
    //        string logFile;
    //        Dictionary<string, int> IdDict = new Dictionary<string, int>();

    //        using (StreamReader w = File.OpenText(InstanceListFile))
    //        {
    //            while (!w.EndOfStream)
    //            {
    //                Text[1] = w.ReadLine();
    //                if (Text[1] != null)
    //                {
    //                    Text = Text[1].Split(' ');
    //                    logFile = Text[0];
    //                    Int32.TryParse(Text[1], out int Id);
    //                    IdDict[logFile] = Id;
    //                }
    //            }
    //        }
    //        return IdDict;
    //    }
    //}
}