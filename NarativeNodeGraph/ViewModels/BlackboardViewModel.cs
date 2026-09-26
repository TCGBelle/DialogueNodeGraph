using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public partial class BlackboardViewModel : ObservableObject
    {
        public ObservableCollection<VariableViewModel> Variables { get; } = new();

        [RelayCommand]
        private void AddVariable()
        {
            string baseName = "NewVariable";
            string name = baseName;
            int suffix = 1;

            while (Variables.Any(v => v.Name == name))
            {
                name = $"{baseName}{suffix}";
                suffix++;
            }

            var variable = new VariableViewModel(this) { Name = name };
            Variables.Add(variable);
        }

        public bool IsNameTaken(string name, VariableViewModel excluding)
        {
            return Variables.Any(v => v != excluding && v.Name == name);
        }
    }
}
