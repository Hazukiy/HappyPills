using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using LiteDB;
using LiteRoleplayServer.Models.Player;

namespace LiteRoleplayServer.components.Clients
{
    public class InfoActions : BaseScript
    {
        #region Singleton & Constructor
        private static InfoActions _instance;
        public static InfoActions Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new InfoActions();
                }
                return _instance;
            }
        }
        private InfoActions() { }
        #endregion

        public void InitialInfoLoad([FromSource]Player player)
        {
            var playerLicense = player.Identifiers["license"];
            if (DoesInfoExist(player))
            {
                //Update values
                using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                {
                    var col = db.GetCollection<InfoModel>(SharedProperties.DatabaseTableInfo);
                    var playerInfo = col.FindOne(x => x.LicenseID.Equals(playerLicense));

                    //Update values
                    var hasNewName = playerInfo.AllNames.Any(x => !x.Equals(player.Name));
                    if(hasNewName)
                    {
                        playerInfo.AllNames[playerInfo.AllNames.Length + 1] = player.Name;
                    }
                    playerInfo.IP = player.EndPoint;
                    playerInfo.LastLoginDate = DateTime.UtcNow;
                    playerInfo.Name = player.Name;

                    //Update
                    col.Update(playerInfo);

                    //Notify
                    Console.WriteLine($"Player info profile updated({playerInfo.ToString()})");
                }
            }
            else
            {
                InsertNewInfo(player, playerLicense);
            }
        }

        #region Private Methods
        /// <summary>
        /// Returns true if player already has an info profile
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool DoesInfoExist(Player player)
        {
            var retVal = false;
            var playerLicense = player.Identifiers["license"];
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<InfoModel>(SharedProperties.DatabaseTableInfo);
                retVal = col.Exists(x => x.LicenseID.Equals(playerLicense));
            }
            return retVal;
        }

        /// <summary>
        /// Inserts a new info profile
        /// </summary>
        private void InsertNewInfo(Player player, string id)
        {
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<InfoModel>(SharedProperties.DatabaseTableInfo);

                //Construct model
                InfoModel newInfo = new InfoModel()
                {
                    LicenseID = id, //Always use latest ID.
                    Name = player.Name,
                    AllNames = new[] { player.Name },
                    FirstLoginDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow,
                    IP = player.EndPoint,
                    IsAdmin = false,
                    IsBanned = false
                };

                //Insert new profile
                col.Insert(newInfo);

                //Notify
                Console.WriteLine($"New info profile inserted({newInfo.ToString()})");
            }
        }
        #endregion

    }
}
