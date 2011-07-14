using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using OpenWithTest;

namespace MattManela.OpenWithTest
{
    public interface ISolutionIndexService
    {
        event Action<string> FileOpened;
        event Action<string> FileClosed;
        event Action IndexLoadingStarted;
        event Action<uint, uint> IndexLoadingProgress;
        event Action IndexLoadingFinished;

        List<string> GetPathsForFileName(string filePath);
        bool IsFileOpen(string filePath);

        void DeleteIndexFile();
    }

    public class SolutionIndexService : ISolutionIndexService
    {
        public event Action<string> FileOpened;
        public event Action<string> FileClosed;
        public event Action IndexLoadingStarted;
        public event Action<uint, uint> IndexLoadingProgress;
        public event Action IndexLoadingFinished;

        private const string CSharpProjectItemEventsName = "CSharpProjectItemsEvents";
        private readonly ILogger logger;
        private readonly DTE2 dte;
        private readonly IFileSystemWrapper fileSystemWrapper;
        private IDictionary<string, bool> LoadedFiles { get; set; }
        private readonly DocumentEvents documentEvents;
        private readonly ProjectItemsEvents csharpProjectItemEvents;
        private readonly SolutionEvents solutionEvents;
        private ISolutionFileIndex solutionFileIndex = SolutionFileIndex.Empty;
        private readonly DataContractSerializer serializer = new DataContractSerializer(typeof(SolutionFileIndex));
        private bool stopIndexBuilding;

