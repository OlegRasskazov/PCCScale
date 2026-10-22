
using System.Globalization;
using System.IO;
using System.Text;

public sealed record PanMeasurement(
    double Weight,
    double PcmPercent);

public sealed class RecordingService
{
    private static readonly string _folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PCCScale", "Records");
    private readonly Dictionary<(int Oven, int Pan), RecordingSession> _sessions = new();

    private sealed class RecordingSession
    {
        public required CancellationTokenSource Cancellation { get; init; }
        public required string FilePath { get; init; }
        public required Task Task { get; set; }
    }

    public bool IsRecording(int oven, int pan)
    {
        return _sessions.ContainsKey((oven, pan));
    }

    public async Task StartAsync(
        int oven,
        int pan,
        string pcc,
        string note,
        Func<PanMeasurement> getMeasurement)
    {
        var key = (oven, pan);

        if (_sessions.ContainsKey(key))
            return;

        Directory.CreateDirectory(_folder);

        string safeName = string.Concat(pcc.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        string filename = $"Oven_{oven:D2}_Pan_{pan:D2}_{safeName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.csv";

        string path = Path.Combine(_folder, filename);

        await File.WriteAllTextAsync(
            path,
            $"Timestamp,Oven,Pan,PCCSlab,Weight_g,PCM_Percent,Note:{note}\n",
            Encoding.UTF8);

        var cts = new CancellationTokenSource();

        var session = new RecordingSession
        {
            Cancellation = cts,
            FilePath = path,
            Task = Task.CompletedTask
        };

        _sessions.Add(key, session);

        // Capture measurements on the WPF UI thread.
        var dispatcher = System.Windows.Application.Current.Dispatcher;

        async Task WriteMeasurementAsync()
        {
            PanMeasurement measurement = await dispatcher.InvokeAsync(getMeasurement);

            string line = string.Join(",",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                oven.ToString(CultureInfo.InvariantCulture),
                pan.ToString(CultureInfo.InvariantCulture),
                "\"" + pcc.Replace("\"", "\"\"") + "\"",
                measurement.Weight.ToString(CultureInfo.InvariantCulture),
                measurement.PcmPercent.ToString(CultureInfo.InvariantCulture));

            await File.AppendAllTextAsync(path, line + Environment.NewLine, Encoding.UTF8, cts.Token);
        }

        session.Task = Task.Run(async () =>
        {
            try
            {
                await WriteMeasurementAsync();

                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

                while (await timer.WaitForNextTickAsync(cts.Token))
                {
                    await WriteMeasurementAsync();
                }
            }
            catch (OperationCanceledException)
                when (cts.IsCancellationRequested)
            {
                // Recording stopped.
            }
        });
    }

    public async Task StopAsync(int oven, int pan)
    {
        var key = (oven, pan);

        if (!_sessions.Remove(key, out var session))
            return;

        session.Cancellation.Cancel();

        try
        {
            await session.Task;
        }
        finally
        {
            session.Cancellation.Dispose();
        }
    }
}
