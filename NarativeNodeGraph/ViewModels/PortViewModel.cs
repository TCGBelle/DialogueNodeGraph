using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace NarativeNodeGraph.ViewModels
{
    public partial class PortViewModel : ObservableObject
    {
        public NodeViewModel ParentNode { get; }
        
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private PortType type;
        public ICommand BeginConnectionCommand { get; }
        public ICommand EndConnectionCommand { get; }
        public PortViewModel(NodeViewModel parent, PortModel model)
        {
            ParentNode = parent;
            Name = model.Name;
            Type = model.Type;
            BeginConnectionCommand = new RelayCommand(BeginConnection);
            EndConnectionCommand = new RelayCommand(EndConnection);
        }

        private void BeginConnection()
        {
            System.Diagnostics.Debug.WriteLine("PORT CLICK!");
            ParentNode.ParentGraph.StartConnection(this);
        }

        private void EndConnection()
        {
            ParentNode.ParentGraph.CompleteConnection(this);
        }
    }
}
