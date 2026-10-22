using CommunityToolkit.Mvvm.ComponentModel;

namespace PCCScale.Models
{
    public partial class PCCViewModel : ObservableObject
    {
        public string PartNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        [ObservableProperty]
        public double _length;
        [ObservableProperty]
        public double _width;
        [ObservableProperty]
        public double _thickness;
        [ObservableProperty]
        public double _weight;
        [ObservableProperty]
        public double _swellingCoefficient;
        [ObservableProperty]
        public double _targetWeightPercentage;
        [ObservableProperty]
        public PCM _pcm = new PCM();
        [ObservableProperty]
        public string _note = string.Empty;
        public double Volume { get; private set; }
        public double BuoyancyGrowthFactor { get; private set; }
        public double Buoyancy { get; private set; }

        partial void OnLengthChanged(double value) => CalculateVolume();
        partial void OnWidthChanged(double value) => CalculateVolume();
        partial void OnThicknessChanged(double value) => CalculateVolume();

        partial void OnTargetWeightPercentageChanged(double value) => CalculateBuoyancyGrowthFactor();
        partial void OnSwellingCoefficientChanged(double value) => CalculateBuoyancyGrowthFactor();
        
        partial void OnPcmChanged(PCM value) => CalculateBuoyancy();

        private void CalculateVolume()
        {
            Volume = Length * Width * Thickness / 1000000; 
            CalculateBuoyancy();
        }
        private void CalculateBuoyancyGrowthFactor()
        {
            if (Weight != 0 && TargetWeightPercentage != 0)
            {
                BuoyancyGrowthFactor = SwellingCoefficient * (100 - TargetWeightPercentage) / (Weight * TargetWeightPercentage);
            }
            else
            {
                BuoyancyGrowthFactor = 0;
            }
        }
        private void CalculateBuoyancy()
        {
            if(Pcm == null || Volume == 0)
            {
                Buoyancy = 0;
                return;
            }
            Buoyancy = Volume * Pcm.Density;
        }
    }
}
