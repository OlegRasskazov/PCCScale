using System.Text.Json;

namespace PCCScale.Services.PCCSlabCatalog
{
    public class PCCCatalog
    {
        private static readonly string _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCCScale");
        private static readonly string _userFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCCScale", "PCCs.json");
        private static readonly string _defaultFilePath = Path.Combine(AppContext.BaseDirectory, "PCCCatalog", "PCCs.json");
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "PCCSlabs.json");
        private readonly List<PCC> _PCCSlabs = new();

        public PCCCatalog()
        {
            InitializeFile();
            using var stream = File.OpenRead(_userFilePath);

            var items = JsonSerializer.Deserialize<List<PCC>>(stream) ?? new List<PCC>();
            _PCCSlabs.AddRange(items);
        }

        public async Task SavePCCSlab(PCC pccSlab)
        {
            if (_PCCSlabs.Any(s => s.Name.Equals(pccSlab.Name, StringComparison.OrdinalIgnoreCase)))
                return;
            _PCCSlabs.Add(pccSlab);
            await SaveAsync();
        }

        public PCC? GetPCCSlabByName(string name) => _PCCSlabs.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public IList<string> GetPCCSlabNames() => _PCCSlabs.Select(s => s.Name).ToList();

        private async Task SaveAsync()
        {
            await using var stream = File.Create(_userFilePath);

            await JsonSerializer.SerializeAsync(
                stream,
                _PCCSlabs,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }

        private void InitializeFile()
        {
            if (!File.Exists(_userFilePath) && File.Exists(_defaultFilePath))
            {
                Directory.CreateDirectory(_appDataFolder);
                File.Copy(_defaultFilePath, _userFilePath);
            }
        }
    }
}
