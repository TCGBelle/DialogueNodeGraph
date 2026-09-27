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

        private string rawValue = "false";
        public string Value
        {
            get => rawValue;
            set
            {
                if (rawValue == value)
                    return;

                if (Type == VariableType.Int && !int.TryParse(value, out _))
                {
                    ValueError = "Enter a whole number.";
                    OnPropertyChanged(nameof(Value));   // box snaps back to the old value
                    return;
                }

                ValueError = null;
                rawValue = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        [ObservableProperty]
        private string? valueError;

        public bool HasValueError => !string.IsNullOrEmpty(ValueError);

        partial void OnValueErrorChanged(string? value) => OnPropertyChanged(nameof(HasValueError));

        public bool BoolValue
        {
            get => Value == "true";
            set => Value = value ? "true" : "false";
        }

        public bool IsBoolType => Type == VariableType.Bool;
        public bool IsIntType => Type == VariableType.Int;

        [ObservableProperty]
        private string? nameError;

        private string name = "NewVariable";

        partial void OnTypeChanged(VariableType value)
        {
            Value = value switch
            {
                VariableType.Bool => "false",
                VariableType.Int => "0",
                _ => Value
            };

            OnPropertyChanged(nameof(IsBoolType));
            OnPropertyChanged(nameof(IsIntType));
            OnPropertyChanged(nameof(BoolValue));
        }
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
        public Array AvailableTypes => Enum.GetValues(typeof(VariableType));
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
            Value = model.Value;
            ParentBlackboard = parentBlackboard;
        }
    }
}
