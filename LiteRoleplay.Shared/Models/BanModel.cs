//Class definition: Database ban model for ban collection.

using System;

namespace LiteRoleplay.Shared
{
    public class BanModel
    {
        public int Id { get; set; }

        public string LicenseID { get; set; }

        public string BanReason { get; set; }

        public string BannedBy { get; set; }

        public DateTime FirstBanDate { get; set; }

        public DateTime BannedUntil { get; set; }
    }
}
