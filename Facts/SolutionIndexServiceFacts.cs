using EnvDTE80;
using Moq;

namespace MattManela.OpenWithTest.Facts
{
    public class SolutionIndexServiceFacts
    {
        public class TestableSolutionIndexService : SolutionIndexService
        {
            public Mock<ILogger> MoqLogger { get; set; }
            public Mock<DTE2> MoqDTE { get; set; }
            public Mock<IFileSystemWrapper> MoqFileSystemWrapper { get; set; }

            private TestableSolutionIndexService(Mock<ILogger> moqLogger, Mock<DTE2> moqDTE, Mock<IFileSystemWrapper> moqFileSystemWrapper)
                : base(moqLogger.Object, moqDTE.Object, moqFileSystemWrapper.Object)
            {
                MoqLogger = moqLogger;
                MoqDTE = moqDTE;
                MoqFileSystemWrapper = moqFileSystemWrapper;
            }

            public static TestableSolutionIndexService Create()
            {
                var service = new TestableSolutionIndexService(new Mock<ILogger>(), new Mock<DTE2>(), new Mock<IFileSystemWrapper>());

                return service;
            }
        }
    }
}