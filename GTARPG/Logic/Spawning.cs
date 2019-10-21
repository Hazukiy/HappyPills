using GTARPG.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using GTARPGClient.Models;
using System.Reflection;

namespace GTARPG.Logic
{
    public class Spawning : BaseScript
    {
        private static Spawning _instance;
        private List<CatProfile> catProfiles;

        public static Spawning Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Spawning();
                }
                return _instance;
            }
        }

        public List<CatProfile> CatProfiles
        {
            get
            {
                return catProfiles;
            }
        }

        private Spawning()
        {
            catProfiles = new List<CatProfile>();
        }

        public void Command_GetCats(int source)
        {
            var player = new Player(source);
            var itemNo = 1;
            foreach (var item in catProfiles.Where(x => x.Owner == player))
            {
                Utils.Instance.PrintToChat($"{itemNo}.{item.Name} - {item.Age} years old. Has {item.Hunger} hunger left.", ChatColor.Blue);
                itemNo++;
            }
        }

        public void Command_SpawnCar(int source)
        {
            var car = new Model(VehicleHash.T20);
            var player = new Player(source);
            var result = World.CreateVehicle(car, player.Character.Position + 5);
            if (result.IsCompleted)
            {
                var ownedCar = result.Result;
                ownedCar.IsInvincible = true;

                Utils.Instance.PrintToChat($"Your new T20 has spawned!", ChatColor.Green);
            }

            if (result.IsFaulted || result.IsCanceled)
            {
                Utils.Instance.PrintToChat("There was an issue spawning a car.", ChatColor.Red);
            }
        }

        public void Command_SpawnCat(int source, List<object> args)
        {
            try
            {
                if (args.Count != 1)
                {
                    Utils.Instance.PrintToChat($"Command usage: /cat <catname>", ChatColor.Red);
                    return;
                }

                var mdl = new Model(PedHash.Cat);
                var player = new Player(source);
                var playerPos = player.Character.Position;
                playerPos[0] = playerPos[0] += 4.0f;

                var result = World.CreatePed(mdl, playerPos);
                if (result.IsCompleted)
                {
                    var cat = result.Result;
                    cat.CanBeTargetted = false;
                    cat.IsInvincible = true;

                    var newCat = new CatProfile()
                    {
                        Owner = player,
                        Ped = cat,
                        Name = args[0].ToString(),
                        Age = 1,
                        Hunger = 100,
                        IsFollow = true

                    };
                    catProfiles.Add(newCat);
                    Utils.Instance.PrintToChat($"{newCat.Name} has spawned!", ChatColor.Green);
                }

                if (result.IsFaulted || result.IsCanceled)
                {
                    Utils.Instance.PrintToChat("There was an issue spawning.", ChatColor.Red);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }
    }
}
