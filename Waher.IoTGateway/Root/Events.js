function CheckEvents(TabID)
{
	try
	{
		if ("WebSocket" in window)
		{
			CheckEventsWS(TabID);
			return;
		}
	}
	catch (e)
	{
		// Try fallback mechanism
	}

	CheckEventsXHTTP(TabID);
}

function FindNeuronDomain()
{
	var Meta = document.getElementsByTagName('meta');
	var c = Meta.length;
	var i;

	for (i = 0; i < c; i++)
	{
		var Name = Meta[i].getAttribute("name");
		if (Name && Name.toLowerCase() == "neuron")
		{
			var Domain = Meta[i].getAttribute("content");

			if (Domain)
				return Domain;
			else
				break;
		}
	}

	return window.location.host;
}

function CheckEventsWS(TabID)
{
	var Location = window.location;
	var Uri;

	if (Location.protocol === "https:")
		Uri = "wss:";
	else
		Uri = "ws:";

	Uri += "//" + FindNeuronDomain() + "/ClientEventsWS";

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
			if (Socket)
			{
				if (Socket.readyState === Socket.OPEN)
				{
					Socket.send(JSON.stringify({
						"cmd": "Ping"
					}));
				}
				else
				{
					if (PingTimer)
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
				}
			}
		}, 10000);

		window.onbeforeunload = function ()
		{
			if (Socket && Socket.readyState === Socket.OPEN)
			{
				window.clearInterval(PingTimer);
				PingTimer = null;

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
		var Event;
		var s = event.data;
		if (s === "" || s === null)
			return;

		try
		{
			Event = JSON.parse(s);
		}
		catch (e)
		{
			console.log(e);
			console.log(s);
		}

		EvaluateEvent(Event);
	};

	Socket.onerror = function ()
	{
		if (PingTimer)
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
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				if (NeedsReload)
				{
					window.location.reload(false);
					return;
				}

				var Events;
				var Event;
				var i, c;

				try
				{
					var s = xhttp.responseText;

					if (s && s !== "")
					{
						try
						{
							Events = JSON.parse(s);
						}
						catch (e)
						{
							console.log(e);
							console.log(s);
						}

						if (Events)
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
	if (Event && Event.type.match(/^[a-zA-Z0-9]+$/g))
	{
		try
		{
			console.log(Event.type);
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

function OpenUrl(Url)
{
	window.open(Url, "_blank");
}

function EndsWith(String, Suffix)
{
	var c = String.length;
	var d = Suffix.length;

	if (c < d)
		return false;

	return String.substring(c - d, c) === Suffix;
}

function CheckServerInstance(ID)
{
	if (ServerID === "")
		ServerID = ID;
	else if (ServerID !== ID)
		Reload(null);
}

function POST(Content, Resource)
{
	if (Resource === undefined)
		Resource = window.location.href;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var s = xhttp.responseText;
				var i = s.indexOf("<body>");
				var j = s.indexOf("</body>");

				if (i >= 0 && j >= i)
				{
					s = s.substring(i + 6, j);
					document.body.innerHTML = s;
				}
			}
			else
				ShowError(xhttp);
		};
	}

	xhttp.open("POST", Resource, true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify({ "tab": TabID, "data": Content }));
}

function LoadContent(Id)
{
	if (ContentQueue === null)
	{
		ContentQueue = [];

		var xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function ()
		{
			if (xhttp.readyState === 4)
			{
				if (xhttp.status === 200)
				{
					console.log("Received asynchronous content " + Id);

					var Div = document.getElementById("id" + Id);
					if (Div)
						SetDynamicHtml(Div, xhttp.responseText);

					if (xhttp.getResponseHeader("X-More") === "1")
					{
						console.log("Loading more asynchronous content from " + Id);
						xhttp.open("GET", "/ClientEvents/" + Id, true);
						xhttp.send();
						return;
					}
				}
				else
				{
					console.log("Unable to receive asynchronous content " + Id);
					ShowError(xhttp);
				}

				if (ContentQueue.length === 0)
				{
					console.log("Loading of asynchronous content completed.");
					ContentQueue = null;
				}
				else
				{
					Id = ContentQueue.shift();
					console.log("Loading asynchronous content " + Id);
					xhttp.open("GET", "/ClientEvents/" + Id, true);
					xhttp.send();
				}
			};
		}

		console.log("Loading asynchronous content " + Id);
		xhttp.open("GET", "/ClientEvents/" + Id, true);
		xhttp.send();
	}
	else
	{
		console.log("Queueing loading of asynchronous content " + Id);
		ContentQueue.push(Id);
	}
}

function SetDynamicHtml(ParentElement, Html)
{
	var Script = [];
	var HtmlLower = Html.toLocaleLowerCase();
	var i = HtmlLower.indexOf("<script");

	while (i >= 0)
	{
		var j = HtmlLower.indexOf("</script>", i + 7);
		if (j < 0)
			break;

		Script.push(Html.substring(i + 8, j));

		Html = Html.substring(0, i) + Html.substring(j + 9);
		HtmlLower = HtmlLower.substring(0, i) + HtmlLower.substring(j + 9);

		i = HtmlLower.indexOf("<script", i);
	}

	ParentElement.innerHTML = Html;

	var c = Script.length;

	for (i = 0; i < c; i++)
	{
		try
		{
			eval(Script[i]);
		}
		catch (e)
		{
			console.log(e);
		}
	}
}

var ContentQueue = null;
var TabID;
var ServerID = "";
var EventCheckingEnabled = true;

try
{
	if (window.name.length === 36)
		TabID = window.name;
	else
		TabID = window.name = CreateGUID();
}
catch (e)
{
	TabID = CreateGUID();
}

CheckEvents(TabID);
