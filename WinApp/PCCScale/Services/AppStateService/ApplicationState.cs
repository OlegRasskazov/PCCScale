namespace PCCScale.Services.AppStateService
{
    public class ApplicationState
    {
        public List<OvenStateData> Ovens { get; init; } = new();
    }
    public class OvenStateData
    {
        public int Id { get; init; }
        public string SelectedPcm { get; set; } = string.Empty;
        public List<PanStateData> Pans { get; init; } = new();
    }
    public class PanStateData
    {
        public int Id { get; init; }
        public string SelectedPcc { get; set; } = string.Empty;
        public int TareWeight { get; set; }
        public double CountsPerGram { get; set; }
    }
}
