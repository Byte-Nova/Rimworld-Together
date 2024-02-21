using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimworldTogether.GameClient.Misc
{
    public static class Logs
    {

        private static string LogFile;
        private static string LogFilePath;


        private static string InstanceListFile;

        public static void Message(string message)
        {
            //Write message to player.log file
            Log.Message(message);

            //write message to (player name).log file located in RimWorld by Ludeon Studios\Rimworld Together
            writeMessage(message);
        }

        public static void Warning(string message)
        {
            //Write warning to player.log file
            Log.Warning(message);

            //write message to (player name).log file located in RimWorld by Ludeon Studios\Rimworld Together
            writeMessage(message);
        }

        public static void Error(string message, bool ignoreStopLoggingLimit = true)
        {
            //Write warning to player.log file
            Log.Error(message, ignoreStopLoggingLimit);

            //write message to (player name).log file located in RimWorld by Ludeon Studios\Rimworld Together
            writeMessage(message);
        }

        private static void writeMessage(string message)
        {
            using (StreamWriter w = File.AppendText(LogFilePath))
            {
                w.WriteLine(message);
            }

        }

        public static void prepareFileName(string ModFolder)
        {

            //get Instance file path
            InstanceListFile = Path.Combine(ModFolder, "InstanceList.txt");

            Dictionary<string, int> IdDict = new Dictionary<string, int>();
            if (File.Exists(InstanceListFile))
            {
                //for each file, check if another instance of rimworld is using it,
                //if not then set that file as the log file to write to
                IdDict = getLogCheckout(InstanceListFile);
                foreach (string file in IdDict.Keys){

                    //check if the process is currently running
                    if (Process.GetProcessById(IdDict[file]).HasExited){
                        IdDict.Remove(file);
                        LogFile = file;
                        LogFilePath = Path.Combine(ModFolder, file);
                        break;
                    }
                    //check if the process which is running is a rimworld process
                    else if (Process.GetProcessById(IdDict[file]).ProcessName != Process.GetCurrentProcess().ProcessName)
                    {
                        IdDict.Remove(file);
                        LogFile = file;
                        LogFilePath = Path.Combine(ModFolder, file);
                        break;
                    }
                
                }

                //if no Log file is available,
                //set LogFilePath to a new log file
                if (LogFilePath == null)
                {
                    LogFile = $"Log{IdDict.Count + 1}.log";
                    LogFilePath = Path.Combine(ModFolder, LogFile);
                }

                //update the Instance List. 
                using (StreamWriter w = File.CreateText(InstanceListFile))
                {
                    foreach (string file in IdDict.Keys)
                    {
                        w.WriteLine($"{file} {IdDict[file]}");
                    }

                    w.WriteLine($"{LogFile} {Process.GetCurrentProcess().Id}");
                }

                //erase the log file
                File.Delete(LogFilePath);
            }
            //if the instance file does not exist, create it and add in the first entry
            else
            {
                using (StreamWriter w = File.CreateText(InstanceListFile))
                {
                    w.WriteLine($"Log1.log {Process.GetCurrentProcess().Id}");
                }
            }
        }

        private static Dictionary<string,int> getLogCheckout(string InstanceListFile)
        {
            string[] Text = new String[2];
            string logFile;
            Dictionary<string, int> IdDict = new Dictionary<string, int>();

            using (StreamReader w = File.OpenText(InstanceListFile))
            {
                while (!w.EndOfStream)
                {
                    Text[1] = w.ReadLine();
                    if (Text[1] != null)
                    {
                        Text = Text[1].Split(' ');
                        logFile = Text[0];
                        Int32.TryParse(Text[1], out int Id);
                        IdDict[logFile] = Id;
                    }
                }
            }
            return IdDict;
        }


    }
}
