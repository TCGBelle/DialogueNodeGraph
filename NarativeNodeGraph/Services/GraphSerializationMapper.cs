using NarativeNodeGraph.Models;
using NarativeNodeGraph.Models.Serialization;
using NarativeNodeGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NarativeNodeGraph.Services
{
    public sealed class GraphSerializationMapper
    {
        public GraphFileDto ToDto(GraphViewModel graphViewModel)
        {
            if (graphViewModel is null)
                throw new ArgumentNullException(nameof(graphViewModel));

            return new GraphFileDto
            {
                Nodes = graphViewModel.Nodes.Select(ToNodeDto).ToList(),
                Connections = graphViewModel.Connections
                    .Where(c => c.From is not null && c.To is not null)
                    .Select(c => new ConnectionDto
                    {
                        FromPortId = c.From.Id,
                        ToPortId = c.To!.Id
                    })
                    .ToList()
            };
        }

        public GraphLoadResult FromDto(GraphFileDto dto, GraphViewModel graphViewModel)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (graphViewModel is null)
                throw new ArgumentNullException(nameof(graphViewModel));

            var nodes = new List<NodeViewModel>();
            var portsById = new Dictionary<Guid, PortViewModel>();

            foreach (var nodeDto in dto.Nodes)
            {
                var node = CreateNode(nodeDto, graphViewModel);
                nodes.Add(node);

                foreach (var port in node.Ports)
                {
                    portsById[port.Id] = port;
                }
            }

            var connections = new List<ConnectionViewModel>();

            foreach (var connectionDto in dto.Connections)
            {
                if (!portsById.TryGetValue(connectionDto.FromPortId, out var fromPort))
                    continue;

                if (!portsById.TryGetValue(connectionDto.ToPortId, out var toPort))
                    continue;

                connections.Add(new ConnectionViewModel(fromPort, toPort, graphViewModel.DeleteConnectionCommand));
            }

            return new GraphLoadResult(nodes, connections);
        }

        private static NodeDto ToNodeDto(NodeViewModel node)
        {
            return new NodeDto
            {
                Id = node.Id,
                Kind = node.Kind.ToString(),
                X = node.X,
                Y = node.Y,
                Title = node.Title,
                DialogueText = GetDialogueText(node),
                AnswerText = GetAnswerText(node),
                Ports = node.Ports.Select(p => new PortDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Type = p.Type.ToString(),
                    Label = p.Label
                }).ToList()
            };
        }

        private static NodeViewModel CreateNode(NodeDto dto, GraphViewModel parentGraph)
        {
            if (!Enum.TryParse<NodeKind>(dto.Kind, ignoreCase: true, out var kind))
                throw new InvalidOperationException($"Unknown node kind '{dto.Kind}'.");

            var model = new NodeModel
            {
                Id = dto.Id,
                X = dto.X,
                Y = dto.Y,
                Title = dto.Title
            };

            return kind switch
            {
                NodeKind.Start => new StartNodeViewModel(
                    model,
                    parentGraph,
                    dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Output))?.Id),

                NodeKind.End => new EndNodeViewModel(
                    model,
                    parentGraph,
                    dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Input))?.Id),

                NodeKind.Answer => CreateAnswerNode(dto, model, parentGraph),
                NodeKind.PlayerDialogue => CreatePlayerDialogueNode(dto, model, parentGraph),
                NodeKind.NpcDialogue => CreateNpcDialogueNode(dto, model, parentGraph),
                _ => throw new InvalidOperationException($"Unsupported node kind '{kind}'.")
            };
        }

        private static AnswerNodeViewModel CreateAnswerNode(NodeDto dto, NodeModel model, GraphViewModel parentGraph)
        {
            var inPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Input))?.Id;
            var outPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Output))?.Id;

            var node = new AnswerNodeViewModel(model, parentGraph, inPortId, outPortId)
            {
                AnswerText = dto.AnswerText ?? string.Empty
            };

            return node;
        }

        private static PlayerDialogueNodeViewModel CreatePlayerDialogueNode(NodeDto dto, NodeModel model, GraphViewModel parentGraph)
        {
            var inPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Input))?.Id;
            var outPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Output))?.Id;

            var node = new PlayerDialogueNodeViewModel(model, parentGraph, inPortId, outPortId)
            {
                DialogueText = dto.DialogueText ?? string.Empty
            };

            return node;
        }

        private static NpcDialogueNodeViewModel CreateNpcDialogueNode(NodeDto dto, NodeModel model, GraphViewModel parentGraph)
        {
            var inPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Input))?.Id;
            var outPortId = dto.Ports.FirstOrDefault(p => p.Type == nameof(PortType.Output))?.Id;

            var node = new NpcDialogueNodeViewModel(model, parentGraph, inPortId, outPortId)
            {
                DialogueText = dto.DialogueText ?? string.Empty
            };

            return node;
        }

        private static string? GetDialogueText(NodeViewModel node)
        {
            return node switch
            {
                PlayerDialogueNodeViewModel player => player.DialogueText,
                NpcDialogueNodeViewModel npc => npc.DialogueText,
                _ => null
            };
        }

        private static string? GetAnswerText(NodeViewModel node)
        {
            return node is AnswerNodeViewModel answer
                ? answer.AnswerText
                : null;
        }
    }

    public sealed record GraphLoadResult(
        IReadOnlyList<NodeViewModel> Nodes,
        IReadOnlyList<ConnectionViewModel> Connections);
}
