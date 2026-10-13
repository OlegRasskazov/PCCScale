using PCCScale.Models;
using System.IO;
using System.Text.Json;

namespace PCCScale.Services
{
    public class PCCCatalog
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "PCCSlabs.json");
        private readonly List<PCCSlab> _PCCSlabs = new();
            public IReadOnlyList<PCCSlab> PCCSlabs => _PCCSlabs;

        public PCCCatalog()
        {
            using var stream = File.OpenRead(_filePath);

            var items = JsonSerializer.Deserialize<List<PCCSlab>>(stream) ?? new List<PCCSlab>();
            _PCCSlabs.AddRange(items);
        }

        public async Task AddPCCSlab(PCCSlab pccSlab)
        {
            if (_PCCSlabs.Any(s => s.Name.Equals(pccSlab.Name, StringComparison.OrdinalIgnoreCase)))
                return;
            _PCCSlabs.Add(pccSlab);
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            await using var stream = File.Create(_filePath);

            await JsonSerializer.SerializeAsync(
                stream,
                _PCCSlabs,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
    }
}
