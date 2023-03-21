using NLog;
using System;
using System.Diagnostics;

namespace Caro.Game.Utilts
{
    public class LogUtil
    {
        volatile static Logger Log = LogManager.GetLogger("Trace");
        volatile static Logger LogAuth = LogManager.GetLogger("Authen");
        volatile static Logger LogError = LogManager.GetLogger("TraceError");

        public static void LogAuthen(string message)
        {
            LogAuth.Info(":\t" + GetCalleeString() + "\t" + message );
        }

        public static void LogMessage(string message)
        {
            Log.Info(":\t" + GetCalleeString() + "\t" + message);
        }

        public static void LogFailed(Exception ex)
        {
            LogError.Error(string.Format("\t{0}{1}{2}{3}{4}", GetCalleeString(), Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
        }

        private static string GetCalleeString()
        {
            foreach (StackFrame sf in new StackTrace().GetFrames())
            {
                if (!string.IsNullOrEmpty(sf.GetMethod().ReflectedType.Namespace) && !typeof(LogUtil).FullName.StartsWith(sf.GetMethod().ReflectedType.Namespace))
                {
                    return string.Format("{0}.{1} ", sf.GetMethod().ReflectedType.Name, sf.GetMethod().Name);
                }
            }
            return string.Empty;
        }
    }
}
