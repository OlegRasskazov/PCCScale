using System.Diagnostics.CodeAnalysis;

namespace PCCScale.Models
{
    public class PCCSlab
    {
        public required string Name { get; init; }
        public required double Length { get; init; }
        public required double Width { get; init;  }
        public required double Thickness { get; init; }
        public required double Weight { get; init; }
        public required double TargetWeightPercentage { get; init; }

        public double Volume => Length * Width * Thickness / 1000000;

        [SetsRequiredMembers]
        public PCCSlab(double weight, double thickness, double width, double length, double targetWeightPercentage, string name)
        {
            Weight = weight;
            Thickness = thickness;
            Width = width;
            Length = length;
            TargetWeightPercentage = targetWeightPercentage;
            Name = name;
        }
    }
}
