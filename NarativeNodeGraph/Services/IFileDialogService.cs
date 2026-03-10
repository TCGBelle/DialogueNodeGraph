using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarativeNodeGraph.Services
{
    public interface IFileDialogService
    {
        string? ShowSaveFileDialog(string filter, string defaultExtension, string fileName);
        string? ShowOpenFileDialog(string filter, string defaultExtension);
    }
}
