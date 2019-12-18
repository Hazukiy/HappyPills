using LiteRoleplayServer.Models.Economy;

namespace LiteRoleplayServer
{
    public class SharedProperties
    {
        //Auth
        public const string OwnerString = "e5afcd512615765bda314b00b587e7d76259331c";

        //Database / tables
        public const string DatabaseName = "LiteRP.db";
        public const string DatabaseTableInfo = "PlayerInfo";
        public const string DatabaseTableProfile = "PlayerProfile";
        public const string DatabaseTableAdmins = "Admins";
        public const string DatabaseTableBanned = "BannedPlayers";

        //Chat properties
        public const string ChatPrefix = "[LiteRP]";

        //Chat colors
        public static int[] ColorNormal = { 24, 127, 221 };
        public static int[] ColorGood = { 101, 253, 142 };
        public static int[] ColorWarning = { 255, 153, 42 };
        public static int[] ColorError = { 238, 58, 67 };

        //Default player profile
        public static int DefaultWallet = 0;
        public static int DefaultBank = 300; //$300
        public static int DefaultSalary = 2; //$2 every minute
        public static int DefaultSalaryCount = 60; //1 minute
        public const string DefaultJob = JobsModel.Unemployed;

        //Server -> Client Events/Callbacks
        public const string ProfileCallback = "sendProfile";

        //Client - > Server Events/Callbacks
        public const string EventLoadProfile = "netLoadProfile";
        public const string EventSaveProfile = "netSaveProfile";
        public const string EventInvokeOwnership = "netInvokeOwnership";
        public const string EventBanPlayer = "netBanPlayer";
        public const string EventUnbanPlayer = "netUnbanPlayer";
    }
}
