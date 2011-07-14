using System;
using Microsoft.VisualStudio.Shell;

namespace MattManela.OpenWithTest
{
    public interface IVisualStudioCommands
    {
        void OpenDocument(string filePath);
    }

    public class VisualStudioCommands : IVisualStudioCommands
    {
        private readonly IServiceProvider serviceProvider;

        public VisualStudioCommands(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void OpenDocument(string filePath)
        {
            VsShellUtilities.OpenDocument(serviceProvider, filePath);
        }
    }
}