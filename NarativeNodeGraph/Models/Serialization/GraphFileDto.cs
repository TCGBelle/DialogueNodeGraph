using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Models.Serialization
{
    public sealed class GraphFileDto
    {
        public List<NodeDto> Nodes { get; set; } = new();
        public List<ConnectionDto> Connections { get; set; } = new();
    }

    public sealed class NodeDto
    {
        public Guid Id { get; set; }
        public string Kind { get; set; } = string.Empty;

        public double X { get; set; }
        public double Y { get; set; }

        public string Title { get; set; } = string.Empty;

        // Optional per-node data
        public string? DialogueText { get; set; }
        public string? AnswerText { get; set; }

        public Dictionary<string, string> Data { get; set; } = new();

        public List<PortDto> Ports { get; set; } = new();
    }

    public sealed class PortDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public sealed class ConnectionDto
    {
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
    }
}
