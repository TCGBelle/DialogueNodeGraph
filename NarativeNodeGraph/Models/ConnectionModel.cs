using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Models
{
    public class ConnectionModel
    {
        public Guid ForwardPortID { get; set; }
        public Guid BackwardPortID { get; set; }
    }
}
