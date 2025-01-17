using Shared;

namespace GameClient.Values
{
    public static class ServerValues
    {
        public static bool isAdmin;

        public static bool hasFaction;

        public static void SetValues(ServerGlobalData serverGlobalData)
        {
            isAdmin = serverGlobalData._isClientAdmin;

            hasFaction = serverGlobalData._isClientFactionMember;
        }

        public static void CleanValues()
        {
            isAdmin = false;

            hasFaction = false;
        }
    }
}
