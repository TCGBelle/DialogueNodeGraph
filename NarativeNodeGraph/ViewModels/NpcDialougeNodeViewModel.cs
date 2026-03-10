using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public sealed partial class NpcDialogueNodeViewModel : NodeViewModel
    {
        public override NodeKind Kind => NodeKind.NpcDialogue;

        [ObservableProperty]
        private string dialogueText = "";

        public PortViewModel In { get; }

        public PortViewModel Out { get; }

        public NpcDialogueNodeViewModel(GraphViewModel parentGraph)
           : base(parentGraph)
        {
            Title = "NPC Dialogue";
            In = AddPort(PortType.Input, "In");
            Out = AddPort(PortType.Output, "Out");
        }

        public NpcDialogueNodeViewModel(NodeModel model, GraphViewModel parentGraph)
            : this(model, parentGraph, null, null)
        {
        }

        public NpcDialogueNodeViewModel(
            NodeModel model,
            GraphViewModel parentGraph,
            Guid? inPortId,
            Guid? outPortId)
            : base(model, parentGraph)
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "NPC Dialogue" : Title;

            In = AddPort(PortType.Input, "In", inPortId);
            Out = AddPort(PortType.Output, "Out", outPortId);
        }
    }
}
