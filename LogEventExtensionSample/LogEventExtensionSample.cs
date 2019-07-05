using System;
using System.Collections.Concurrent;
using TcHmiSrv;
using TcHmiSrv.Management;

namespace LogEventExtensionSample
{
    // The default class of the server extension must inherit from the IExtension interface.
    // The path of the server extension is available via the ExtensionPath property if the default class of the server extension inherits from the ExtensionWithPath base class instead.
    public class LogEventExtensionSample : IExtension
    {
        private ITcHmiSrvExtHost _host = null;

        private object _shutdownLock = new object();
        private bool _isShuttingDown = false;

        private TcHmiSrvRequestListener _requestListener = new TcHmiSrvRequestListener();
        private TcHmiSrvShutdownListener _shutdownListener = new TcHmiSrvShutdownListener();

        private Log _logger = null;

        // Initializes the server extension.
        public ErrorValue Init(ITcHmiSrvExtHost host, Context context)
        {
            _logger = new Log(host);

            try
            {
                _host = host;

                // Add event handlers
                _requestListener.OnRequest += OnRequest;
                _shutdownListener.OnShutdown += OnShutdown;

                // Register listeners
                _host.register_listener(context, _requestListener);
                _host.register_listener(context, _shutdownListener);

                _logger.Send(context, "MESSAGE_INIT");
                return ErrorValue.HMI_SUCCESS;
            }
            catch (Exception ex)
            {
                _logger.Send(context, "ERROR_INIT", ex.Message, Severity.Severity_Error);
                return ErrorValue.HMI_E_EXTENSION_LOAD;
            }
        }

        // Called when a client requests a symbol from the extension domain.
        private ErrorValue OnRequest(object sender, TcHmiSrvRequestListener.OnRequestEventArgs e)
        {
            ErrorValue ret = ErrorValue.HMI_SUCCESS;
            Context context = e.Context;
            CommandGroup commands = e.Commands;

            try
            {
                commands.Result = ExtensionErrorValue.HMI_EXT_SUCCESS;
                string mapping = "";

                foreach (Command command in commands)
                {
                    mapping = command.Mapping;

                    try
                    {
                        // Use the mapping to check which command is requested
                        switch (mapping)
                        {
                            case "LogParameterChangeEvent":
                                ret = LogParameterChangeEvent(context, command);
                                break;

                            case "LogEvent":
                                ret = LogEvent(context, command);
                                break;

                            default:
                                ret = ErrorValue.HMI_E_EXTENSION;
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        command.ExtensionResult = ExtensionErrorValue.HMI_EXT_FAIL;
                        command.ResultString = _logger.Localize(context, "ERROR_CALL_COMMAND", new string[] { mapping, ex.Message });
                    }
                }
            }
            catch
            {
                commands.Result = ExtensionErrorValue.HMI_EXT_FAIL;
            }
            finally
            {
                if (commands.Result != ExtensionErrorValue.HMI_EXT_SUCCESS)
                {
                    // Reset the read value of the commands to prevent the server from sending invalid data
                    foreach (Command command in commands)
                    {
                        command.ReadValue = null;
                    }

                    ret = ErrorValue.HMI_E_EXTENSION;
                }
            }

            return ret;
        }

        // Called when the server is shutting down. After exiting this function the extension dll will be unloaded.
        private ErrorValue OnShutdown(object sender, TcHmiSrvShutdownListener.OnShutdownEventArgs e)
        {
            // If the extension does not shutdown after 10 seconds (blocking threads) OnShutdown will be called again
            lock (_shutdownLock)
            {
                if (_isShuttingDown)
                {
                    return ErrorValue.HMI_SUCCESS;
                }

                _isShuttingDown = true;

                Context context = e.Context;

                try
                {
                    // Unregister listeners
                    _host.unregister_listener(context, _requestListener);
                    _host.unregister_listener(context, _shutdownListener);

                    _logger.Send(context, "MESSAGE_SHUTDOWN");
                    return ErrorValue.HMI_SUCCESS;
                }
                catch (Exception ex)
                {
                    _logger.Send(context, "ERROR_SHUTDOWN", ex.Message, Severity.Severity_Error);
                    return ErrorValue.HMI_E_EXTENSION;
                }
            }
        }

        private ErrorValue LogParameterChangeEvent(Context context, Command command)
        {
            Message message = new Message(context, Severity.Severity_Info, "LOG > " + getKeyValueFromCommand(command, "symbol") 
                + " changed from " + getKeyValueFromCommand(command, "changedFrom") + " to " + getKeyValueFromCommand(command, "changedTo"));

            message.Parameters.insert("user", getKeyValueFromCommand(command, "user"));
            message.Parameters.insert("comment", getKeyValueFromCommand(command, "comment"));

            _host.send(context, message);

            return ErrorValue.HMI_SUCCESS;
        }

        private Value getKeyValueFromCommand(Command command, System.String key)
        {
            Value writeValue = command.WriteValue;

            if (writeValue.IsSet)
            {

                TcHmiSrv.ValueType type = writeValue.Type;

                if ((type == TcHmiSrv.ValueType.ValueType_Map) || (type == TcHmiSrv.ValueType.ValueType_Struct))
                    return writeValue[key];

            }

            return null;
        }

        private ErrorValue LogEvent(Context context, Command command)
        {
            Message message = new Message(context, Severity.Severity_Info, "LOG > Event: " + getKeyValueFromCommand(command, "details"));

            message.Parameters.insert("user", getKeyValueFromCommand(command, "user"));

            _host.send(context, message);

            return ErrorValue.HMI_SUCCESS;
        }
    }
}