        public SolutionIndexService(ILogger logger, DTE2 dte, IFileSystemWrapper fileSystemWrapper)
        {
            LoadedFiles = new Dictionary<string, bool>();
            this.logger = logger;
            this.dte = dte;
            this.fileSystemWrapper = fileSystemWrapper;
            documentEvents = dte.Events.DocumentEvents;
            csharpProjectItemEvents = dte.Events.SolutionItemsEvents;
            solutionEvents = dte.Events.SolutionEvents;

            csharpProjectItemEvents = (ProjectItemsEvents)dte.Events.GetObject(CSharpProjectItemEventsName);
            csharpProjectItemEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ErrorWrapper<ProjectItem>(ProjectItemEventsItemAdded));
            csharpProjectItemEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ErrorWrapper<ProjectItem>(ProjectItemsEventsItemRemoved));
            csharpProjectItemEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ErrorWrapper<ProjectItem, string>(ProjectItemsEventsItemRenamed));

            documentEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(ErrorWrapper<Document>(DocumentEvents_DocumentOpened));
            documentEvents.DocumentClosing += new _dispDocumentEvents_DocumentClosingEventHandler(ErrorWrapper<Document>(DocumentEvents_DocumentClosing));
            solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(ErrorWrapper(SolutionEvents_Opened));
            solutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(ErrorWrapper(SolutionEvents_BeforeClosing));
        }



        public Action ErrorWrapper(Action action)
        {
            return () =>
                       {
                           try
                           {
                               action();
                           }
                           catch (Exception e)
                           {
                               logger.Log(e.Message, "SolutionIndexService", e);
                           }
                       };
        }

        public Action<T1> ErrorWrapper<T1>(Action<T1> action)
        {
            return (x) =>
            {
                try
                {
                    action(x);
                }
                catch (Exception e)
                {
                    logger.Log(e.Message, "SolutionIndexService", e);
                }
            };
        }

        public Action<T1, T2> ErrorWrapper<T1, T2>(Action<T1, T2> action)
        {
            return (x, y) =>
            {
                try
                {
                    action(x, y);
                }
                catch (Exception e)
                {
                    logger.Log(e.Message, "SolutionIndexService", e);
                }
            };
        }


        public bool IsFileOpen(string filePath)
        {
            return LoadedFiles.ContainsKey(filePath) && LoadedFiles[filePath];
        }

        public void DeleteIndexFile()
        {
            try
            {
                if (dte.Solution != null && !string.IsNullOrEmpty(dte.Solution.FullName))
                {
                    if (DoesCachedIndexExist(dte.Solution.FullName))
                    {
                        var cachePath = GetCachePathFromSolutionPath(dte.Solution.FullName);
                        stopIndexBuilding = true;
                        fileSystemWrapper.DeleteFile(cachePath);
                        solutionFileIndex = SolutionFileIndex.Empty;
                        IndexLoadingFinished();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log(e.Message, "SolutionIndexService", e);
            }
        }

        public List<string> GetPathsForFileName(string filePath)
        {
            if (solutionFileIndex != null)
                return solutionFileIndex.Get(filePath);
            return new List<string>();
        }

        private void Intialize()
        {
            LoadedFiles.Clear();
            BuildOpenFilesIndex();
            BuildSolutionFilesIndex();
        }


        private void BuildOpenFilesIndex()
        {
            if (dte.ActiveWindow != null)
            {
                foreach (Window2 window in dte.ActiveWindow.Collection)
                {
                    try
                    {
                        if (window.ProjectItem != null && IsFile(window.ProjectItem))
                            LoadedFiles[window.ProjectItem.FileNames[0]] = true;
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Unable to determine what the project item is", "SolutionIndexService", ex);
                    }
                }
            }
        }

        private bool IsFile(ProjectItem projectItem)
        {
            try
            {
                return projectItem.Kind.Equals(Constants.vsProjectItemKindPhysicalFile);
            }
            catch (Exception ex)
            {
                logger.Log("Unable to determine if project item is file", "SolutionIndexService", ex);
            }

            return false;
        }

        private void TraverseProjectTree(ProjectItems projectItems, ISolutionFileIndex index)
        {
            foreach (ProjectItem projectItem in projectItems)
            {
                if (IsFile(projectItem))
                {
                    index.Add(projectItem.FileNames[0]);
                }
                else if (projectItem.ProjectItems != null && projectItem.ProjectItems.Count > 0)
                    TraverseProjectTree(projectItem.ProjectItems, index);
            }
        }

        private void BuildSolutionFilesIndex()
        {
            try
            {
                if (dte.Solution != null && !string.IsNullOrEmpty(dte.Solution.FullName) && !stopIndexBuilding)
                {
                    bool doesCachedIndexExist = DoesCachedIndexExist(dte.Solution.FullName);
                    if (doesCachedIndexExist)
                        solutionFileIndex = LoadFileIndex(dte.Solution.FullName);
                    else
                        OnIndexLoadingStarted();

                    var updatedSolutionFileIndex = new SolutionFileIndex(dte.Solution.FullName);
                    var projectCount = (uint)dte.Solution.Projects.Count;
                    if (!doesCachedIndexExist)
                        OnIndexLoadingProgress(0, projectCount);
                    uint index = 0;
                    foreach (Project project in dte.Solution.Projects)
                    {
                        if (stopIndexBuilding) return;

                        TraverseProjectTree(project.ProjectItems, updatedSolutionFileIndex);
                        if (!doesCachedIndexExist)
                            OnIndexLoadingProgress(++index, projectCount);
                    }
                    Interlocked.Exchange(ref solutionFileIndex, updatedSolutionFileIndex);
                    SaveFileIndex();
                    if (!doesCachedIndexExist)
                        OnIndexLoadingFinished();
                }
            }
            catch (Exception ex)
            {
                logger.Log("Unable to build or load solution index", "SolutionIndexService", ex);
            }
        }


        private string GetAppDataDir()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            var authorFolder = Path.Combine(appDataFolder, "MattManela");
            var appFolder = Path.Combine(authorFolder, "OpenWithTest");
            fileSystemWrapper.CreateDirectory(appFolder);
            return appFolder;
        }

        public bool DoesCachedIndexExist(string solutionPath)
        {
            var path = GetCachePathFromSolutionPath(solutionPath);
            return fileSystemWrapper.Exists(path);
        }

        public void SaveFileIndex()
        {
            if (stopIndexBuilding)
            {
                stopIndexBuilding = false;
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(solutionFileIndex.SolutionPath))
                    using (
                        var stream = fileSystemWrapper.Open(
                            GetCachePathFromSolutionPath(solutionFileIndex.SolutionPath), FileMode.Create,
                            FileAccess.Write))
                        serializer.WriteObject(stream, solutionFileIndex);
            }
            catch (SerializationException e)
            {
                logger.Log(e.Message, "SolutionIndexService::LoadFileIndex", e);
            }
            catch (IOException e)
            {
                logger.Log(e.Message, "SolutionIndexService::LoadFileIndex", e);
            }

        }

        private string GetCachePathFromSolutionPath(string solutionPath)
        {
            var cacheFileName = Path.ChangeExtension(solutionPath.Replace(Path.DirectorySeparatorChar, '-').Replace(":", "-"), "xml");
            var appDataFolder = GetAppDataDir();
            return Path.Combine(appDataFolder, cacheFileName);
        }

        public SolutionFileIndex LoadFileIndex(string solutionPath)
        {
            SolutionFileIndex index = null;

            try
            {
                var path = GetCachePathFromSolutionPath(solutionPath);
                if (fileSystemWrapper.Exists(path))
                {
                    using (var stream = fileSystemWrapper.Open(path, FileMode.Open, FileAccess.Read))
                        index = serializer.ReadObject(stream) as SolutionFileIndex;
                }
            }
            catch (XmlException e)
            {
                logger.Log(e.Message, "SolutionIndexService::LoadFileIndex", e);
            }
            catch (IOException e)
            {
                logger.Log(e.Message, "SolutionIndexService::LoadFileIndex", e);
            }

            return index ?? new SolutionFileIndex(solutionPath);
        }


        private void SolutionEvents_BeforeClosing()
        {
            SaveFileIndex();
        }

        private void ProjectItemEventsItemAdded(ProjectItem projectItem)
        {
            if (IsFile(projectItem))
                solutionFileIndex.Add(projectItem.FileNames[0]);
        }

        private void ProjectItemsEventsItemRenamed(ProjectItem projectItem, string oldName)
        {
            if (IsFile(projectItem))
            {
                var newFilePath = projectItem.FileNames[0];
                var oldFilePath =
                    Path.Combine(newFilePath.Substring(0, newFilePath.Length - Path.GetFileName(newFilePath).Length),
                                 oldName);
                solutionFileIndex.Remove(oldFilePath);
                solutionFileIndex.Add(projectItem.FileNames[0]);
            }
        }

        private void ProjectItemsEventsItemRemoved(ProjectItem projectItem)
        {
            if (IsFile(projectItem))
                solutionFileIndex.Remove(projectItem.FileNames[0]);
        }

        private void SolutionEvents_Opened()
        {
            if (string.IsNullOrEmpty(dte.Solution.FullName)) return;
            stopIndexBuilding = false;
            ThreadPool.QueueUserWorkItem(state => Intialize());
        }

        private void DocumentEvents_DocumentClosing(Document document)
        {
            OnFileClosed(document.FullName);
            LoadedFiles[document.FullName] = false;
        }

        private void DocumentEvents_DocumentOpened(Document document)
        {
            OnFileOpend(document.FullName);
            LoadedFiles[document.FullName] = true;
        }


        private void OnFileOpend(string path)
        {
            if (FileOpened != null)
                FileOpened(path);
        }

        private void OnFileClosed(string path)
        {
            if (FileClosed != null)
                FileClosed(path);
        }

        private void OnIndexLoadingStarted()
        {
            if (IndexLoadingStarted != null)
                IndexLoadingStarted();
        }

        private void OnIndexLoadingProgress(uint progress, uint total)
        {
            if (IndexLoadingProgress != null)
                IndexLoadingProgress(progress, total);
        }

        private void OnIndexLoadingFinished()
        {
            if (IndexLoadingFinished != null)
                IndexLoadingFinished();
        }

    }
}