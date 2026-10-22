namespace PCCScale.Services.PCCSlabCatalog
{
    public class PCC
    {
        public string PartNumber { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public double Length { get; init; }
        public double Width { get; init; }
        public double Thickness { get; init; }
        public double Weight { get; init; }
        public double SwellingCoefficient { get; init; }
        public double TargetWeightPercentage { get; init; }
    }
}
