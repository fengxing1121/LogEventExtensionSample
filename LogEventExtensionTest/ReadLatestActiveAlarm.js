// Provider for a best effort IntelliSense of Visual Studio 2017.
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Lib\jquery.d.ts" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\TcHmi.d.ts" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Controls\System\TcHmiControl\Source.d.ts" />

// Provider for a best effort IntelliSense of Visual Studio 2013/2015.
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\Lib\jquery\jquery.js" />
/// <reference path="C:\TwinCAT\Functions\TE2000-HMI-Engineering\Infrastructure\TcHmiFramework\Latest\TcHmi.js" />

(function (TcHmi) {
    // If you want to unregister an event outside the event code you need to use the return value of the method register()
    var destroyOnInitialized = TcHmi.EventProvider.register('onInitialized', function (e, data) {
        // This event will be raised only once, so we can free resources. 
        // It's best practice to use destroy function of the event object within the callback function to avoid conflicts.
        e.destroy();

        var textblock = TcHmi.Controls.get('TcHmiTextblock_LatestEvent');

        if (!textblock) {
            return;
        }

        // filter for events which are alarms and have timeConfirmed unset or timeCleared unset
        // adjust this to fit your needs
        var filter = [
            {
                path: 'type',
                comparator: '==',
                value: TcHmi.Server.Events.Type.Alarm
            },
            {
                logic: 'AND'
            },
            [
                {
                    path: 'timeConfirmed',
                    comparator: '==',
                    value: '1970-01-01T00:00:00.000Z'
                },
                {
                    logic: 'OR'
                },
                {
                    path: 'timeCleared',
                    comparator: '==',
                    value: '1970-01-01T00:00:00.000Z'
                }
            ]
        ];

        // we need to keep a list of active alarms so we can show another alarm if the currently displayed one gets cleared and confirmed
        var activeAlarms = [];

        // function to handle list of events. This function will be called when the event consumer is first registered and when the locale changes.
        function consumeEventList(data) {
            if (data.error !== TcHmi.Errors.NONE) {
                // handle error
                return;
            }

            activeAlarms = data.events; // data.events is an array of events
            updateAlarmDisplay();
        }

        // function to handle new incoming events
        function consumeEventSubscription(data) {
            if (data.error !== TcHmi.Errors.NONE) {
                // handle error
                return;
            }

            var event = data.event;
            var changeType = data.changeType; // enum of TcHmi.Server.Events.ChangeType. AlarmRaised: 0, AlarmChanged: 1, AlarmDisposed: 2, MessageSent: 3
            var removedByFilter = data.removedByFilter; // true if the alarm has changed in such a way that it no longer matches the filter. Means this alarm should be removed and there will be no further updates.

            if (removedByFilter) {
                activeAlarms = activeAlarms.filter(alarm => alarm.id !== event.id);
            }
            else if (changeType === TcHmi.Server.Events.ChangeType.AlarmRaised) {
                activeAlarms.push(event);
            }
            else {
                for (var i = 0; i < activeAlarms.length; i++) {
                    if (activeAlarms[i].id === event.id) {
                        activeAlarms[i] = event;
                        break;
                    }
                }
            }

            updateAlarmDisplay();
        }

        function updateAlarmDisplay() {
            if (activeAlarms.length === 0) {
                textblock.setText('No active alarms');
            }
            else {
                // sort by time raised, newest first
                activeAlarms.sort(function (a, b) { return b.timeRaised - a.timeRaised });
                textblock.setText(activeAlarms[0].text);
            }
        }

        // register a consumer to receive events
        // the destroy function can be used to stop receiving events
        var destroyFunction = TcHmi.Server.Events.registerConsumer(filter, { list: consumeEventList, subscription: consumeEventSubscription });
    });
})(TcHmi);

