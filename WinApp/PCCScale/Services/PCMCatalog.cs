using PCCScale.Models;
using System.IO;
using System.Text.Json;

namespace PCCScale.Services
{
    public sealed class PCMCatalog
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "PCMs.json");
        private readonly List<PCM> _PCMs = new List<PCM>();
        public IReadOnlyList<PCM> PCMs => _PCMs;

        public PCMCatalog()
        {
            using var stream = File.OpenRead(_filePath);

            var items = JsonSerializer.Deserialize<List<PCM>>(stream);
            _PCMs.AddRange(items);
        }

        public async Task AddPCM(PCM pcm)
        {
            if (_PCMs.Any(p => p.Name.Equals(pcm.Name, StringComparison.OrdinalIgnoreCase)))
                return;
            _PCMs.Add(pcm);
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(_filePath)!);

            await using var stream = File.Create(_filePath);

            await JsonSerializer.SerializeAsync(
                stream,
                _PCMs,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
    }
}
