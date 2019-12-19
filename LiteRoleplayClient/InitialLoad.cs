using System;
using System.Collections.Generic;
using System.Drawing;
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

        //Local timer
        private int PlayerSalaryTimer { get; set; }

        //Menu boolean
        private bool IsMenuOpen { get; set; }

        //Menu index
        private int MenuIndex { get; set; }

        public InitialLoad()
        {
            //Built-in events
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);

            //Custom events
            EventHandlers[SharedProperties.ProfileCallback] += new Action<ProfileModel>(OnPlayerProfileCallback);

            //Set salary timer
            PlayerSalaryTimer = SharedProperties.DefaultSalaryTimer;

            //Start salary timer
            Tick += PlayerSalaryTick;

            //Set menu index
            MenuIndex = 1;

            //Start menu tick
            Tick += MenuTick;

            //Render in HUD
            Tick += HudTick;

            //Load profile
            TriggerServerEvent(SharedProperties.EventLoadProfile);

            //Notify
            Console.WriteLine($"[LiteRoleplay v{Assembly.GetExecutingAssembly().GetName().Version}][CLIENT] - Loaded successfully.");
        }

        #region Events
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            //Player commands
            RegisterCommand("profile", new Action<int, List<object>, string>((source, args, raw) => { Command_GetProfile(); }), false);

            //Admin commands
            RegisterCommand("ban", new Action<int, List<object>, string>((source, args, raw) => { Command_BanPlayer(args); }), false);
            RegisterCommand("unban", new Action<int, List<object>, string>((source, args, raw) => { Command_UnBanPlayer(args); }), false);
            RegisterCommand("invokeowner", new Action<int, List<object>, string>((source, args, raw) => { Command_InvokeOwner(); }), false);
            RegisterCommand("admincar", new Action<int, List<object>, string>((source, args, raw) => { Command_AdminCar(args); }), false);
            RegisterCommand("freeze", new Action<int, List<object>, string>((source, args, raw) => { Command_FreezePlayer(args); }), false);
        }

        private void OnPlayerSpawned()
        {
            var player = new Player(GetPlayerIndex());
            SpawnPlayer(player, SharedProperties.DefaultSpawn);
        }
        #endregion

        #region Custom Events
        public void OnPlayerProfileCallback(dynamic profile)
        {
            if (profile != null && PlayerProfile == null)
            {
                PlayerProfile = SharedProperties.ConvertToProfile(profile);
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
            SetTextFont(1);
            SetTextColour(128, 128, 128, 255);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(1, 0, 0, 0, 150);
            SetTextOutline();
            SetTextEntry("String");
            AddTextComponentString("This works");
            DrawText(1.0f, 1.0f);
            await Delay(1000);
        }

        private async Task MenuTick()
        {
            //Draw Menu
            if(IsMenuOpen)
            {
                //Menu control functions


                DrawMenu();
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
                    PrintToChat($"You've received ${PlayerProfile.Salary} from your salary!", SharedProperties.ColorGood);

                    //Reset
                    PlayerSalaryTimer = SharedProperties.DefaultSalaryTimer;
                }
                await Delay(1000);
            }
        }
        #endregion

        #region Menu Methods
        private void DrawMenu()
        {
            //Main container
            DrawRect(0.5f, 0.5f, 0.500f, 0.500f, 21, 17, 29, 255);

            //Inner frame
            DrawRect(0.5f, 0.5f, 0.200f, 0.200f, 255, 0, 0, 255);
        }
        #endregion

        #region Non-Admin Commands
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
        #endregion

        #region Spawning Commands
        private async void Command_AdminCar(List<object> args)
        {
            if (args.Count == 1)
            {
                // Find model hash
                var hash = (uint)GetHashKey(args[0].ToString());

                //TODO: Keep an eye on this condition, does it have to be both cases? Unsure.
                if(IsModelInCdimage(hash) && IsModelAVehicle(hash))
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
                    personalCar.Mods.CustomPrimaryColor = Color.FromArgb(47, 47, 47, 46);
                    personalCar.Mods.CustomSecondaryColor = Color.FromArgb(255, 201, 62, 62);
                    personalCar.Mods.WindowTint = VehicleWindowTint.PureBlack;
                    personalCar.Mods.LicensePlate = $"ADMINCAR";
                    personalCar.Mods.InstallModKit();

                    //Speed adjustment
                    personalCar.EnginePowerMultiplier = 30.0f;
                    personalCar.EngineTorqueMultiplier = 30.0f;

                    //Spawn ped into car
                    Game.PlayerPed.SetIntoVehicle(personalCar, VehicleSeat.Driver);

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
        #endregion

        #region Management Commands 
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
                    PrintToChat("Usage: /freeze <netID> <1|0>", SharedProperties.ColorWarning);
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
        private void PrintToChat(string printArgs, int[] chatColor)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = chatColor,
                args = new[] { SharedProperties.ChatPrefix, $"{printArgs}" }
            });
        }

        private void SpawnPlayer(Player player, float[] spawnLoc)
        {
            StartPlayerTeleport(player.Handle, spawnLoc[0], spawnLoc[1], spawnLoc[2], spawnLoc[3], false, true, false);
        }
        #endregion
    }
}
