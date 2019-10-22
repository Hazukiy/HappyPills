using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using GTARPG.Tools;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using GTARPGClient.Models;

namespace GTARPG.Logic
{
    public class Admin : BaseScript
    {
        private static Admin _instance;
        public static Admin Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Admin();
                }
                return _instance;
            }
        }

        public bool IsAdmin { get; set; }

        private Admin() { }

        public void Command_InvokeOwner(int source)
        {
            try
            {
                var player = new Player(source);
                TriggerServerEvent("addOwner", player);
                Utils.Instance.PrintToChat($"Admin command invoked", ChatColor.Green);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_IsAdmin()
        {
            Utils.Instance.PrintToChat($"Is Admin: {IsAdmin}", ChatColor.Green);
        }

        public void Command_AddAdmin(int source)
        {
            try
            {
                if (!IsAdmin) return;

                TriggerServerEvent("addAdmin", new Player(source).Character.NetworkId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_RemoveAdmin(int source)
        {
            try
            {
                if (!IsAdmin) return;

                TriggerServerEvent("removeAdmin", new Player(source).Character.NetworkId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_ClearPolice(int source)
        {
            try
            {
                if (!IsAdmin) return;

                new Player(source).WantedLevel = 0;
                Utils.Instance.PrintToChat("Wanted level cleared.", ChatColor.Green);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_GetPlayers()
        {
            try
            {
                if (!IsAdmin) return;

                var counter = 1;
                Utils.Instance.PrintToChat($"=====PlayerList=====", ChatColor.Red);
                foreach (Player item in Players)
                {
                    Utils.Instance.PrintToChat($"{counter}.{item.Name}(NID:{item.Character.NetworkId}) SID:{item.ServerId}", ChatColor.Green);
                    counter++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_God(int source, List<object> args)
        {
            try
            {
                if (!IsAdmin) return;

                var arguments = args.OfType<string>();
                if (arguments.Count() == 1 && (arguments.Contains("on") || arguments.Contains("off")))
                {
                    bool isEnabled = false;
                    if(arguments.FirstOrDefault().ToLower().Equals("on"))
                    {
                        isEnabled = true;
                    }

                    var player = new Player(source);
                    player.Character.CanBeKnockedOffBike = !isEnabled;
                    player.Character.CanBeDraggedOutOfVehicle = !isEnabled;
                    player.Character.IsInvincible = isEnabled;
                    Utils.Instance.PrintToChat($"Godmode set to: {isEnabled}", ChatColor.Green);
                }
                else
                {
                    Utils.Instance.PrintToChat("Command Usage: /god (on|off)", ChatColor.Red);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_SetSunnyAndDay(List<object> args)
        {
            try
            {
                if (!IsAdmin) return;

                var arguments = args.OfType<string>();
                if (arguments.Count() == 1)
                {
                    var time = Convert.ToInt32(arguments.FirstOrDefault());
                    if (time > 0 && time < 24)
                    {
                        var correctTime = string.Empty;
                        if (time > 12)
                        {
                            correctTime = "PM";
                        }
                        else
                        {
                            correctTime = "AM";
                        }

                        NetworkOverrideClockTime(time, 00, 00);
                        World.TransitionToWeather(Weather.ExtraSunny, 10f);
                        Utils.Instance.PrintToChat($"Weather set to sunny and time set to {time}{correctTime}", ChatColor.Green);
                    }
                    else
                    {
                        Utils.Instance.PrintToChat($"Command Usage: /sunnyday <time (1-24)>", ChatColor.Red);
                    }
                }
                else
                {
                    Utils.Instance.PrintToChat($"Command Usage: /sunnyday <time (1-24)>", ChatColor.Red);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_GetPosition(int source)
        {
            try
            {
                if (!IsAdmin) return;

                var player = new Player(source).Character;
                Utils.Instance.PrintToChat($"Current Position: X {player.Position.X} Y {player.Position.Y} Z {player.Position.Z}", ChatColor.Green);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_ShowID(int source)
        {
            try
            {
                if (!IsAdmin) return;

                var player = new Player(source);
                Utils.Instance.PrintToChat($"ServerID: {player.ServerId}", ChatColor.Red);
                Utils.Instance.PrintToChat($"Handle: {player.Handle}", ChatColor.Red);
                Utils.Instance.PrintToChat($"NetworkID: {player.Character.NetworkId}", ChatColor.Red);
                Utils.Instance.PrintToChat($"NativeVal: {player.Character.NativeValue}", ChatColor.Red);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }
    }
}
