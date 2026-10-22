using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Models;
using PCCScale.Models.Enums;
using PCCScale.Services;
using PCCScale.Services.AppStateService;
using PCCScale.Services.BleService;
using PCCScale.Services.PCCSlabCatalog;
using System.Windows.Input;

namespace PCCScale.ViewModels;

public partial class PanViewModel : ObservableObject
{
    private readonly OvenViewModel _oven;
    private PanStateData _panStateData;
    private readonly PCCCatalog _pccCatalog;
    private readonly RecordingService _recordingService;
    private readonly PanSettingsDialogService _settingsDialog;

    public int Id { get; init; }
    private int _measurementCount = 0;
    private List<int> rawMeasurmentList = new();

    public double CountsPerGram { get; set; }

    [ObservableProperty]
    private bool _isConnected;
    [ObservableProperty]
    private bool isRecording;
    public string RecordButtonText => IsRecording ? "Stop" : "Record";
    partial void OnIsRecordingChanged(bool value)
    {
        OnPropertyChanged(nameof(RecordButtonText));
    }

    [ObservableProperty]
    private PanStateEnum _state;
    public int TareWeight { get; set; }
    [ObservableProperty]
    private int _absWeight;
    [ObservableProperty]
    private double _weight;
    [ObservableProperty]
    private double _slabWeight;
    [ObservableProperty]
    private double _slabWeightPercentage;

    public IList<string> Pccs { get; }
    [ObservableProperty]
    private string _selectedPccName;

    [ObservableProperty]
    public PCCViewModel _pcc;

    //public double WeightPercentage { get; private set; }
    //{
    //    get
    //    {
    //        if (PccSlab is null || _oven.PCM is null || PccSlab.Weight == 0)
    //            return 0;
    //        var a = PccSlab.ExpansionCoefficient*(100-PccSlab.TargetWeightPercentage)/(PccSlab.Weight*PccSlab.TargetWeightPercentage);
    //        var fa = PccSlab.Volume * _oven.PCM.Density;

    //        var pcmWeight = (Weight - fa + PccSlab.Weight) / (fa * a - 1);

    //        //var pcmWeight = PccSlab.Volume * _oven.PCM.Density - PccSlab.Weight + Weight;
    //        return pcmWeight * 100 / (PccSlab.Weight + pcmWeight);
    //    }
    //}

    public void UpdatePanStateData()
    {
        _panStateData.TareWeight = TareWeight;
        _panStateData.CountsPerGram = CountsPerGram;
        _panStateData.SelectedPcc = Pcc?.Name ?? "Undefined";
    }

    public ICommand OpenSettingsDialogCommand => new AsyncRelayCommand(OpenSettingsDialogAsync);

    [RelayCommand]
    private async Task ToggleRecordingAsync()
    {
        if (IsRecording)
        {
            await _recordingService.StopAsync(
                _oven.Id, Id);

            IsRecording = false;
            return;
        }

        await _recordingService.StartAsync(_oven.Id, Id, Pcc.Name, Pcc.Note,
            () => new PanMeasurement(SlabWeight,
                                     SlabWeightPercentage));

        IsRecording = true;
    }

    public event Action<int>? OnPanWeightValueChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="PanViewModel"/> class.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="id"></param>
    /// <param name="oven"></param>
    /// <param name="panStateData"></param>
    /// <exception cref="ArgumentException"></exception>
    // TO DO: Consider refactoring this constructor to reduce the number of parameters and improve testability.
    public PanViewModel(IServiceProvider services, int id, OvenViewModel oven, PanStateData panStateData)
    {
        Id = id;
        _oven = oven;
        _panStateData = panStateData;

        _settingsDialog = services.GetRequiredService<PanSettingsDialogService>();
        _recordingService = services.GetRequiredService<RecordingService>();
        _pccCatalog = services.GetRequiredService<PCCCatalog>();
        // TO DO: try propogating the event from the service to the view model instead of subscribing to it directly here.
        services.GetRequiredService<BLEService>().WeightValueUpdated += OnWeightValueUpdated;

        Pccs = _pccCatalog.GetPCCSlabNames();
        SelectedPccName = _panStateData.SelectedPcc;
        Pcc = GetPccViewModel(SelectedPccName);

        CountsPerGram = panStateData.CountsPerGram;
        TareWeight = panStateData.TareWeight;
    }

    private async Task OpenSettingsDialogAsync() => _settingsDialog.ShowSettings(this);

    private void OnWeightValueUpdated(int rack, int pan, int weight)
    {
        if (rack == _oven.Id && pan == Id)
        {
            IsConnected = true;
            if (_measurementCount < 3)
            {
                rawMeasurmentList.Add(weight);
                _measurementCount++;
                return;
            }
            _measurementCount = 0;
            AbsWeight = (int)(rawMeasurmentList.Average()) - 0x800000;

            Weight = (AbsWeight - TareWeight) / CountsPerGram;

            var pcmWeight = (Pcc.Weight - Weight - Pcc.Buoyancy) / (Pcc.Buoyancy * Pcc.BuoyancyGrowthFactor - 1);

            SlabWeight = Pcc.Weight + pcmWeight;
            SlabWeightPercentage = pcmWeight * 100 / SlabWeight;

            rawMeasurmentList.Clear();
            OnPanWeightValueChanged?.Invoke(weight);
        }
    }
    partial void OnSelectedPccNameChanged(string value) => Pcc = GetPccViewModel(value);
    private PCCViewModel GetPccViewModel(string pccName)
    {
        var pcc = _pccCatalog.GetPCCSlabByName(pccName) ?? throw new ArgumentException("No PCC available", nameof(pccName));
        return new PCCViewModel
        {
            Pcm = _oven.PCM,
            PartNumber = pcc.PartNumber,
            Name = pcc.Name,
            Length = pcc.Length,
            Width = pcc.Width,
            Thickness = pcc.Thickness,
            Weight = pcc.Weight,
            SwellingCoefficient = pcc.SwellingCoefficient,
            TargetWeightPercentage = pcc.TargetWeightPercentage
        };
    }

}