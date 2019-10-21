using GTARPG.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using GTARPGClient.Models;
using static CitizenFX.Core.Native.API;

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

        private Admin()
        {

        }

        public void Command_ClearPolice(int source)
        {
            var player = new Player(source);
            player.WantedLevel = 0;
            Utils.Instance.PrintToChat("Wanted level cleared.", ChatColor.Green);
        }

        public void Command_GetPlayers()
        {
            var counter = 1;
            Utils.Instance.PrintToChat($"=====PlayerList=====", ChatColor.Red);
            foreach (Player item in Players)
            {
                Utils.Instance.PrintToChat($"{counter}.{item.Name}(NID:{item.Character.NetworkId}) SID:{item.ServerId}", ChatColor.Green);
                counter++;
            }
        }

        public void Command_NoAnnoy(int source)
        {
            var player = new Player(source);
            player.Character.CanBeKnockedOffBike = false;
            player.Character.CanBeDraggedOutOfVehicle = false;
            player.Character.IsInvincible = true;
            player.Character.Weapons.Give(WeaponHash.AdvancedRifle, 1000, true, true);
            Utils.Instance.PrintToChat("Anti-annoy now on.", ChatColor.Green);
        }

        public void Command_SetSunnyAndDay()
        {
            NetworkOverrideClockTime(10, 00, 00);

            World.Weather = Weather.ExtraSunny;
            World.TransitionToWeather(Weather.ExtraSunny, 10f);
            Utils.Instance.PrintToChat($"Weather set to sunny and time set to 10AM", ChatColor.Green);
        }
    }
}
