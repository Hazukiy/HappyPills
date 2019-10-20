using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTARPG.Models
{
    public class SaveMe
    {
        public int Source { get; set; }
        public string Comment { get; set; }
        public DateTime DateOfComment { get; set; }

        public override string ToString()
        {
            return $"{Source}-{DateOfComment.ToString("ddMMyyy-hhmmss")}";
        }
    }
}
