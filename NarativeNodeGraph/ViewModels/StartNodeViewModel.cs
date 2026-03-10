using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public sealed class StartNodeViewModel : NodeViewModel
    {
        public override NodeKind Kind => NodeKind.Start;

        public PortViewModel Out { get; }

        public StartNodeViewModel(GraphViewModel parentGraph)
    : base(parentGraph)
        {
            Title = "Start";
            Out = AddPort(PortType.Output, "Out");
        }

        public StartNodeViewModel(NodeModel model, GraphViewModel parentGraph)
            : this(model, parentGraph, null)
        {
        }

        public StartNodeViewModel(NodeModel model, GraphViewModel parentGraph, Guid? outPortId)
            : base(model, parentGraph)
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "Start" : Title;
            Out = AddPort(PortType.Output, "Out", outPortId);
        }
    }
}

