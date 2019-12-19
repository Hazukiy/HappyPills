using System;

namespace LiteRoleplay.Shared.Models
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
            return $"Id: {Id} | LicenseID: {LicenseID} | Name: {Name} | FirstLoginDate: {FirstLoginDate} | LastLoginDate: {LastLoginDate} | IP: {IP} | IsAdmin: {IsAdmin} | IsBanned: {IsBanned}";
        }
    }
}
