using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NarativeNodeGraph.ViewModels
{
    public sealed class ValidationIssue
    {
        public string Message { get; }
        public bool IsError { get; }

        public ValidationIssue(string message, bool isError)
        {
            Message = message;
            IsError = isError;
        }
    }

    public sealed class GraphValidationResult
    {
        public List<ValidationIssue> Issues { get; } = new();

        public bool HasErrors => Issues.Exists(i => i.IsError);

        public void AddError(string message) => Issues.Add(new ValidationIssue(message, true));
        public void AddWarning(string message) => Issues.Add(new ValidationIssue(message, false));
    }
}
