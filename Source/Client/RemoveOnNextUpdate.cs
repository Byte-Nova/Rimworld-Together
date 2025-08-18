using GameClient.Core;
using GameClient.Core.Preferences;
using GameClient.Files;
using GameClient.Misc;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient
{
    public static class RemoveOnNextUpdate
    {
        public static void FixSpacesInUsername()
        {
            if (!File.Exists(Master.LoginDataPath)) return;
            else
            {
                LoginDataFile file = Serializer.SerializeFromFile<LoginDataFile>(Master.LoginDataPath);
                if (file.Username.Contains(' '))
                {
                    Printer.Warning($"Username '{file.Username}' contained spaces, fixing");
                    file.Username = file.Username.Replace(' ', '_');
                    UserLoginHandler.SaveLoginData(file);
                }
            }
        }
    }
}
