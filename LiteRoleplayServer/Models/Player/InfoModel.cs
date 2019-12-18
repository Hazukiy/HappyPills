using System;

namespace LiteRoleplayServer.Models.Player
{
    public class InfoModel
    {
        public int Id { get; set; }
        public string LicenseID { get; set; }
        public string Name { get; set; }
        public string[] AllNames { get; set; }
        public DateTime FirstLoginDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string IP { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }

        public override string ToString()
        {
            var returnString = $"Info Profile({LicenseID} - {IP}) - Name: {Name} FirstLogin: {FirstLoginDate} LastLogin: {LastLoginDate} IsAdmin: {IsAdmin} IsBanned: {IsBanned} - AllNames:";
            foreach(var item in AllNames)
            {
                returnString += $" {item}";
            }

            return returnString;
        }
    }
}
