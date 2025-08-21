using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Models
{
    public enum PortType
    {
        Input,
        Output
    }
    public class PortModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public PortType Type { get; set; }
    }
}
