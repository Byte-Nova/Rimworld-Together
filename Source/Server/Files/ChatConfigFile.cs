using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer;

    public class ChatConfigFile
    {
        public bool EnableMoTD = false;
        public string MessageOfTheDay = "";
        public bool LoginNotifications = false;
        public bool DisconnectNotifications = false;
    }