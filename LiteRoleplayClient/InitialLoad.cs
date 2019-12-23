using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using LiteRoleplay.Shared;
using static CitizenFX.Core.Native.API;

namespace LiteRoleplayClient
{
    public class InitialLoad : BaseScript
    {
        //Local copy of player profile
        private ProfileModel PlayerProfile { get; set; }

        //Local copy of job profile
        private JobsModel JobProfile { get; set; }

        //Local player variables
        private int PlayerSalaryTimer { get; set; }
        private bool IsMenuOpen { get; set; }
        private int MenuIndex { get; set; }
        private int PowerMultiplier { get; set; }
        private int TorqueMultiplier { get; set; }
        private bool CanBeWanted { get; set; }

        public InitialLoad()
        {
            //Built-in events
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);

            //Custom events
            EventHandlers[SharedProperties.ProfileCallback] += new Action<ProfileModel>(OnPlayerProfileCallback);
            EventHandlers[SharedProperties.AdminCallback] += new Action<int>(OnPlayerGetAdminCallback);
            EventHandlers[SharedProperties.JobsCallback] += new Action<int>(OnPlayerChangeJobCallback);

            //Load player
            InitalisePlayer();

            //Notify
            Console.WriteLine($"[LiteRoleplay v{Assembly.GetExecutingAssembly().GetName().Version}][CLIENT] - Loaded successfully.");
        }

        private void InitalisePlayer()
        {
            //Set locals
            PlayerSalaryTimer = SharedProperties.DefaultSalaryTimer;
            MenuIndex = 1;
            CanBeWanted = true;

            //Start timers
            Tick += PlayerSalaryTick;
            Tick += MenuTick;
            Tick += LocationTick;
            Tick += ModelCheckerTick;
            Tick += HudTick;

            //Load profile
            TriggerServerEvent(SharedProperties.EventLoadProfile);
        }

        #region Events
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            //Player commands
            RegisterCommand("profile", new Action<int, List<object>, string>((source, args, raw) => { Command_GetProfile(); }), false);
            RegisterCommand("deposit", new Action<int, List<object>, string>((source, args, raw) => { Command_DepositWallet(args); }), false);
            RegisterCommand("changejob", new Action<int, List<object>, string>((source, args, raw) => { Command_ChangeJob(args); }), false);
            RegisterCommand("kill", new Action<int, List<object>, string>((source, args, raw) => { Command_Commit(); }), false);
            RegisterCommand("dv", new Action<int, List<object>, string>((source, args, raw) => { Command_DeleteVehicle(); }), false);

            //Cop commands
            RegisterCommand("copcar", new Action<int, List<object>, string>((source, args, raw) => { Command_SpawnCopCar(); }), false);

            //Ban commands
            RegisterCommand("ban", new Action<int, List<object>, string>((source, args, raw) => { Command_BanPlayer(args); }), false);
            RegisterCommand("unban", new Action<int, List<object>, string>((source, args, raw) => { Command_UnBanPlayer(args); }), false);
            RegisterCommand("addban", new Action<int, List<object>, string>((source, args, raw) => { Command_AddBan(args); }), false);

            //Perms
            RegisterCommand("invokeowner", new Action<int, List<object>, string>((source, args, raw) => { Command_InvokeOwner(); }), false);
            RegisterCommand("giveadmin", new Action<int, List<object>, string>((source, args, raw) => { Command_GiveAdmin(args); }), false);
            RegisterCommand("removeadmin", new Action<int, List<object>, string>((source, args, raw) => { Command_RemoveAdmin(args); }), false);

            //Spawning commands
            RegisterCommand("admincar", new Action<int, List<object>, string>((source, args, raw) => { Command_AdminCar(args); }), false);
            RegisterCommand("spawncar", new Action<int, List<object>, string>((source, args, raw) => { Command_SpawnCar(args); }), false);
            RegisterCommand("lockcar", new Action<int, List<object>, string>((source, args, raw) => { Command_LockCar(args); }), false);
            RegisterCommand("torque", new Action<int, List<object>, string>((source, args, raw) => { Command_SetTorque(args); }), false);
            RegisterCommand("power", new Action<int, List<object>, string>((source, args, raw) => { Command_SetPower(args); }), false);
            RegisterCommand("fixcar", new Action<int, List<object>, string>((source, args, raw) => { Command_FixCar(); }), false);

