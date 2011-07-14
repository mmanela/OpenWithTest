using System;
using System.Collections.Generic;
using System.Linq;

namespace MattManela.OpenWithTest
{
    public interface IFileCompanionOpener
    {
        void OpenFileCompanion(string filePath, IList<string> testClassSuffixes);
    }

    public class FileCompanionOpener : IFileCompanionOpener
    {
        private readonly ILogger logger;
        private readonly IVisualStudioCommands visualStudioCommands;
        private readonly ISolutionIndexService indexService;
        private readonly IFileCompanionFinder fileCompanionFinder;

        public FileCompanionOpener(ILogger logger, ISolutionIndexService indexService, IVisualStudioCommands visualStudioCommands, IFileCompanionFinder fileCompanionFinder)
        {
            this.logger = logger;
            this.visualStudioCommands = visualStudioCommands;
            this.indexService = indexService;
            this.fileCompanionFinder = fileCompanionFinder;
        }

        public void OpenFileCompanion(string filePath, IList<string> testClassSuffixes)
        {
            // Check if this file is opened during solution start up 
            // if it is then lets not open its companion
            if (indexService.IsFileOpen(filePath))
                return;
            var companionFiles = fileCompanionFinder.FindFileCompanions(filePath, testClassSuffixes);
            OpenCompanion(filePath, companionFiles);
        }


        private void OpenCompanion(string openedFile, IEnumerable<string> companionFiles)
        {
            if (companionFiles.Count() <= 0) return;
            if (companionFiles.All(x => !indexService.IsFileOpen(x)))
            {
                try
                {
                    visualStudioCommands.OpenDocument(companionFiles.First());
                    visualStudioCommands.OpenDocument(openedFile);
                }
                catch (Exception e)
                {
                    logger.Log("Unable to open documents", "FileComanionOpener", e);
                }
            }
        }
    }
}