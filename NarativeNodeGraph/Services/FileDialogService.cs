using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace NarativeNodeGraph.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? ShowSaveFileDialog(string filter, string defaultExtension, string fileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExtension,
                FileName = fileName,
                AddExtension = true
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }

        public string? ShowOpenFileDialog(string filter, string defaultExtension)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                DefaultExt = defaultExtension,
                CheckFileExists = true
            };

            return dialog.ShowDialog() == true
                ? dialog.FileName
                : null;
        }
    }
}
