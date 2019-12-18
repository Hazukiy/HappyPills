using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRoleplayServer.Models.Admin
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
