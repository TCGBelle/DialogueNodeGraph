using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NarativeNodeGraph.ViewModels
{
    public enum NodeKind
    {
        Start,
        End,
        NpcDialogue,
        Answer,
        PlayerDialogue
    }
    public abstract partial class NodeViewModel : ObservableObject
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public abstract NodeKind Kind { get; }
        [ObservableProperty]
        private double x;

        [ObservableProperty]
        private double y;

        [ObservableProperty]
        private string title = "";

        [ObservableProperty]
        private bool isSelected;
        [ObservableProperty]
        private double width = double.NaN;
        [ObservableProperty]
        private double height = double.NaN;

        public double MinWidth => 100;
        public double MinHeight => 80;

        public ObservableCollection<PortViewModel> Ports { get; } = new();
        public IRelayCommand<(double X, double Y)> DragCommand { get; }
        public ICommand SelectCommand { get; }
        public ICommand ClickCommand { get; }
        public GraphViewModel ParentGraph { get; set; }

        public virtual object? BodyContent => null;
        protected NodeViewModel(GraphViewModel parentGraph)
        {
            ParentGraph = parentGraph ?? throw new ArgumentNullException(nameof(parentGraph));
            DragCommand = new RelayCommand<(double X, double Y)>(OnDrag);
            SelectCommand = new RelayCommand<bool>(ctrlHeld => ParentGraph.SelectNode(this, ctrlHeld));
            ClickCommand = new RelayCommand(() => ParentGraph.SelectOnly(this));
            Ports.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Ports));
        }
        public NodeViewModel(NodeModel model, GraphViewModel parentGraph)
        {
            x = model.X;
            y = model.Y;
            title = model.Title;
            Id = model.Id;
            ParentGraph = parentGraph;
            DragCommand = new RelayCommand<(double X, double Y)>(OnDrag);
            SelectCommand = new RelayCommand<bool>(ctrlHeld => ParentGraph.SelectNode(this, ctrlHeld));
            ClickCommand = new RelayCommand(() => ParentGraph.SelectOnly(this));
            Ports.CollectionChanged += (_, __) => OnPropertyChanged(nameof(Ports));
        }
        protected PortViewModel AddPort(PortType type, string? label = null, Guid? fixedId = null)
        {
            var portName = type == PortType.Input ? "Input" : "Output";

            var port = new PortViewModel(
                this,
                portName,
                type,
                label,
                fixedId);

            Ports.Add(port);
            return port;
        }

        protected void RemovePort(PortViewModel port)
        {
            if (Ports.Contains(port))
                Ports.Remove(port);
        }


        public IEnumerable<PortViewModel> InputPorts => Ports.Where(p => p.Type == PortType.Input);
        public IEnumerable<PortViewModel> OutputPorts => Ports.Where(p => p.Type == PortType.Output);

        private void OnDrag((double X, double Y) delta)
        {
            if (IsSelected)
            {
                ParentGraph.MoveSelected(delta);
                return;
            }
            // this is duplicate code but it fixes a double selection bug odly where double selecting a node two diffreent ways deselcts it but not if this is here
            double currZoom = ParentGraph.Zoom;
            Debug.WriteLine("Dragging node with delta: " + delta + " and current zoom: " + currZoom);
            X += delta.X/currZoom;
            Y += delta.Y/currZoom;
        }

        public void SetSize(double width, double height)
        {
            Width = Math.Max(MinWidth, width);
            Height = Math.Max(MinHeight, height);
        }

        public void RemovePortPublic(PortViewModel port)
        {
            RemovePort(port);
        }
    }

}