            //World management
            RegisterCommand("settime", new Action<int, List<object>, string>((source, args, raw) => { Command_SetTime(args); }), false);
            RegisterCommand("weather", new Action<int, List<object>, string>((source, args, raw) => { Command_ChangeWeather(args); }), false);
            RegisterCommand("getloc", new Action<int, List<object>, string>((source, args, raw) => { Command_GetLocation(); }), false);
            RegisterCommand("getareaname", new Action<int, List<object>, string>((source, args, raw) => { Command_GetAreaName(); }), false);

            //Self-Player management
            RegisterCommand("freeze", new Action<int, List<object>, string>((source, args, raw) => { Command_FreezePlayer(args); }), false);
            RegisterCommand("god", new Action<int, List<object>, string>((source, args, raw) => { Command_Godmode(args); }), false);
            RegisterCommand("wanted", new Action<int, List<object>, string>((source, args, raw) => { Command_Wanted(args); }), false);
            RegisterCommand("weapons", new Action<int, List<object>, string>((source, args, raw) => { Command_GetAllWeapons(); }), false);
        }

        private void OnPlayerSpawned()
        {
            var player = new Player(GetPlayerIndex());

            //Spawn
            SpawnPlayer(player, JobProfile.SpawnPoint);

            //Give weapons
            GivePlayerJobWeapons();
        }
        #endregion

        #region Custom Events
        public void OnPlayerChangeJobCallback(int jobID)
        {
            if(jobID > 0)
            {
                JobProfile = SharedProperties.AllJobs.Where(x => x.JobID == jobID).FirstOrDefault();

                //Remove weapons from ped
                RemoveAllPedWeapons(GetPlayerPed(GetPlayerIndex()), true);

                //Give weapons
                GivePlayerJobWeapons();

                PrintToChat($"Welcome to your new job: [{JobProfile}]", SharedProperties.ColorGood);
            }
            else
            {
                Debug.WriteLine("An error happened trying to change job.");
            }
        }

        public void OnPlayerGetAdminCallback(int status)
        {
            //Got admin 
            if(status == 1)
            {
                if(PlayerProfile != null)
                {
                    PlayerProfile.IsAdmin = true;
                }
            }

            //Revoked admin
            if(status == 0)
            {
                if (PlayerProfile != null)
                {
                    PlayerProfile.IsAdmin = false;
                }
            }
        }

        public void OnPlayerProfileCallback(dynamic profile)
        {
            if (profile != null)
            {
                ProfileModel newProfile = SharedProperties.ConvertToProfile(profile);
                PlayerProfile = newProfile;

                if(JobProfile == null)
                {
                    JobProfile = SharedProperties.AllJobs.Where(x => x.JobID == PlayerProfile.Job).FirstOrDefault();
                    PrintToChat($"Job Profile Loaded: {JobProfile}", SharedProperties.ColorGood);
                }

                PrintToChat($"Profile Loaded: {PlayerProfile}", SharedProperties.ColorGood);
            }
            else
            {
                Debug.WriteLine("Critial error - failed to load profile.");
                PrintToChat("Failed to load profile.", SharedProperties.ColorError);
            }
        }
        #endregion

        #region Timers
        private async Task HudTick()
        {
            //Main hud
            DisplayHud();

            if(Game.PlayerPed.CurrentVehicle != null)
            {
                DisplayMPH();
            }
        }

        private async Task ModelCheckerTick()
        {
            Player player = new Player(GetPlayerIndex());

            //Ensure player isn't trying to change their skin through exploits
            if(JobProfile != null)
            {
                foreach (var item in JobProfile.Models)
                {
                    var jobHash = GetHashKey(item);
                    if (player.Character.Model.Hash == jobHash)
                    {
                        return;
                    }
                }

                //Change model
                ChangePlayerModel(player);
            }

            await Delay(5000);
        }
        private async Task LocationTick()
        {
            Player player = new Player(GetPlayerIndex());
            if(IsNearAtm(player))
            {
                if(PlayerProfile.Wallet > 0)
                {
                    ShowNotification("Press E to deposit wallet!");
                    if (IsControlJustReleased(1, 38))
                    {
                        TriggerServerEvent(SharedProperties.EventDepositWallet, PlayerProfile.Wallet);
                        
                        //Prevent double action through async processes
                        await Delay(1000);
                    }
                }
            }

            //if(IsNearPoliceStation(player))
            //{
                
            //}

            //await Delay(1000);
        }
        private async Task MenuTick()
        {
            //Wanted 
            if(!CanBeWanted)
            {
                Player player = new Player(GetPlayerIndex());
                ClearPlayerWantedLevel(player.Handle);
            }

            //Draw Menu
            if(IsMenuOpen)
            {
                //Menu control functions


                //DrawMenu();
            }

            //F1
            if (IsControlJustReleased(1, 288))
            {
                if(!IsMenuOpen)
                {
                    IsMenuOpen = true;
                    PrintToChat($"Menu Open", SharedProperties.ColorGood);
                }
                else
                {
                    IsMenuOpen = false;
                    PrintToChat($"Menu Closed", SharedProperties.ColorGood);
                }
            }
        }
        private async Task PlayerSalaryTick()
        {
            if(PlayerProfile != null)
            {
                if(PlayerSalaryTimer > 0)
                {
                    PlayerSalaryTimer--;
                }
                else if(PlayerSalaryTimer == 0)
                {
                    //Give player money
                    PlayerProfile.Wallet += PlayerProfile.Salary;

                    //Save
                    TriggerServerEvent(SharedProperties.EventSaveProfile, PlayerProfile);

                    //Notify
                    PrintToChat($"You've received ${PlayerProfile.Salary} from your salary!", SharedProperties.ColorNormal);

                    //Reset
                    PlayerSalaryTimer = SharedProperties.DefaultSalaryTimer;
                }
                await Delay(1000);
            }
        }
        #endregion

        #region Commands
        private void Command_FixCar()
        {
            if(PlayerProfile.IsAdmin)
            {

            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }

        private void Command_DeleteVehicle()
        {
            var currentCar = Game.PlayerPed.CurrentVehicle;
            currentCar.Delete();
        }
        private async void Command_SpawnCopCar()
        {
            if (JobProfile.IsAdmin || JobProfile.IsPolice)
            {
                var hash = (uint)GetHashKey("police3"); //City pd car
                if(IsModelInCdimage(hash) && IsModelAVehicle(hash))
                {
                    var copCar = await World.CreateVehicle("police3", Game.PlayerPed.Position, Game.PlayerPed.Heading);

                    //Modify
                    copCar.CanTiresBurst = false;
                    copCar.Mods.WindowTint = VehicleWindowTint.PureBlack;
                    copCar.Mods.LicensePlate = $"POLICE";
                    copCar.Mods.InstallModKit();

                    //Spawn ped into car
                    Game.PlayerPed.SetIntoVehicle(copCar, VehicleSeat.Driver);

                    //Delete last car
                    var lastCar = Game.Player.LastVehicle;
                    lastCar.Delete();

                    //Notify
                    PrintToChat("Your cop car has spawned!", SharedProperties.ColorGood);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_Commit()
        {
            Player player = new Player(GetPlayerIndex());
            player.Character.Kill();
            PrintToChat("You've committed suicide", SharedProperties.ColorGood);
        }
        private void Command_ChangeJob(List<object> args)
        {
            if (args.Count == 1)
            {
                var jobID = Convert.ToInt32(args[0].ToString());
                if(IsJobValid(jobID))
                {
                    TriggerServerEvent(SharedProperties.EventChangeJob, jobID);
                }
                else
                {
                    PrintToChat("Invalid job ID.", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("Usage: /changejob <jobid>", SharedProperties.ColorWarning);
            }
        }
        private void Command_DepositWallet(List<object> args)
        {
            //Also check if they're near any banks
            if(IsNearAtm(new Player(GetPlayerIndex())))
            {
                if (args.Count == 1)
                {
                    var amount = Convert.ToInt64(args[0].ToString());
                    if (amount <= PlayerProfile.Wallet)
                    {
                        TriggerServerEvent(SharedProperties.EventDepositWallet, amount);
                    }
                    else
                    {
                        PrintToChat("You don't have that much in your wallet.", SharedProperties.ColorWarning);
                    }
                }
                else
                {
                    PrintToChat("Usage: /deposit <amount>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You must be near a bank to deposit.", SharedProperties.ColorWarning);
            }
        }
        private void Command_GetAreaName()
        {
            if (PlayerProfile.IsAdmin)
            {
                var player = new Player(GetPlayerIndex());
                var name = World.GetStreetName(player.Character.Position);
                PrintToChat($"Area Name: [{name}]", SharedProperties.ColorGood);
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_GetLocation()
        {
            if(PlayerProfile.IsAdmin)
            {
                var player = new Player(GetPlayerIndex());
                var pos = player.Character.Position;

                //Just print it out everywhere 
                PrintToChat($"Location: X: {pos.X} - Y: {pos.Y} - Z: {pos.Z}", SharedProperties.ColorGood);
                Console.WriteLine($"{pos.X} {pos.Y} {pos.Z}");
                Debug.WriteLine($"{pos.X} {pos.Y} {pos.Z}");
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_GetProfile()
        {
            if (PlayerProfile != null)
            {
                PrintToChat($"{PlayerProfile}", SharedProperties.ColorGood);
            }
            else
            {
                PrintToChat($"Profile was null.", SharedProperties.ColorError);
            }
        }
        //TODO: Add spawned cars into list
        private async void Command_SpawnCar(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1 || args.Count == 2)
                {
                    // Find model hash
                    var hash = (uint)GetHashKey(args[0].ToString());
                    if (IsModelInCdimage(hash) && IsModelAVehicle(hash))
                    {
                        //Spawn
                        var spawnedCar = await World.CreateVehicle(args[0].ToString(), Game.PlayerPed.Position + 2.0f, Game.PlayerPed.Heading);

                        if(args.Count == 2)
                        {
                            var val = Convert.ToInt32(args[1]);
                            if(val == 1)
                            {
                                //Godmode car
                                spawnedCar.CanTiresBurst = false;
                                spawnedCar.CanBeVisiblyDamaged = false;
                                spawnedCar.CanEngineDegrade = false;
                                spawnedCar.CanWheelsBreak = false;
                                spawnedCar.IsExplosionProof = true;
                                spawnedCar.IsFireProof = true;
                                spawnedCar.IsInvincible = true;
                            }
                        }

                        //Modify
                        spawnedCar.Mods.WindowTint = VehicleWindowTint.PureBlack;
                        spawnedCar.Mods.InstallModKit();

                        if(PowerMultiplier != 0)
                        {
                            spawnedCar.EnginePowerMultiplier = PowerMultiplier;
                        }

                        if(TorqueMultiplier != 0)
                        {
                            spawnedCar.EngineTorqueMultiplier = TorqueMultiplier;
                        }

                        //Notify
                        PrintToChat($"You've spawned a {spawnedCar.DisplayName}", SharedProperties.ColorGood);
                    }
                    else
                    {
                        PrintToChat($"Could find model: {args[0]}", SharedProperties.ColorWarning);
                    }
                }
                else
                {
                    PrintToChat("Usage: /spawncar 'model name' optional:<1=godmode>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        //TODO: Make model a fixed model and remove args
        private async void Command_AdminCar(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    // Find model hash
                    var hash = (uint)GetHashKey(args[0].ToString());
                    if (IsModelInCdimage(hash) && IsModelAVehicle(hash))
                    {
                        //Spawn
                        var personalCar = await World.CreateVehicle(args[0].ToString(), Game.PlayerPed.Position, Game.PlayerPed.Heading);

                        //Godmode car
                        personalCar.CanTiresBurst = false;
                        personalCar.CanBeVisiblyDamaged = false;
                        personalCar.CanEngineDegrade = false;
                        personalCar.CanWheelsBreak = false;
                        personalCar.IsExplosionProof = true;
                        personalCar.IsFireProof = true;
                        personalCar.IsInvincible = true;

                        //Modify
                        personalCar.Mods.WindowTint = VehicleWindowTint.PureBlack;
                        personalCar.Mods.LicensePlate = $"ADMINCAR";
                        personalCar.Mods.InstallModKit();

                        if (PowerMultiplier != 0)
                        {
                            personalCar.EnginePowerMultiplier = PowerMultiplier;
                        }

                        if (TorqueMultiplier != 0)
                        {
                            personalCar.EngineTorqueMultiplier = TorqueMultiplier;
                        }

                        //Spawn ped into car
                        Game.PlayerPed.SetIntoVehicle(personalCar, VehicleSeat.Driver);

                        //Delete last car
                        var lastCar = Game.Player.LastVehicle;
                        lastCar.Delete();

                        //Notify
                        PrintToChat("Your new admin car has spawned!", SharedProperties.ColorGood);
                    }
                    else
                    {
                        PrintToChat($"Could find model: {args[0]}", SharedProperties.ColorWarning);
                    }
                }
                else
                {
                    PrintToChat("Usage: /admincar 'model name'", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_LockCar(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var value = Convert.ToInt32(args[0]);
                    var player = new Player(GetPlayerIndex());
                    var currentCar = player.Character.CurrentVehicle;
                    if(currentCar != null)
                    {
                        switch(value)
                        {
                            case 0:
                                currentCar.LockStatus = VehicleLockStatus.Unlocked;
                                PrintToChat("You've unlocked your car.", SharedProperties.ColorGood);
                                break;
                            case 1:
                                currentCar.LockStatus = VehicleLockStatus.Locked;
                                PrintToChat("You've locked your car.", SharedProperties.ColorGood);
                                break;
                        }
                    }
                    else
                    {
                        PrintToChat("You need to be in a car.", SharedProperties.ColorWarning);
                    }
                }
                else
                {
                    PrintToChat("Usage: /lockcar <1 = on | 0 = off>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_SetTorque(List<object> args)
        {
            if(PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    //Set values
                    var value = Convert.ToInt32(args[0]);
                    TorqueMultiplier = value;

                    Player player = new Player(GetPlayerIndex());
                    var currentCar = player.Character.CurrentVehicle;
                    currentCar.EngineTorqueMultiplier = (float)value;

                    PrintToChat($"Car torque multiplier set to: {value}", SharedProperties.ColorGood);
                }
                else
                {
                    PrintToChat("Usage: /torque <multiplier value>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_SetPower(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if(args.Count == 1)
                {
                    var value = Convert.ToInt32(args[0]);
                    PowerMultiplier = value;

                    Player player = new Player(GetPlayerIndex());
                    var currentCar = player.Character.CurrentVehicle;
                    currentCar.EnginePowerMultiplier = (float)value;

                    PrintToChat($"Car power multiplier set to: {value}", SharedProperties.ColorGood);
                }
                else
                {
                    PrintToChat("Usage: /power <multiplier value>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_Wanted(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    bool val = args[0].Equals("1") ? true : false;
                    var player = new Player(GetPlayerIndex());
                    if (val)
                    {
                        CanBeWanted = true;
                        player.Character.CanBeTargetted = true;
                        PrintToChat("You can now be wanted.", SharedProperties.ColorGood);
                    }
                    else
                    {
                        CanBeWanted = false;
                        player.Character.CanBeTargetted = false;
                        PrintToChat("Wanted turned off", SharedProperties.ColorGood);
                    }
                }
                else
                {
                    PrintToChat("Usage: /wanted <1 = on |0 = off>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_GetAllWeapons()
        {
            if (PlayerProfile.IsAdmin)
            {
                foreach(var item in SharedProperties.AllWeapons)
                {
                    var hash = GetHashKey(item);
                    GiveWeaponToPed(PlayerPedId(), (uint)hash, 1000, false, true);
                    SetPedInfiniteAmmo(PlayerPedId(), true, (uint)hash);
                    SetPedInfiniteAmmoClip(PlayerPedId(), true);
                }
                PrintToChat("You now have all weapons.", SharedProperties.ColorGood);
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_Godmode(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    bool val = args[0].Equals("1") ? true : false;
                    var player = new Player(GetPlayerIndex());
                    if (val)
                    {
                        player.IsInvincible = true;
                        player.Character.IsInvincible = true;
                        player.Character.Health = 200;
                        PrintToChat("Godmode turned on.", SharedProperties.ColorGood);
                    }
                    else
                    {
                        player.IsInvincible = false;
                        player.Character.IsInvincible = false;
                        player.Character.Health = 200;
                        PrintToChat("Godmode turned off.", SharedProperties.ColorGood);
                    }
                }
                else
                {
                    PrintToChat("Usage: /god <1|0>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_GiveAdmin(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var netID = Convert.ToInt32(args[0]);
                    TriggerServerEvent(SharedProperties.EventGiveAdmin, netID);
                }
                else
                {
                    PrintToChat("Usage: /giveadmin <netID>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_RemoveAdmin(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var netID = Convert.ToInt32(args[0]);
                    TriggerServerEvent(SharedProperties.EventRemoveAdmin, netID);
                }
                else
                {
                    PrintToChat("Usage: /removeadmin <netID>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_SetTime(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var time = Convert.ToInt32(args[0]);
                    if(time < 24 && time > 1)
                    {
                        NetworkOverrideClockTime(time, 0, 0);
                        PrintToChat($"Time set to hour: {time}", SharedProperties.ColorGood);
                    }
                    else
                    {
                        PrintToChat("Time must be between 1 and 24", SharedProperties.ColorWarning);
                    }
                }
                else
                {
                    PrintToChat("Usage: /settime <time 1-24>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_AddBan(List<object> args)
        {
            if(PlayerProfile.IsAdmin)
            {
                if (args.Count == 3)
                {
                    var licenseID = args[0];
                    var reason = args[1];
                    var hours = Convert.ToInt32(args[2]);

                    TriggerServerEvent(SharedProperties.EventAddBan, new[] { licenseID, reason, hours });
                }
                else
                {
                    PrintToChat("Usage: /addban 'license id' 'reason' <hours>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_ChangeWeather(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var val = Convert.ToInt32(args[0]);
                    //TODO: Add more weather types
                    switch(val)
                    {
                        case 1:
                            World.Weather = Weather.ExtraSunny;
                            World.TransitionToWeather(Weather.ExtraSunny, 5.0f);
                            PrintToChat("Weather change to extra sunny.", SharedProperties.ColorGood);
                            break;
                        case 2:
                            World.Weather = Weather.Christmas;
                            World.TransitionToWeather(Weather.Christmas, 5.0f);
                            PrintToChat("Weather change to christmas", SharedProperties.ColorGood);
                            break;
                        default:
                            PrintToChat("Could not find weather type.", SharedProperties.ColorWarning);
                            break;
                    }
                }
                else
                {
                    PrintToChat("Usage: /weather <1=exsunny|2=christmas>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_FreezePlayer(List<object> args)
        {
            if(PlayerProfile.IsAdmin)
            {
                if(args.Count == 2)
                {
                    var netID = Convert.ToInt32(args[0]);
                    var freezePlayer = Convert.ToInt32(args[1]);

                    TriggerServerEvent(SharedProperties.EventFreezePlayer, new[] { netID, freezePlayer });
                }
                else
                {
                    PrintToChat("Usage: /freeze <netID> <1|0>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_KickPlayer(List<object> args)
        {
            if (PlayerProfile.IsAdmin)
            {
                if (args.Count == 2)
                {
                    var netID = Convert.ToInt32(args[0]);
                    var reason = args[1];

                    TriggerServerEvent(SharedProperties.EventKickPlayer, new[] { netID, reason });
                }
                else
                {
                    PrintToChat("Usage: /kick <netID> 'reason'", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_BanPlayer(List<object> args)
        {
            if(PlayerProfile.IsAdmin)
            {
                if (args.Count == 3)
                {
                    var netID = Convert.ToInt32(args[0]);
                    var reason = args[1];
                    var hours = Convert.ToInt32(args[2]);

                    TriggerServerEvent(SharedProperties.EventBanPlayer, new[] { netID, reason, hours });
                }
                else
                {
                    PrintToChat("Usage: /ban <netid> 'reason' <hours>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_UnBanPlayer(List<object> args)
        {
            if(PlayerProfile.IsAdmin)
            {
                if (args.Count == 1)
                {
                    var dbID = Convert.ToInt32(args[0]);
                    TriggerServerEvent(SharedProperties.EventUnbanPlayer, new[] { dbID });
                }
                else
                {
                    PrintToChat("Usage: /unban <dbID>", SharedProperties.ColorWarning);
                }
            }
            else
            {
                PrintToChat("You do not have permission to this.", SharedProperties.ColorError);
            }
        }
        private void Command_InvokeOwner()
        {
            TriggerServerEvent(SharedProperties.EventInvokeOwnership);
        }
        #endregion

        #region Private Methods
        private void GivePlayerJobWeapons()
        {
            //Give player weapons from job
            if (JobProfile != null)
            {
                foreach (var item in JobProfile.SpawnWeapons)
                {
                    var hash = GetHashKey(item);
                    GiveWeaponToPed(PlayerPedId(), (uint)hash, 1000, false, true);

                    if (JobProfile.IsAdmin)
                    {
                        SetPedInfiniteAmmo(PlayerPedId(), true, (uint)hash);
                        SetPedInfiniteAmmoClip(PlayerPedId(), true);
                    }
                }
            }
        }
        private async void ChangePlayerModel(Player player)
        {
            var rnd = new Random().Next(0, JobProfile.Models.Length);
            var model = (uint)GetHashKey(JobProfile.Models[rnd]);
            while(!HasModelLoaded(model))
            {
                RequestModel(model);
                await Delay(200);
            }

            if(HasModelLoaded(model))
            {
                SetPlayerModel(player.Handle, model);
            }
        }
        private bool IsJobValid(int jobID)
        {
            foreach(var item in SharedProperties.AllJobs)
            {
                if(item.JobID == jobID)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsNearAtm(Player player)
        {
            var playerLoc = player.Character.Position;
            foreach(var item in SharedProperties.Location_ATM)
            {
                //Convert and get distance
                Vector3 itemVec = new Vector3(item);
                Vector3.Distance(ref itemVec, ref playerLoc, out float distance);

                if(distance <= 1.5f)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsNearPoliceStation (Player player)
        {
            var playerLoc = player.Character.Position;
            foreach (var item in SharedProperties.Location_Police)
            {
                //Convert and get distance
                Vector3 itemVec = new Vector3(item);
                Vector3.Distance(ref itemVec, ref playerLoc, out float distance);

                if (distance <= 1.0f)
                {
                    return true;
                }
            }
            return false;
        }
        private bool GetStringBool(string arg)
        {
            return arg.ToString()
                .Trim()
                .ToLower()
                .Equals("true") ? true : false;
        }
        private void PrintToChat(string printArgs, int[] chatColor)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = chatColor,
                args = new[] { SharedProperties.ChatPrefix, $"{printArgs}" }
            });
        }
        private void ShowNotification(string message)
        {
            SetTextScale(0.35f, 0.50f);
            SetTextFont(4);
            SetTextProportional(true);
            SetTextColour(0, 255, 0, 255);
            SetTextEntry("String");
            AddTextComponentString(message);
            SetTextCentre(true);
            DrawText(0.45f, 0.5f);
        }
        private void DisplayHud()
        {
            SetTextScale(0.35f, 0.50f);
            SetTextFont(4);
            SetTextProportional(true);
            SetTextColour(248, 183, 29, 255);
            SetTextEntry("String");
            SetTextDropshadow(5, 0, 0, 0, 0);
            AddTextComponentString($"Wallet: ${PlayerProfile.Wallet}\nBank: ${PlayerProfile.Bank}\nJob: {JobProfile.JobName}\nSalary: ${PlayerProfile.Salary}\nNext Paycheck: {PlayerSalaryTimer} seconds");
            DrawText(0.18f, 0.80f);
        }
        private void DisplayMPH()
        {
            SetTextScale(0.35f, 0.70f);
            SetTextFont(4);
            SetTextProportional(true);
            SetTextColour(0, 255, 0, 255);
            SetTextEntry("String");
            SetTextDropshadow(5, 0, 0, 0, 0);
            AddTextComponentString($"{Convert.ToInt32(Game.PlayerPed.CurrentVehicle.Speed * 2)} MPH");
            DrawText(0.80f, 0.90f);
        }
        private void SpawnPlayer(Player player, float[] spawnLoc)
        {
            StartPlayerTeleport(player.Handle, spawnLoc[0], spawnLoc[1], spawnLoc[2], 0.5f, false, true, false);
        }
        #endregion
    }
}
