namespace PCCScale.Services.BleService
{
    public class BleOptions
    {
        public const string SectionName = "Ble";

        public string DeviceName { get; set; } = string.Empty;

        public Guid ServiceUuid { get; set; }
        public Guid WeightUuid { get; set; }
        public Guid CommandUuid { get; set; }
        public Guid StatusUuid { get; set; }

    }
}
