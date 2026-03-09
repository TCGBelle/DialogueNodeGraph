using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public sealed partial class AnswerNodeViewModel : NodeViewModel
    {
        public override NodeKind Kind => NodeKind.Answer;

        [ObservableProperty]
        private string answerText = "";

        public PortViewModel In { get; }
        public PortViewModel Out { get; }

        public AnswerNodeViewModel(NodeModel model, GraphViewModel parentGraph)
            : base(model, parentGraph)
        {
            Title = string.IsNullOrWhiteSpace(Title) ? "Answer" : Title;

            In = AddPort(PortType.Input, "In");
            Out = AddPort(PortType.Output, "Out");
        }
    }
}
