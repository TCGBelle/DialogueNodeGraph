using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NarativeNodeGraph.Models.Serialization;

namespace NarativeNodeGraph.Services
{
    public sealed class GraphJsonFileService
    {
        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public async Task SaveAsync(string filePath, GraphFileDto graph)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            if (graph is null)
                throw new ArgumentNullException(nameof(graph));

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, graph, jsonOptions);
        }

        public async Task<GraphFileDto> LoadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Graph file was not found.", filePath);

            await using var stream = File.OpenRead(filePath);
            var graph = await JsonSerializer.DeserializeAsync<GraphFileDto>(stream, jsonOptions);

            return graph ?? new GraphFileDto();
        }
    }
}
