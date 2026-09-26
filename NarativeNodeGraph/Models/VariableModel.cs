using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Models
{
    public enum VariableType
    {
        Bool,
        Int
    }

    public class VariableModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "NewVariable";
        public VariableType Type { get; set; } = VariableType.Bool;
        public string Value { get; set; } = "false";
    }
}
