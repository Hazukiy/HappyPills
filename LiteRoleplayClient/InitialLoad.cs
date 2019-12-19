using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using LiteRoleplayClient.components.Chat;
using LiteRoleplay.Shared.Models;
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

        //Car boost
        private float CarPower { get; set; }
        private float CarTorque { get; set; }

        private List<Vehicle> PersonalCars { get; set; }

        public InitialLoad()
        {
            //Built-in events
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);

            //Custom events
            EventHandlers[SharedProperties.ProfileCallback] += new Action<ProfileModel>(OnPlayerProfileCallback);

            //Set salary timer
            PlayerSalaryTimer = SharedProperties.SalaryTimer;

            //Start salary timer
            Tick += PlayerSalaryTick;

            //Set menu index
            MenuIndex = 1;

            //Start menu tick
            Tick += MenuTick;

            //Load profile
            TriggerServerEvent(SharedProperties.EventLoadProfile);

            //Load person cars as empty
            PersonalCars = new List<Vehicle>();
            CarPower = 10.0f;
            CarTorque = 10.0f;

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
            RegisterCommand("players", new Action<int, List<object>, string>((source, args, raw) => { Command_GetOnline(); }), false);
            RegisterCommand("admincar", new Action<int, List<object>, string>((source, args, raw) => { Command_AdminCar(args); }), false);
        }

        private void OnPlayerSpawned()
        {
            StartPlayerTeleport(new Player(GetPlayerIndex()).Handle, -1329.704f, -1512.436f, 4.379375f, 1.0f, false, true, false);
        }
        #endregion

        #region Custom Events
        public void OnPlayerProfileCallback(dynamic profile)
        {
            if (profile != null && PlayerProfile == null)
            {
                PlayerProfile = ConvertToProfile(profile);
                Debug.WriteLine("Profile loaded from callback.");
                ChatUtils.Instance.PrintToChat($"Profile Loaded: {PlayerProfile}", SharedProperties.ColorGood);
            }
            else
            {
                //Do some trying logic here...
                Debug.WriteLine("Critial error - failed to load profile.");
                ChatUtils.Instance.PrintToChat("Failed to load profile", SharedProperties.ColorError);
            }
        }
        #endregion

        #region Timers
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
                    ChatUtils.Instance.PrintToChat($"Menu Open", SharedProperties.ColorGood);
                }
                else
                {
                    IsMenuOpen = false;
                    ChatUtils.Instance.PrintToChat($"Menu Closed", SharedProperties.ColorGood);
                }
            }

            //KEYPAD UP (car Power up)
            if (IsControlPressed(1, 127))
            {
                IncreaseCarPower();
            }

            //KEYPAD DOWN (car Power down)
            if (IsControlPressed(1, 128))
            {
                DecreaseCarPower();
            }

            //ARROW RIGHT (car torque up)
            if (IsControlPressed(1, 123))
            {
                IncreaseCarTorque();
            }

            //ARROW LEFT (car torque down)
            if (IsControlPressed(1, 124))
            {
                DecreaseCarTorque();
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
                    ChatUtils.Instance.PrintToChat($"You've received ${PlayerProfile.Salary} from your salary!", SharedProperties.ColorGood);

                    //Reset
                    PlayerSalaryTimer = SharedProperties.SalaryTimer;
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
        private void Command_GetOnline()
        {
            foreach (var player in Players)
            {
                ChatUtils.Instance.PrintToChat($"Player {player.Name} (Handle: {player.Handle}, ServerID: {player.ServerId}, NetID: {player.Character.NetworkId})", SharedProperties.ColorNormal);
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

                    //Speed
                    //personalCar.EnginePowerMultiplier = CarPower;
                    //personalCar.EngineTorqueMultiplier = CarTorque;

                    //Spawn ped into car
                    Game.PlayerPed.SetIntoVehicle(personalCar, VehicleSeat.Driver);

                    //Add personal car
                    PersonalCars.Add(personalCar);

                    //Notify
                    ChatUtils.Instance.PrintToChat("Your new admin car has spawned!", SharedProperties.ColorGood);
                }
                else
                {
                    ChatUtils.Instance.PrintToChat($"Could find model: {args[0]}", SharedProperties.ColorWarning);
                }
            }
            else
            {
                ChatUtils.Instance.PrintToChat("Usage: /admincar 'model name'", SharedProperties.ColorWarning);
            }
        }
        #endregion

        #region Management Commands 
        private void Command_BanPlayer(List<object> args)
        {
            if(args.Count == 3)
            {
                var netID = Convert.ToInt32(args[0]);
                var reason = args[1];
                var hours = Convert.ToInt32(args[2]);

                TriggerServerEvent(SharedProperties.EventBanPlayer, new[] { netID, reason, hours });
            }
            else
            {
                ChatUtils.Instance.PrintToChat("Usage: /ban <netid> 'reason' <hours>", SharedProperties.ColorWarning);
            }
        }

        private void Command_UnBanPlayer(List<object> args)
        {
            if (args.Count == 1)
            {
                var dbID = Convert.ToInt32(args[0]);
                TriggerServerEvent(SharedProperties.EventUnbanPlayer, new[] { dbID });
            }
            else
            {
                ChatUtils.Instance.PrintToChat("Usage: /unban <dbID>", SharedProperties.ColorWarning);
            }
        }

        private void Command_InvokeOwner()
        {
            TriggerServerEvent(SharedProperties.EventInvokeOwnership);
        }

        private void Command_GetProfile()
        {
            if(PlayerProfile != null)
            {
                ChatUtils.Instance.PrintToChat($"{PlayerProfile.ToString()}", SharedProperties.ColorGood);
            }
            else
            {
                ChatUtils.Instance.PrintToChat($"Profile was null.", SharedProperties.ColorError);
            }
        }
        #endregion

        #region Private Methods
        private void IncreaseCarPower()
        {
            var entityIndex = GetVehicleIndexFromEntityIndex(GetPlayerIndex());
            var currentCar = new Vehicle(entityIndex);

            CarPower += 500.0f;
            currentCar.EnginePowerMultiplier = CarPower;

            ChatUtils.Instance.PrintToChat($"Car Power Increased: {CarPower}", SharedProperties.ColorGood);
        }

        private void DecreaseCarPower()
        {
            var entityIndex = GetVehicleIndexFromEntityIndex(GetPlayerIndex());
            var currentCar = new Vehicle(entityIndex);

            ChatUtils.Instance.PrintToChat($"Driver: {currentCar.Driver} - {currentCar.DisplayName} - {currentCar.Model}", SharedProperties.ColorError);

            var preCalc = CarPower - 500.0f;

            if(preCalc > 0 && CarPower > 0)
            {
                CarPower -= 500.0f;
                currentCar.EnginePowerMultiplier = CarPower;

                ChatUtils.Instance.PrintToChat($"Car Power Decreased: {CarPower}", SharedProperties.ColorError);
            }
        }

        private void IncreaseCarTorque()
        {
            var entityIndex = GetVehicleIndexFromEntityIndex(GetPlayerIndex());
            var currentCar = new Vehicle(entityIndex);

            CarTorque += 500.0f;
            currentCar.EngineTorqueMultiplier = CarTorque;

            ChatUtils.Instance.PrintToChat($"Car Torque Increased: {CarTorque}", SharedProperties.ColorGood);
        }

        private void DecreaseCarTorque()
        {
            var entityIndex = GetVehicleIndexFromEntityIndex(GetPlayerIndex());
            var currentCar = new Vehicle(entityIndex);
            var preCalc = CarTorque - 500.0f;

            if (preCalc > 0 && CarTorque > 0)
            {
                CarTorque -= 500.0f;
                currentCar.EngineTorqueMultiplier = CarTorque;

                ChatUtils.Instance.PrintToChat($"Car Torque Decreased: {CarTorque}", SharedProperties.ColorError);
            }
        }

        private ProfileModel ConvertToProfile(dynamic obj)
        {
            return new ProfileModel()
            {
                Id = obj.Id,
                LicenseID = obj.LicenseID,
                Wallet = obj.Wallet,
                Bank = obj.Bank,
                Salary = obj.Salary,
                Job = obj.Job,
                IsWanted = obj.IsWanted
            };
        }
        #endregion
    }
}
