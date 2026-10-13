using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PCCScale.Models;
using PCCScale.Models.Enums;
using PCCScale.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PCCScale.ViewModels;

public class PanViewModel : INotifyPropertyChanged
{
    public int Number { get; init; }

    private readonly OvenViewModel _oven;
    private readonly PanSettingsDialogService _settingsDialog;

    public double LcCountSize { get; set; } = 348.57;

    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value)
                return;
            _isConnected = value;
            OnPropertyChanged();
        }
    }
    private PanStateEnum _state;
    public PanStateEnum State { get => _state; set => _state = value; }
    public int TareWeight { get; set; }
    private int _absWeight;
    public int AbsWeight
    {
        get => _absWeight; 
        set
        {

            {
                if (_absWeight == value)
                    return;

                _absWeight = value;
                OnPropertyChanged(nameof(Weight));
                OnPropertyChanged(nameof(WeightPercentage));
            }
        }
    }
    private PCCSlab _pccSlab;
    public PCCSlab PCCslab
    {
        get => _pccSlab;
        set
        {
            if (_pccSlab == value)
                return;

            _pccSlab = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WeightPercentage));
        }
    }
    public int Weight
    {
        get => Convert.ToInt32((AbsWeight - TareWeight) / LcCountSize);
    }
    public double WeightPercentage
    {
        get
        {
            if (PCCslab is null || _oven.PCM is null || PCCslab.Weight == 0)
                return 0;
            var pcmWeight = PCCslab.Volume * _oven.PCM.Density - PCCslab.Weight + Weight;
            return pcmWeight * 100 / (PCCslab.Weight + pcmWeight);
        }
    }
    public ObservableCollection<PCCSlab> PCCslabs { get; }

    public ICommand OpenSettingsDialogCommand => new AsyncRelayCommand(OpenSettingsDialogAsync);


    public PanViewModel(int number, OvenViewModel oven, IServiceProvider services)
    {
        var bleService = services.GetRequiredService<BLEService>();
        var pccSlabCatalog = services.GetRequiredService<PCCCatalog>();
        _settingsDialog = services.GetRequiredService<PanSettingsDialogService>();

        PCCslabs = new ObservableCollection<PCCSlab>(pccSlabCatalog.PCCSlabs);
        _pccSlab = PCCslabs.FirstOrDefault() ?? throw new ArgumentException("No PCCSlab available", nameof(PCCslabs));
        bleService.WeightValueUpdated += OnWeightValueUpdated;
        Number = number;
        _oven = oven;
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    private async Task OpenSettingsDialogAsync()
    {
        _settingsDialog.ShowSettings(this);
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
    private void OnWeightValueUpdated(int rack, int pan, int weight)
    {
        if (rack == _oven.Number && pan == Number)
        {
            IsConnected = true;
            AbsWeight = weight - 0x800000;
        }
    }

}