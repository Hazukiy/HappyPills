//Class definition: Jobs model
namespace LiteRoleplay.Shared
{
    public class JobsModel
    {
        public int JobID { get; set; }
        public string JobName { get; set; }
        public string[] Models { get; set; }
        public string Description { get; set; }
        public int Salary { get; set; }
        public bool IsPolice { get; set; }
        public bool IsAdmin { get; set; }
        public string[] SpawnWeapons{ get; set; }
        public float[] SpawnPoint { get; set; }

        public override string ToString()
        {
            return JobName;
        }
    }
}
