using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace PCCScale.ViewModels
{
    internal partial class PanSettingsViewModel : ObservableObject
    {
        public PanViewModel Pan { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEditingEnabled))]
        [NotifyPropertyChangedFor(nameof(CalibrationMessage))]
        [NotifyPropertyChangedFor(nameof(IsContinueCalibrationVisible))]
        [NotifyPropertyChangedFor(nameof(IsCalibrating))]
        private CalibrationStateEnum _calibrationState;

        /// <summary>
        /// Commands
        /// </summary>
        public ICommand TareCommand => new AsyncRelayCommand(TareAsync);
        public ICommand CalibrateCommand => new AsyncRelayCommand(CalibrateAsync);
        public ICommand ContinueCalibrationCommand => new AsyncRelayCommand(ContinueCalibrationAsync);
        private async Task TareAsync() => Pan.TareWeight = Pan.AbsWeight;
        private async Task CalibrateAsync()
        {
            CalibrationState = CalibrationStateEnum.ZeroCalibrating;
            Pan.OnPanWeightValueChanged += OnPanWeghtValueChangedHandler;
        }
        private async Task ContinueCalibrationAsync() => CalibrationState = CalibrationStateEnum.ReferenceWeightCalibrating;

        public string CalibrationMessage
        {
            get => CalibrationState switch
            {
                CalibrationStateEnum.None => "Calibration required.",
                CalibrationStateEnum.ZeroCalibrating => "Zero weight calibration in progress.",
                CalibrationStateEnum.ZeroCalibrationReady => "Zero calibration is completed. Put reference weight on scale and press start to calibrate reference.",
                CalibrationStateEnum.ReferenceWeightCalibrating => "Reference weight calibration in progress.",
                CalibrationStateEnum.ReferenceWeighCalibrationtReady => "Recalibrate.",
                _ => throw new NotImplementedException()
            };
        }
        public bool IsEditingEnabled => CalibrationState == CalibrationStateEnum.None || CalibrationState == CalibrationStateEnum.ReferenceWeighCalibrationtReady;
        public bool IsContinueCalibrationEnabled => ReferenceWeight > 0;
        public bool IsCalibrating => CalibrationState == CalibrationStateEnum.ZeroCalibrating || CalibrationState == CalibrationStateEnum.ReferenceWeightCalibrating;
        public bool IsContinueCalibrationVisible => CalibrationState == CalibrationStateEnum.ZeroCalibrationReady;
        public PanSettingsViewModel(PanViewModel pan)
        {
            Pan = pan;
            _calibrationState = pan.CountsPerGram == 0 ? CalibrationStateEnum.None : CalibrationStateEnum.ReferenceWeighCalibrationtReady;
        }

        private void OnPanWeghtValueChangedHandler(int weight)
        {
            switch (CalibrationState)
            {
                case CalibrationStateEnum.ZeroCalibrating:
                    ZeroADCWeightsList.Add(weight);
                    if (ZeroADCWeightsList.Count > 10) CalibrationState = CalibrationStateEnum.ZeroCalibrationReady;
                    break;
                case CalibrationStateEnum.ReferenceWeightCalibrating:
                    ReferenceADCWeightsList.Add(weight);
                    if (ReferenceADCWeightsList.Count > 10)
                    {
                        Pan.OnPanWeightValueChanged -= OnPanWeghtValueChangedHandler;
                        Pan.CountsPerGram = (ZeroADCWeightsList.Average() - ReferenceADCWeightsList.Average()) / ReferenceWeight;
                        ZeroADCWeightsList.Clear();
                        ReferenceADCWeightsList.Clear();
                        ReferenceWeight = 0;
                        CalibrationState = CalibrationStateEnum.ReferenceWeighCalibrationtReady;
                    }
                    break;
            }
        }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsContinueCalibrationEnabled))]
        private int _referenceWeight;
        private List<int> ZeroADCWeightsList = new();
        private List<int> ReferenceADCWeightsList = new();

        public void UpdateState()
        {
            Pan.UpdatePanStateData();
        }
    }
    internal enum CalibrationStateEnum
    {
        None,
        ZeroCalibrating,
        ZeroCalibrationReady,
        ReferenceWeightCalibrating,
        ReferenceWeighCalibrationtReady
    }

}