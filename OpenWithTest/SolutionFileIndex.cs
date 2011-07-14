using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace OpenWithTest
{
    public interface ISolutionFileIndex
    {
        string SolutionPath { get; }
        void Add(string filePath);
        void Remove(string filePath);
        List<string> Get(string filePath);
        bool Contains(string filePath);
    }

    [DataContract]
    public class SolutionFileIndex : ISolutionFileIndex
    {
        [DataMember]
        public string SolutionPath { get; set; }

        [DataMember]
        public IDictionary<string, List<string>> Files { get; set; }

        public static ISolutionFileIndex Empty
        {
            get { return new EmptySolutionIndex(); }
        }

        private class EmptySolutionIndex : ISolutionFileIndex
        {
            public string SolutionPath
            {
                get { return null; }
            }

            public void Add(string filePath)
            {
            }

            public void Remove(string filePath)
            {
            }

            public List<string> Get(string filePath)
            {
                return new List<string>();
            }

            public bool Contains(string filePath)
            {
                return false;
            }
        }

        public SolutionFileIndex(string solutionPath)
        {
            SolutionPath = solutionPath;
            Files = new Dictionary<string, List<string>>();
        }

        public void Add(string filePath)
        {
            var fileName = GetFileNameKey(filePath);
            if (!Files.ContainsKey(fileName))
                Files[fileName] = new List<string>();

            if (!Files[fileName].Contains(filePath))
                Files[fileName].Add(filePath);
        }

        public void Remove(string filePath)
        {
            var fileName = GetFileNameKey(filePath);
            if (Files.ContainsKey(fileName))
                Files[fileName].Remove(filePath);
        }


        public List<string> Get(string filePath)
        {
            var fileName = GetFileNameKey(filePath);
            if (Files.ContainsKey(fileName))
                return Files[fileName];
            return new List<string>();
        }

        public bool Contains(string filePath)
        {
            return Get(filePath).Count > 0;
        }

        private static string GetFileNameKey(string filePath)
        {
            return Path.GetFileName(filePath).ToUpperInvariant();
        }
    }
}