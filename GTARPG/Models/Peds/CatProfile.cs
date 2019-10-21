using CitizenFX.Core;

namespace GTARPGClient.Models
{
    public class CatProfile
    {
        public Player Owner { get; set; }
        public Ped Ped { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Hunger { get; set; }
        public bool IsFollow { get; set; }
    }
}
