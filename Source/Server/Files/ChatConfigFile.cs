namespace GameServer;

    [Serializable]
    public class ChatConfigFile
    {
        public bool EnableMoTD = false;
        
        public string MessageOfTheDay = "Remember to drink water";
        
        public bool LoginNotifications = false;
        
        public bool DisconnectNotifications = false;
    }