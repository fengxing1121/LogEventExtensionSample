// Provider for a best effort IntelliSense of Visual Studio 2017.
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Lib\jquery.d.ts" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\TcHmi.d.ts" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Controls\System\TcHmiControl\Source.d.ts" />

// Provider for a best effort IntelliSense of Visual Studio 2013/2015.
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Lib\jquery\jquery.js" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\TcHmi.js" />

(function (TcHmi) {

    var CallServerFunction = function (FunctionToCall, Parameter) {

        console.log("called");

        var Request = {
            'requestType': 'ReadWrite',
            'commands': [{
                'symbol': FunctionToCall,
                'commandOptions': ['SendErrorMessage']
            }]
        };

        if (Parameter !== undefined && Parameter !== null && Parameter !== '') {          
           Request.commands[0].writeValue = Parameter;
        }


        if (TcHmi.Server.isWebsocketReady()) {
            //Send request to the TwinCAT HMI Server
            TcHmi.Server.request(Request, function (data) {
                //Check callback data for errors
                if (data.error !== TcHmi.Errors.NONE) {
                    //Handle TcHmi.Server class level error here
                    TcHmi.Log.error('Failed to connect to server with error = ' + data.error);
                    return;
                }

                //Check response for errors
                if (data.response.error !== undefined) {
                    //Handle TwinCAT HMI Server response level error here
                    TcHmi.Log.warn(TcHmi.Log.buildMessage(data.response.error));
                    return;
                }

                //If no commands are defined in the response, return
                if (data.response.commands === undefined || !Array.isArray(data.response.commands)) return;
                //Check all commands in the response for errors
                for (var i = 0; i < data.response.commands.length; i++) {
                    
                    if (data.response.commands[i].error !== undefined) {
                        //Handle TwinCAT HMI Server command level error here
                        TcHmi.Log.warn(TcHmi.Log.buildMessage(data.response.commands[i].error));
                        TcHmi.Functions.getFunction('HandleErrorCodes')(data.response.commands[i].error);
                    }
                    else {
                        // Extract readValue if function returned data
                        if (data.response.commands[i].readValue !== undefined) {
                            TcHmi.Log.debug('Response #' + i + 'from server request -> CallServerFunction: ' + data.response.commands[i].readValue);
                        }
                    }
                }
            });
        }
    };
    
    TcHmi.Functions.registerFunction('CallServerFunction', CallServerFunction);
})(TcHmi);