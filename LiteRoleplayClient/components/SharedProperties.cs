namespace LiteRoleplayClient
{
    public class SharedProperties
    {
        //Chat properties
        public const string ChatPrefix = "[LiteRP]";

        //Chat colors
        public static int[] ColorNormal = { 24, 127, 221 };
        public static int[] ColorGood = { 101, 253, 142 };
        public static int[] ColorWarning = { 255, 153, 42 };
        public static int[] ColorError = { 238, 58, 67 };

        //Defaukts
        public static int SalaryTimer = 60;

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
