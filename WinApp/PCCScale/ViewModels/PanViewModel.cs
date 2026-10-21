using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Models;
using PCCScale.Models.Enums;
using PCCScale.Services;
using PCCScale.Services.BleService;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PCCScale.ViewModels;

public partial class PanViewModel : ObservableObject
{
    public int Id { get; init; }

    private readonly OvenViewModel _oven;
    private readonly PanSettingsDialogService _settingsDialog;

    public double LcCountSize { get; set; } //348.57;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private PanStateEnum _state;
    public int TareWeight { get; set; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Weight))]
    [NotifyPropertyChangedFor(nameof(WeightPercentage))]
    private int _absWeight;

    public ObservableCollection<PCCSlab> PCCslabs { get; }
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WeightPercentage))]
    private PCCSlab _pccSlab;
    public int Weight => Convert.ToInt32((AbsWeight - TareWeight) / LcCountSize);
    
    public double WeightPercentage
    {
        get
        {
            if (PccSlab is null || _oven.PCM is null || PccSlab.Weight == 0)
                return 0;
            var pcmWeight = PccSlab.Volume * _oven.PCM.Density - PccSlab.Weight + Weight;
            return pcmWeight * 100 / (PccSlab.Weight + pcmWeight);
        }
    }

    public ICommand OpenSettingsDialogCommand => new AsyncRelayCommand(OpenSettingsDialogAsync);
    public event Action<int>? OnPanWeghtValueUpdated;
    public PanViewModel(int number, OvenViewModel oven, IServiceProvider services)
    {
        var bleService = services.GetRequiredService<BLEService>();
        var pccSlabCatalog = services.GetRequiredService<PCCCatalog>();
        _settingsDialog = services.GetRequiredService<PanSettingsDialogService>();

        PCCslabs = new ObservableCollection<PCCSlab>(pccSlabCatalog.PCCSlabs);
        _pccSlab = PCCslabs.FirstOrDefault() ?? throw new ArgumentException("No PCCSlab available", nameof(PCCslabs));
        bleService.WeightValueUpdated += OnWeightValueUpdated;
        Id = number;
        _oven = oven;
        LcCountSize = 100;
    }

    private async Task OpenSettingsDialogAsync()
    {
        _settingsDialog.ShowSettings(this);
    }

    private void OnWeightValueUpdated(int rack, int pan, int weight)
    {
        if (rack == _oven.Number && pan == Id)
        {
            IsConnected = true;
            AbsWeight = weight - 0x800000;
            OnPanWeghtValueUpdated?.Invoke(weight);
        }
    }

}