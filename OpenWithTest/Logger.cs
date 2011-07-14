﻿using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell.Interop;

namespace MattManela.OpenWithTest
{
    public enum LogType
    {
        Information,
        Warning,
        Error
    }

    public class Logger : ILogger
    {
        IServiceProvider serviceProvider;
        public Logger(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Log(string message, string source, LogType logType)
        {
            IVsActivityLog log = serviceProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null) return;
            int hr = log.LogEntry((UInt32)ToEntryType(logType), source, message);
        }


        public void Log(string message, string source, Exception e)
        {
            string format = "Message: {0} \n Exception Message: {1} \n Stack Trace: {2}";
            IVsActivityLog log = serviceProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;
            if (log == null) return;
            int hr = log.LogEntry((UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, source, string.Format(CultureInfo.CurrentCulture, format, message, e.Message, e.StackTrace));
        }


        public void MessageBox(string title, string message, LogType logType)
        {
            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO;
            if (logType == LogType.Error) icon = OLEMSGICON.OLEMSGICON_CRITICAL;
            else if (logType == LogType.Warning) icon = OLEMSGICON.OLEMSGICON_WARNING;

            IVsUIShell uiShell = (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            uiShell.ShowMessageBox(0, ref clsid, title, message, string.Empty,
                0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                icon, 0, out result);

        }

        private __ACTIVITYLOG_ENTRYTYPE ToEntryType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Information:
                    return __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION;
                case LogType.Warning:
                    return __ACTIVITYLOG_ENTRYTYPE.ALE_WARNING;
                case LogType.Error:
                    return __ACTIVITYLOG_ENTRYTYPE.ALE_ERROR;
                default:
                    return __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION;
            }

        }

    }
}
