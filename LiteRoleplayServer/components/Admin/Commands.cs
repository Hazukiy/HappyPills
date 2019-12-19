using System;
using CitizenFX.Core;
using LiteDB;
using LiteRoleplayServer.components.Utils;
using LiteRoleplayServer.Models.Admin;
using LiteRoleplay.Shared.Models;
using static CitizenFX.Core.Native.API;

namespace LiteRoleplayServer.components.Admin
{
    public class Commands : BaseScript
    {
        #region Singleton & Constructor
        private static Commands _instance;
        public static Commands Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Commands();
                }
                return _instance;
            }
        }
        private Commands() { }
        #endregion

        #region Checks
        /// <summary>
        /// Checks if a player has admin
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckPerms(Player player)
        {
            var retVal = false;
            var playerLicense = player.Identifiers["license"];
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<AdminModel>(SharedProperties.DatabaseTableAdmins);
                var adminProfile = col.FindOne(x => x.LicenseID.Equals(playerLicense));
                if (adminProfile != null)
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns null is not banned, true otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BanModel IsPlayerBanned(string id)
        {
            BanModel retVal = null;
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<BanModel>(SharedProperties.DatabaseTableBanned);
                var doesExist = col.Exists(x => x.LicenseID.Equals(id));
                if(doesExist)
                {
                    retVal = col.FindOne(x => x.LicenseID.Equals(id));
                }
            }
            return retVal;
        }
        #endregion

        #region Banning/Kicking
        /// <summary>
        /// Bans a player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="targetIndex"></param>
        /// <param name="reason"></param>
        /// <param name="hours"></param>
        public void BanPlayer([FromSource]Player player, int netID, string reason, int hours)
        {
            var target = Players[netID];
            if(target != null)
            {
                if (CheckPerms(player))
                {
                    //Construct model
                    BanModel newBan = new BanModel()
                    {
                        LicenseID = target.Identifiers["license"],
                        BanReason = reason,
                        BannedBy = player.Name,
                        FirstBanDate = DateTime.UtcNow,
                        BannedUntil = DateTime.UtcNow.AddHours(hours),
                    };

                    //Insert new ban
                    using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                    {
                        var col = db.GetCollection<BanModel>(SharedProperties.DatabaseTableBanned);
                        col.Insert(newBan);
                    }

                    //Update player info
                    using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                    {
                        var col = db.GetCollection<InfoModel>(SharedProperties.DatabaseTableInfo);
                        var playerInfo = col.FindOne(x => x.LicenseID.Equals(target.Identifiers["license"]));
                        playerInfo.IsBanned = true;
                        col.Update(playerInfo);
                    }

                    //Notify server
                    Console.WriteLine($"{player.Name} has been banned for {hours} hours for {reason} by {player.Name}");
                    ChatUtils.Instance.PrintToAll($"{target.Name} has been banned for {hours} hours for {reason}", SharedProperties.ColorGood);

                    //Kick player
                    DropPlayer(target.Handle, reason);
                }
                else
                {
                    Console.WriteLine($"Warning: Player {player.Name} tried to access an admin command.");
                }
            }
        }

        /// <summary>
        /// Unbans a player with dbID
        /// </summary>
        /// <param name="player"></param>
        /// <param name="dbID"></param>
        public void UnbanPlayer([FromSource]Player player, int dbID)
        {
            if (CheckPerms(player))
            {
                //Find entry by db id
                using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                {
                    var col = db.GetCollection<BanModel>(SharedProperties.DatabaseTableBanned);
                    var result = col.Delete(x => x.Id == dbID);
                    if(result > 0)
                    {
                        ChatUtils.Instance.PrintToClient(player, $"You have unbanned ID {dbID}", SharedProperties.ColorGood);
                    }
                    else
                    {
                        ChatUtils.Instance.PrintToClient(player, $"Failed to unban ID {dbID}", SharedProperties.ColorError);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Warning: Player {player.Name} tried to access an admin command.");
            }
        }

        /// <summary>
        /// Unbans player via license id
        /// </summary>
        /// <param name="player"></param>
        public void UnbanPlayer(string id)
        {
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<BanModel>(SharedProperties.DatabaseTableBanned);
                var result = col.Delete(x => x.LicenseID.Equals(id));
                if (result > 0)
                {
                    Console.WriteLine($"Successfully unbanned player with id: {id}");
                }
                else
                {
                    Console.WriteLine($"Failed to unban player with id: {id}");
                }
            }
        }
        #endregion

        #region Perms Management
        /// <summary>
        /// OWNER usage only. Invoke ownership via license id
        /// </summary>
        /// <param name="player"></param>
        public void InvokeOwnership([FromSource]Player player)
        {
            var playerLicense = player.Identifiers["license"];
            if (playerLicense.Equals(SharedProperties.OwnerString))
            {
                using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                {
                    var col = db.GetCollection<AdminModel>(SharedProperties.DatabaseTableAdmins);
                    var col2 = db.GetCollection<InfoModel>(SharedProperties.DatabaseTableInfo);

                    var adminProfile = col.FindOne(x => x.LicenseID.Equals(playerLicense));
                    var playerInfo = col2.FindOne(x => x.LicenseID.Equals(playerLicense));

                    if (adminProfile == null && playerInfo != null)
                    {
                        //Construct new admin profile.
                        AdminModel newProfile = new AdminModel()
                        {
                            LicenseID = playerLicense,
                            Name = player.Name
                        };

                        //Update admin status
                        playerInfo.IsAdmin = true;

                        //Insert & update new admin
                        col.Insert(newProfile);
                        col2.Update(playerInfo);

                        //Notify
                        ChatUtils.Instance.PrintToClient(player, "You've invoked ownership of the server.", SharedProperties.ColorGood);
                        Console.WriteLine($"Ownership invoked by {player.Name}");
                    }
                    else
                    {
                        ChatUtils.Instance.PrintToClient(player, "Failed to gain ownership of the server.", SharedProperties.ColorError);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Warning, player {player.Name} ({player.EndPoint} - {playerLicense}): Tried to access the invoke ownership command.");
            }
        }
        #endregion
    }
}
