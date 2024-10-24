namespace GameServer;

    [Serializable]
    public class ChatConfigFile
    {
        public bool EnableMoTD = false;
        
        public string MessageOfTheDay = "Remember to drink water";
        
        public bool LoginNotifications = false;
        
        public bool DisconnectNotifications = false;
        
        public bool EndGameNotifications = false;
        
        public bool BroadcastEndGameNotifications = false;
    }