using CommunityToolkit.Mvvm.ComponentModel;
using NarativeNodeGraph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.ViewModels
{
    public partial class VariableViewModel : ObservableObject
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        [ObservableProperty]
        private VariableType type = VariableType.Bool;

        [ObservableProperty]
        private string value = "false";

        [ObservableProperty]
        private string? nameError;

        private string name = "NewVariable";
        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                if (ParentBlackboard.IsNameTaken(value, this))
                {
                    NameError = "A variable with this name already exists.";
                    OnPropertyChanged(nameof(Name));   // tells the TextBox to revert to the old value
                    return;
                }

                NameError = null;
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public BlackboardViewModel ParentBlackboard { get; set; }

        public VariableViewModel(BlackboardViewModel parentBlackboard)
        {
            ParentBlackboard = parentBlackboard;
        }

        public VariableViewModel(VariableModel model, BlackboardViewModel parentBlackboard)
        {
            Id = model.Id;
            name = model.Name;
            type = model.Type;
            value = model.Value;
            ParentBlackboard = parentBlackboard;
        }
    }
}
