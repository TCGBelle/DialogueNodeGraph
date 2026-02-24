using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Models
{
    public class ConnectionModel
    {
        public PortModel From { get; set; } = null!;
        public PortModel To { get; set; } = null!;
    }
}
