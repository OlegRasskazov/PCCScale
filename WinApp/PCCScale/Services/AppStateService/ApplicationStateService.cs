using PCCScale.Services.AppStateService;
using System.IO;
using System.Text.Json;

public sealed class ApplicationStateService
{
    private readonly string _defaultFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "default_state.json");
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

    public ApplicationState GetState() => _state;

    private ApplicationState Load()
    {
        ApplicationState state;
        string json;
        if (!File.Exists(_filePath))
        {
            _lock.WaitAsync().Wait();
            try
            {
                using var stream = File.OpenRead(_defaultFilePath);
                state = JsonSerializer.Deserialize<ApplicationState>(stream) ?? throw new InvalidDataException("Default application state is invalid.");

                json = JsonSerializer.Serialize(state, _jsonOptions);

                string tempPath = _filePath + ".tmp";

                File.WriteAllTextAsync(tempPath, json).Wait();

                File.Move(tempPath, _filePath, overwrite: true);
            }
            finally
            {
                _lock.Release();
            }

            return state;
        }

        json = File.ReadAllText(_filePath);
        state = JsonSerializer.Deserialize<ApplicationState>(json)
            ?? throw new InvalidDataException("Application state is invalid.");

        return state;
    }

    public async Task WriteStateAsync()
    {
        string tempPath = _filePath + ".tmp";
        string backupPath = _filePath + ".bak";

        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            _state,
            _jsonOptions);

        await using (var stream = new FileStream(
            tempPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            options: FileOptions.Asynchronous))
        {
            await stream.WriteAsync(json);
            await stream.FlushAsync();
            stream.Flush(flushToDisk: true);
        }

        if (File.Exists(_filePath))
        {
            File.Replace(tempPath, _filePath, backupPath);
        }
        else
        {
            File.Move(tempPath, _filePath);
        }
    }
}