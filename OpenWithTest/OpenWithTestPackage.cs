using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using MattManela.OpenWithTest.OptionPages;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace MattManela.OpenWithTest
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideOptionPage(typeof (OpenWithTestSettings), "Open With Test", "General", 14340, 17770, true)]
    [ProvideOptionPage(typeof(ResetOptions), "Open With Test", "Reset", 14340, 17771, true)]
    [InstalledProductRegistration("#110", "#112", "1.0.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(GuidList.autoLoadOnSolutionExists)]
    [Guid(GuidList.guidOpenWithTestPkgString)]
    public sealed class OpenWithTestPackage : Package
    {
        private ILogger logger;
        private DTE2 dte;
        private ISolutionIndexService indexService;
        private IFileCompanionOpener companionOpener;
        private OpenWithTestSettings openWithTestSettings;
        private ResetOptions resetOptions;
        private IVsStatusbar statusBar;
        private uint progressBarCookie;
        private const string ProgressBarLabel = "Building Open With Test index...";
        private OleMenuCommandService menuCommandService;
        private OleMenuCommand enableDisableCommand;

        public OpenWithTestPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }


        protected override void Initialize()
        {
            try
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
                base.Initialize();

                statusBar = GetService(typeof (SVsStatusbar)) as IVsStatusbar;
                openWithTestSettings = GetDialogPage(typeof (OpenWithTestSettings)) as OpenWithTestSettings;
                openWithTestSettings.PropertyChanged += openWithTestSettings_PropertyChanged;
                resetOptions = GetDialogPage(typeof(ResetOptions)) as ResetOptions;
                logger = new Logger(this);
                dte = (DTE2) GetService(typeof (DTE));
                if (dte == null)
                    throw new ArgumentNullException("dte");


               var enableDisableCommandID = new CommandID(GuidList.guidOpenWithTestCmdSet,(int)PkgCmdIDList.cmdidEnableDisable);
               enableDisableCommand = DefineCommandHandler(EnableDisableOpenWithTest, enableDisableCommandID);
               enableDisableCommand.Visible = true;
               enableDisableCommand.Checked = openWithTestSettings.EnableAutoOpen;


                indexService = new SolutionIndexService(logger, dte, new FileSystemWrapper());
                companionOpener = new FileCompanionOpener(logger, indexService, new VisualStudioCommands(this), new FileCompanionFinder(logger, indexService));

                resetOptions.IndexService = indexService;
                indexService.FileOpened += indexService_FileOpened;
                indexService.IndexLoadingStarted += indexService_IndexLoadingStarted;
                indexService.IndexLoadingProgress += indexService_IndexLoadingProgress;
                indexService.IndexLoadingFinished += indexService_IndexLoadingFinished;

            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                if(logger != null)
                    logger.Log(e.Message, "OpenWithTestPackage::Initialzie", e);
            }
        }

        void openWithTestSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var settings = sender as OpenWithTestSettings;
            if (settings == null) return;
            if(e.PropertyName.Equals("EnableAutoOpen", StringComparison.OrdinalIgnoreCase))
            {
                enableDisableCommand.Checked = settings.EnableAutoOpen;
            }
        }

        private void indexService_IndexLoadingStarted()
        {
            statusBar.Progress(ref progressBarCookie, 1, "", 0, 0);
        }

        private void indexService_IndexLoadingProgress(uint progress, uint total)
        {
            if (progressBarCookie > 0)
                statusBar.Progress(ref progressBarCookie, 1, ProgressBarLabel, progress, total);
        }

        private void indexService_IndexLoadingFinished()
        {
            if (progressBarCookie > 0)
                statusBar.Progress(ref progressBarCookie, 0, "", 0, 0);
            progressBarCookie = 0;
        }

        private void indexService_FileOpened(string filePath)
        {
            if (openWithTestSettings.EnableAutoOpen)
                companionOpener.OpenFileCompanion(filePath, openWithTestSettings.TestClassSuffixes);
        }

        private void EnableDisableOpenWithTest(object sender, EventArgs e)
        {
            OleMenuCommand command = sender as OleMenuCommand;
            if(command != null)
            {
                openWithTestSettings.EnableAutoOpen = !openWithTestSettings.EnableAutoOpen;
                command.Checked = openWithTestSettings.EnableAutoOpen;
            }
        }

        /// <summary>
        /// Define a command handler.
        /// When the user press the button corresponding to the CommandID
        /// the EventHandler will be called.
        /// </summary>
        /// <param name="id">The CommandID (Guid/ID pair) as defined in the .ctc file</param>
        /// <param name="handler">Method that should be called to implement the command</param>
        /// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
        internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
        {
            // if the package is zombied, we don't want to add commands
            if (Zombied)
                return null;

            // Make sure we have the service
            if (menuCommandService == null)
            {
                // Get the OleCommandService object provided by the MPF; this object is the one
                // responsible for handling the collection of commands implemented by the package.
                menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            }
            OleMenuCommand command = null;
            if (null != menuCommandService)
            {
                // Add the command handler
                command = new OleMenuCommand(handler, id);
                menuCommandService.AddCommand(command);
            }
            return command;
        }
    }
}