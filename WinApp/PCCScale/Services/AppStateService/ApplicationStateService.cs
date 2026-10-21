using PCCScale.Services.AppStateService;
using System.IO;
using System.Text.Json;

public sealed class ApplicationStateService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private ApplicationState _state;

    public ApplicationStateService()
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PCCScale");

        Directory.CreateDirectory(directory);

        _filePath = Path.Combine(directory, "state.json");

        _state = Load();
    }

    private ApplicationState Load()
    {
        if (!File.Exists(_filePath))
            return new ApplicationState();

        string json = File.ReadAllText(_filePath);

        return JsonSerializer.Deserialize<ApplicationState>(json)
            ?? throw new InvalidDataException(
                "Application state is invalid.");
    }
    public CalibrationData? GetCalibration(int ovenNumber, int panNumber)
        => _state.Ovens.First(x => x.Id == ovenNumber).Pans.First(x => x.Id == panNumber).CalibrationData;

    //public CalibrationData? GetCalibration(int ovenNumber, int panNumber)
    //    => _state.Ovens.First(x => x.OvenNumber == ovenNumber).Pans.First(x => x.PanNumber == panNumber).CalibrationData;

    //public async Task SaveCalibrationAsync(
    //    int ovenNumber,
    //    int panNumber,
    //    CalibrationData calibration)
    //{
    //    if (!double.IsFinite(calibration.ZeroOffsetCounts) ||
    //        !double.IsFinite(calibration.CountsPerGram) ||
    //        calibration.CountsPerGram <= 0 ||
    //        calibration.Direction is not (1 or -1))
    //    {
    //        throw new ArgumentException(
    //            "Invalid calibration data.",
    //            nameof(calibration));
    //    }

    //    await _lock.WaitAsync();

    //    try
    //    {
    //        string json = JsonSerializer.Serialize(
    //            _state, _jsonOptions);
    //        var next = JsonSerializer.Deserialize<ApplicationState>(json, _jsonOptions);
    //        next.

    //        next.Calibrations[key] = calibration;

    //        string json = JsonSerializer.Serialize(
    //            next, _jsonOptions);

    //        string tempPath = _filePath + ".tmp";

    //        await File.WriteAllTextAsync(tempPath, json);

    //        File.Move(
    //            tempPath,
    //            _filePath,
    //            overwrite: true);

    //        // Commit in-memory state only after file save.
    //        _state = next;
    //    }
    //    finally
    //    {
    //        _lock.Release();
    //    }
    //}
    //private CalibrationData GetCalibrationData
}