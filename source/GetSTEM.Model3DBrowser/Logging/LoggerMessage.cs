using System.Diagnostics;
using System.Reflection;

namespace GetSTEM.Model3DBrowser.Logging
{
    internal class LoggerMessage
    {
        internal static string CreateMessage(string message)
        {
            return CreateMessage(message, null);
        }

        internal static string CreateMessage(string message, StackFrame frame)
        {
            string finalMessage = string.Empty;

            if (frame != null)
            {
                MethodBase method = frame.GetMethod();
                //finalMessage = string.Format("{0} - {1} - {2}", method.DeclaringType.FullName, method.Name, message);
                finalMessage = string.Format("{0}", message);
            }
            else
                finalMessage = string.Format("{0}", message);

            return finalMessage;
        }
    }
}
