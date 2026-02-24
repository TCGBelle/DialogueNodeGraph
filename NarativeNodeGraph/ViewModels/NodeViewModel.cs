using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections.ObjectModel;

namespace NarativeNodeGraph.ViewModels
{
    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private double x;

        [ObservableProperty]
        private double y;

        [ObservableProperty]
        private string title;
        public double NodeWidth => 100;

        public ObservableCollection<PortViewModel> Ports { get; } = new();
        public IRelayCommand<(double X, double Y)> DragCommand { get; }

        public GraphViewModel ParentGraph { get; set; }
        public NodeViewModel(NodeModel model, GraphViewModel parentGraph)
        {
            x = model.X;
            y = model.Y;
            title = model.Title;

            DragCommand = new RelayCommand<(double X, double Y)>(OnDrag);
            ParentGraph = parentGraph;
        }

        private void OnDrag((double X, double Y) delta)
        {
            X += delta.X;
            Y += delta.Y;
        }
    }

}
