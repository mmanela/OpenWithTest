using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MattManela.OpenWithTest
{
    public interface IFileCompanionFinder
    {
        IEnumerable<string> FindFileCompanions(string filePath, IEnumerable<string> testClassSuffixes);
    }

    public class FileCompanionFinder : IFileCompanionFinder
    {
        private readonly ILogger logger;
        private readonly ISolutionIndexService indexService;

        public FileCompanionFinder(ILogger logger, ISolutionIndexService indexService)
        {
            this.logger = logger;
            this.indexService = indexService;
        }

        public IEnumerable<string> FindFileCompanions(string filePath, IEnumerable<string> testClassSuffixes)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);

            var suffix = testClassSuffixes.SingleOrDefault(x => name.EndsWith(x,StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(suffix))
            {
                return FindTestClass(name, Path.GetExtension(filePath), filePath, testClassSuffixes);
            }
            else
            {
                return FindImplementationClass(name.Substring(0, name.Length - suffix.Length), Path.GetExtension(filePath), filePath);
            }
        }

        private IEnumerable<string> FindImplementationClass(string name, string extension, string filePath)
        {
            var files = indexService.GetPathsForFileName(name + extension);
            if (files.Count > 0)
                return FindBestMatches(filePath, files);
            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> FindTestClass(string name, string extension, string filePath, IEnumerable<string> testClassSuffixes)
        {
            foreach (var suffix in testClassSuffixes)
            {
                var files = indexService.GetPathsForFileName(name + suffix + extension);
                if (files.Count > 0)
                    return FindBestMatches(filePath, files);
            }

            return Enumerable.Empty<string>();
        }


        private static IEnumerable<string> FindBestMatches(string filePath, IEnumerable<string> companionPaths)
        {
            var fileTokens = TokenizePath(filePath);

            var bestMatches =
                from companionPath in companionPaths
                let similarity = fileTokens.Zip(TokenizePath(companionPath), (x, y) => x == y).TakeWhile(x => x).Count()
                group companionPath by similarity into paths
                orderby paths.Key descending
                select paths;

            var best = bestMatches.FirstOrDefault();
            return best != null ? best.Select(x => x) : Enumerable.Empty<string>();
        }

        private static IEnumerable<string> TokenizePath(string path)
        {
            return path.Split(Path.DirectorySeparatorChar).Reverse().Skip(1);
        }
    }
}