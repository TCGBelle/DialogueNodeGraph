using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public sealed class EndNodeViewModel : NodeViewModel
    {
        public override NodeKind Kind => NodeKind.End;

        public PortViewModel In { get; }

        public EndNodeViewModel(GraphViewModel parentGraph)
            : base(parentGraph)
        {
            Title = "End";
            In = AddPort(PortType.Input, "In");
        }

        public EndNodeViewModel(NodeModel model, GraphViewModel parentGraph)
            : this(model, parentGraph, null)
        {
        }

        public EndNodeViewModel(NodeModel model, GraphViewModel parentGraph, Guid? inPortId)
            : base(model, parentGraph)
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "End" : Title;
            In = AddPort(PortType.Input, "In", inPortId);
        }

    }
}
