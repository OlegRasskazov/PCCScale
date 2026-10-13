namespace PCCScale.Models
{
    public class PCM
    {
        public string Name { get; }
        public int Density { get; }
        public int SensibleHeat { get; } = 0;
        public int LatentHeat { get; } = 0;

        public PCM(string name, int density)
        {
            Name = name;
            Density = density;
        }
    }
}
