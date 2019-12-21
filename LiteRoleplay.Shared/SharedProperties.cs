using CitizenFX.Core;
using System.Collections.Generic;

namespace LiteRoleplay.Shared
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

        //Server -> Client Events/Callbacks
        public const string ProfileCallback = "sendProfile";
        public const string AdminCallback = "sendAdmin";
        public const string JobsCallback = "sendJob";

        //Client - > Server Events/Callbacks
        public const string EventLoadProfile = "netLoadProfile";
        public const string EventSaveProfile = "netSaveProfile";
        public const string EventInvokeOwnership = "netInvokeOwnership";
        public const string EventBanPlayer = "netBanPlayer";
        public const string EventUnbanPlayer = "netUnbanPlayer";
        public const string EventFreezePlayer = "netFreezePlayer";
        public const string EventKickPlayer = "netKickPlayer";
        public const string EventAddBan = "netAddBan";
        public const string EventGiveAdmin = "netGiveAdmin";
        public const string EventRemoveAdmin = "netRemoveAdmin";
        public const string EventDepositWallet = "netDepositWallet";
        public const string EventChangeJob = "netChangeJob";

        public static string[] AllWeapons = new string[]
        {
            "WEAPON_KNIFE",
            "WEAPON_NIGHTSTICK",
            "WEAPON_HAMMER",
            "WEAPON_BAT",
            "WEAPON_GOLFCLUB",
            "WEAPON_CROWBAR",
            "WEAPON_PISTOL",
            "WEAPON_COMBATPISTOL",
            "WEAPON_APPISTOL",
            "WEAPON_PISTOL50",
            "WEAPON_MICROSMG",
            "WEAPON_SMG",
            "WEAPON_ASSAULTSMG",
            "WEAPON_ASSAULTRIFLE",
            "WEAPON_CARBINERIFLE",
            "WEAPON_ADVANCEDRIFLE",
            "WEAPON_MG",
            "WEAPON_COMBATMG",
            "WEAPON_PUMPSHOTGUN",
            "WEAPON_SAWNOFFSHOTGUN",
            "WEAPON_ASSAULTSHOTGUN",
            "WEAPON_BULLPUPSHOTGUN",
            "WEAPON_STUNGUN",
            "WEAPON_SNIPERRIFLE",
            "WEAPON_HEAVYSNIPER",
            "WEAPON_GRENADELAUNCHER",
            "WEAPON_GRENADELAUNCHER_SMOKE",
            "WEAPON_RPG",
            "WEAPON_MINIGUN",
            "WEAPON_GRENADE",
            "WEAPON_STICKYBOMB",
            "WEAPON_SMOKEGRENADE",
            "WEAPON_BZGAS",
            "WEAPON_MOLOTOV",
            "WEAPON_FIREEXTINGUISHER",
            "WEAPON_PETROLCAN",
            "WEAPON_FLARE",
            "WEAPON_SNSPISTOL",
            "WEAPON_SPECIALCARBINE",
            "WEAPON_HEAVYPISTOL",
            "WEAPON_BULLPUPRIFLE",
            "WEAPON_HOMINGLAUNCHER",
            "WEAPON_PROXMINE",
            "WEAPON_SNOWBALL",
            "WEAPON_VINTAGEPISTOL",
            "WEAPON_DAGGER",
            "WEAPON_FIREWORK",
            "WEAPON_MUSKET",
            "WEAPON_MARKSMANRIFLE",
            "WEAPON_HEAVYSHOTGUN",
            "WEAPON_GUSENBERG",
            "WEAPON_HATCHET",
            "WEAPON_RAILGUN",
            "WEAPON_COMBATPDW",
            "WEAPON_KNUCKLE",
            "WEAPON_MARKSMANPISTOL",
            "WEAPON_FLASHLIGHT",
            "WEAPON_MACHETE",
            "WEAPON_MACHINEPISTOL",
            "WEAPON_SWITCHBLADE",
            "WEAPON_REVOLVER",
            "WEAPON_COMPACTRIFLE",
            "WEAPON_DBSHOTGUN",
            "WEAPON_FLAREGUN",
            "WEAPON_AUTOSHOTGUN",
            "WEAPON_BATTLEAXE",
            "WEAPON_COMPACTLAUNCHER",
            "WEAPON_MINISMG",
            "WEAPON_PIPEBOMB",
            "WEAPON_POOLCUE",
            "WEAPON_SWEEPER",
            "WEAPON_WRENCH"
        };

        //Job profiles
        public static JobsModel JobUnemployed = new JobsModel()
        {
            JobID = 1,
            JobName = "Unemployed",
            Models = new string[] { "a_f_m_skidrow_01", "a_m_o_acult_02", "a_m_o_soucent_02", "a_m_o_soucent_03", "a_m_o_tramp_01" },
            Salary = 2,
            IsPolice = false,
            IsAdmin = false,
            SpawnWeapons = new string[]{ "WEAPON_KNIFE" }
        };

        public static JobsModel JobPolice = new JobsModel()
        {
            JobID = 2,
            JobName = "Police Officer",
            Models = new string[] { "s_m_y_ranger_01", "s_m_y_sheriff_01", "s_m_y_cop_01" },
            Salary = 15,
            IsPolice = false,
            IsAdmin = false,
            SpawnWeapons = new string[] { "WEAPON_NIGHTSTICK", "WEAPON_PISTOL", "WEAPON_STUNGUN", "WEAPON_FLASHLIGHT" }
        };

        public static JobsModel JobAdmin = new JobsModel()
        {
            JobID = 3,
            JobName = "Admin",
            Models = new string[] { "u_m_y_juggernaut_01" },
            Salary = 50,
            IsPolice = false,
            IsAdmin = true,
            SpawnWeapons = AllWeapons
        };

        public static List<JobsModel> AllJobs = new List<JobsModel>()
        {
            JobUnemployed,
            JobPolice,
            JobAdmin
        };

        //Player profile
        public static int DefaultWallet = 0;
        public static int DefaultBank = 300;
        public static int DefaultSalaryTimer = 60;
        public static int DefaultJob = JobUnemployed.JobID;
        public static int DefaultSalary = JobUnemployed.Salary;

        //public const string JobPresident = "President";
        //public const string JobMayor = "Mayor";
        //public const string JobPoliceChief = "Police Chief";
        //public const string JobPoliceOfficer = "Police Officer";
        //public const string JobBountyHunter = "Bounty Hunter";
        //public const string JobParamedic = "Paramedic";
        //public const string JobFireman = "Fireman";
        //public const string JobMechanic = "Mechanic";
        //public const string JobChef = "Chef";
        //public const string JobStreetCleaner = "Street Cleaner";
        //public const string JobStreetRacer = "Street Racer";
        //public const string JobDrugLord = "Drug Lord";
        //public const string JobDrugMember = "Drug Member";

        //Spawn Locations 
        public static float[] DefaultSpawn = { -1329.704f, -1512.436f, 4.379375f, 1.0f }; //On venice beach

        //General locations (NameOfLocation_AreaOfLocation)
        public static float[] PoliceStation_Atlee_St = { 440.9918f, -981.0921f, 30.9896f }; //City PD
        public static float[] ATM_Vespucci_Blvd = { 147.0587f, -1035.49f, 29.34372f }; //City bank

        public static float[][] Location_ATM = { ATM_Vespucci_Blvd };
        public static float[][] Location_Police = { PoliceStation_Atlee_St };
        //Custom methods
        public static ProfileModel ConvertToProfile(dynamic obj)
        {
            return new ProfileModel()
            {
                Id = obj.Id,
                LicenseID = obj.LicenseID,
                Wallet = obj.Wallet,
                Bank = obj.Bank,
                Salary = obj.Salary,
                Job = obj.Job,
                IsWanted = obj.IsWanted,
                IsAdmin = obj.IsAdmin
            };
        }
    }
}
