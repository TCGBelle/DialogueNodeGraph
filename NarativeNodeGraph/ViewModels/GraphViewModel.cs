using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class GraphViewModel : ObservableObject
{
    public ObservableCollection<NodeViewModel> Nodes { get; } = new();

    public GraphViewModel()
    {
        // Add a sample node for now
        Nodes.Add(new NodeViewModel(new NarativeNodeGraph.Models.NodeModel { X = 100, Y = 100, Title = "Sample Node" }));
    }
}
