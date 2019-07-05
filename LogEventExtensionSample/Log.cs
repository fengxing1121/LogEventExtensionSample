using System.Collections.Generic;
using TcHmiSrv;

namespace LogEventExtensionSample
{
    // Allows to send messages to the server log.
    public class Log
    {
        private ITcHmiSrvExtHost _host = null;

        public Log(ITcHmiSrvExtHost host)
        {
            _host = host;
        }

        // Sends a message with the specified context, name, parameter and severity.
        public void Send(Context context, string name, string parameter, Severity severity = Severity.Severity_Verbose)
        {
            Send(context, name, new string[] { parameter }, severity);
        }

        // Sends a message with the specified context, name, parameters and severity.
        public void Send(Context context, string name, IEnumerable<string> parameters = null, Severity severity = Severity.Severity_Verbose)
        {
            Message message = new Message(context, severity, name);

            if (parameters != null)
            {
                foreach (string parameter in parameters)
                {
                    message.Parameters.insert(message.Parameters.Size.ToString(), parameter);
                }
            }

            _host.send(context, message);
        }

        // Localizes a message with the specified context, name and parameter.
        public string Localize(Context context, string name, string parameter)
        {
            return Localize(context, name, new string[] { parameter });
        }

        // Localizes a message with the specified context, name and parameters.
        public string Localize(Context context, string name, IEnumerable<string> parameters = null)
        {
            Message message = new Message();
            message.Name = name;
            message.Domain = context.Domain;

            if (parameters != null)
            {
                foreach (string parameter in parameters)
                {
                    message.Parameters.insert(message.Parameters.Size.ToString(), parameter);
                }
            }

            return _host.localize(context, message).Name;
        }
    }
}
