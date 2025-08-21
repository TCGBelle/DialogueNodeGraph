using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NarativeNodeGraph.ViewModels
{
    public partial class PortViewModel : ObservableObject
    {
        [ObservableProperty] private string _name;
        [ObservableProperty] private PortType _type;

        public Guid Id { get; } = Guid.NewGuid();
        public NodeViewModel ParentNode { get; }

        public PortViewModel(NodeViewModel parent, string name, PortType type)
        {
            ParentNode = parent;
            _name = name;
            _type = type;
        }
    }
}
