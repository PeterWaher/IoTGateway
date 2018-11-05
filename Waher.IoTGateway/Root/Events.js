function CheckEvents(TabID)
{
    if ("WebSocket" in window)
        CheckEventsWS(TabID);
    else
        CheckEventsXHTTP(TabID);
}

function CheckEventsWS(TabID)
{
    var Location = window.location;
    var Uri;

    if (Location.protocol == "https:")
        Uri = "wss:";
    else
        Uri = "ws:";

    Uri += "//" + Location.host + "/ClientEventsWS";

    var Socket = new WebSocket(Uri, ["ls"]);
    var PingTimer = null;
    var Closed = false;

    Socket.onopen = function ()
    {
        Socket.send(JSON.stringify({
            "cmd": "Register",
            "tabId": TabID,
            "location": window.location.href
        }));

        PingTimer = window.setInterval(function ()
        {
            Socket.send(JSON.stringify({
                "cmd": "Ping"
            }));
        }, 10000);

        window.onbeforeunload = function ()
        {
            if (Socket != null && Socket.readyState == Socket.OPEN)
            {
                Socket.send(JSON.stringify({
                    "cmd": "Unregister"
                }));

                Socket.close(1000, "Page closed.");
                Socket = null;
            }

            Closed = true;
        };
    };

    Socket.onmessage = function (event)
    {
        var s = event.data;
        if (s == "" || s == null)
            return;

        try
        {
            Event = JSON.parse(s);
        }
        catch (e)
        {
            throw "Invalid JSON received: " + event.data;
        }

        EvaluateEvent(Event);
    };

    Socket.onerror = function ()
    {
        delete Socket;

        if (PingTimer !== null)
        {
            window.clearInterval(PingTimer);
            PingTimer = null;
        }

        if (!Closed)
        {
            window.setTimeout(function ()
            {
                NeedsReload = true;
                CheckEventsWS(TabID);
            }, 5000);
        }
    };
}

function CheckEventsXHTTP(TabID)
{
    var NeedsReload = false;
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState == 4)
        {
            if (xhttp.status == 200)
            {
                if (NeedsReload)
                {
                    delete xhttp;
                    window.location.reload(false);
                    return;
                }

                var Events;
                var Event;
                var i, c;

                try
                {
                    var s = xhttp.responseText;

                    if (s != null && s != "")
                    {
                        try
                        {
                            Events = JSON.parse(s);
                        }
                        catch (e)
                        {
                            throw "Invalid JSON received: " + s;
                        }

                        if (Events != null)
                        {
                            c = Events.length;
                            for (i = 0; i < c; i++)
                                EvaluateEvent(Events[i]);
                        }
                    }
                }
                finally
                {
                    if (EventCheckingEnabled)
                    {
                        xhttp.open("POST", "/ClientEvents", true);
                        xhttp.setRequestHeader("Content-Type", "text/plain");
                        xhttp.setRequestHeader("X-TabID", TabID);
                        xhttp.send(window.location.href);
                    }
                    else
                        delete xhttp;
                }
            }
            else
            {
                ShowError(xhttp);

                if (EventCheckingEnabled)
                {
                    window.setTimeout(function ()
                    {
                        NeedsReload = true;
                        xhttp.open("POST", "/ClientEvents", true);
                        xhttp.setRequestHeader("Content-Type", "text/plain");
                        xhttp.setRequestHeader("X-TabID", TabID);
                        xhttp.send(window.location.href);
                    }, 5000);
                }
                else
                    delete xhttp;
            }
        };
    }

    EventCheckingEnabled = true;

    xhttp.open("POST", "/ClientEvents", true);
    xhttp.setRequestHeader("Content-Type", "text/plain");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(window.location.href);
}

function EvaluateEvent(Event)
{
    if (Event != null && Event.type.match(/^[a-zA-Z0-9]+$/g))
    {
        try
        {
            eval(Event.type + "(Event.data)");
        }
        catch (e)
        {
            console.log(e);
        }
    }
}

function CloseEvents()
{
    EventCheckingEnabled = false;
}

function ShowError(xhttp)
{
    if (xhttp.responseText.length > 0)
        window.alert(xhttp.responseText);
}

function CreateGUID()
{
    function Segment()
    {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }

    return Segment() + Segment() + '-' + Segment() + '-' + Segment() + '-' + Segment() + '-' + Segment() + Segment() + Segment();
}

function NOP(Data)
{
}

function Reload(Data)
{
    window.location.reload(false);
}

function EndsWith(String, Suffix)
{
    var c = String.length;
    var d = Suffix.lenght;

    if (c < d)
        return false;

    return String.substring(c - d, c) === Suffix;
}

function CheckServerInstance(ID)
{
    if (ServerID == "")
        ServerID = ID;
    else if (ServerID != ID)
        Reload(null);
}

var TabID;
var ServerID = "";
var EventCheckingEnabled = true;

try
{
    if (window.name.length == 36)
        TabID = window.name;
    else
        TabID = window.name = CreateGUID();
}
catch (e)
{
    TabID = CreateGUID();
}

CheckEvents(TabID);
