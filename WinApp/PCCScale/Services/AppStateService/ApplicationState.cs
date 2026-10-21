namespace PCCScale.Services.AppStateService
{
    public class ApplicationState
    {
        public List<OvenData> Ovens = new();
    }
    public class CalibrationData
    {
        public double ZeroOffsetCounts { get; set; }
        public double CountsPerGram { get; set; }
        public int Direction { get; set; }
        public DateTime CalibratedAtUtc { get; set; }
    }
    public class OvenData
    {
        public int Id { get; set; }
        public string PCM { get; set; } = string.Empty;
        public List<PanData> Pans { get; set; } = new();
    }
    public class PanData
    {
        public int Id { get; set; }
        public string SelectedPccSlab { get; set; } = string.Empty;
        public CalibrationData CalibrationData { get; set; } = new();

    }
}
