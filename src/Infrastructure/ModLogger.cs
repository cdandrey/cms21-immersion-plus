using System.Runtime.CompilerServices;
using MelonLoader;
using UnityEngine;

namespace Cms21ImmersionPlus
{
    public static class ModLogger
    {
        public static void Log(string msg,
            Types.LoggingLevels loggingLevel,
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            msg = (msg ?? string.Empty).Replace("\r\n", "\n");

#if NET6_0_OR_GREATER
            MelonLogger.Instance loggerInstance = Melon<Cms21ImmersionPlus.Main>.Logger;
            System.Drawing.Color modColor = System.Drawing.Color.FromArgb(4, 163, 204);
            switch (loggingLevel) {
                case Types.LoggingLevels.Normal:
                    loggerInstance.Msg(modColor,
                        string.Format("[{0}:{1}] {2}", callerName, lineNumber, msg));
                    break;
                case Types.LoggingLevels.NormalClean:
                    MelonLogger.Msg(modColor, msg);
                    break;
#else
            switch (loggingLevel) {
                case Types.LoggingLevels.Normal:
                    MelonLogger.Msg(string.Format("[{0}:{1}] {2}",
                        callerName, lineNumber, msg));
                    break;
                case Types.LoggingLevels.NormalClean:
                    MelonLogger.Msg(msg);
                    break;
#endif
                case Types.LoggingLevels.PlayerLog:
                    UnityEngine.Debug.Log(string.Format("CMS21ImmersionPlus[{0}():{1}] {2}",
                        callerName, lineNumber, msg));
                    break;
                case Types.LoggingLevels.Warning:
                    MelonLogger.Warning(string.Format("[{0}():{1}] {2}",
                        callerName, lineNumber, msg));
                    break;
                case Types.LoggingLevels.Error:
                    MelonLogger.Error(string.Format("[{0}():{1}] {2}",
                        callerName, lineNumber, msg));
                    break;
            }
        }
    }
}
