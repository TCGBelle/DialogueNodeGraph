using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public sealed partial class PlayerDialogueNodeViewModel : NodeViewModel
    {
        public override NodeKind Kind => NodeKind.PlayerDialogue;

        [ObservableProperty]
        private string dialogueText = "";

        public PortViewModel In { get; }

        public PortViewModel Out { get; }

        public PlayerDialogueNodeViewModel(GraphViewModel parentGraph)
            : base(parentGraph)
        {
            Title = "Player Dialogue";
            In = AddPort(PortType.Input, "In");
            Out = AddPort(PortType.Output, "Out");
        }

        public PlayerDialogueNodeViewModel(NodeModel model, GraphViewModel parentGraph)
            : this(model, parentGraph, null, null)
        {
        }

        public PlayerDialogueNodeViewModel(
            NodeModel model,
            GraphViewModel parentGraph,
            Guid? inPortId,
            Guid? outPortId)
            : base(model, parentGraph)
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "Player Dialogue" : Title;

            In = AddPort(PortType.Input, "In", inPortId);
            Out = AddPort(PortType.Output, "Out", outPortId);
        }
    }
}
