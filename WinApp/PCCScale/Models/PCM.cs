namespace PCCScale.Models
{
    public class PCM
    {
        public string Name { get; init; } = string.Empty;
        public int Density { get; init; }
        public int SensibleHeat { get;init; }
        public int LatentHeat { get; init; }
    }
}
