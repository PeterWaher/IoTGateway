﻿function FormDataStoreHandler()
{
	const SESSION_STORAGE_NAME = "events_form_data";
	let enabled = true;
	let includeDisabled = false;
	let includeHidden = true;

	function GenerateQuerySelector(element)
	{
		let idPart = "";
		const attribs = {};
		const tagName = element.tagName;

		if (element.id != "")
			idPart = `#${element.id}`;
		if (element.name)
			attribs["name"] = element.name;

		return tagName + Object.entries(attribs).map(([attribName, value]) => `[${attribName}="${CSS.escape(value)}"]`) + idPart;
	}

	function GetDataInputElements()
	{
		const formElements = [

		];

		let baseQuerySelection = ":not(data-refresh-nosave)"

		if (!includeDisabled)
			baseQuerySelection += ":not([disabled])"
		if (!includeHidden)
			baseQuerySelection += ":not([hidden])"

		const inputs = Array.from(document.querySelectorAll("input:not([type=file])" + baseQuerySelection));
		const textareas = Array.from(document.querySelectorAll("textarea" + baseQuerySelection));
		const selections = Array.from(document.querySelectorAll("select" + baseQuerySelection));

		const all = [
			...inputs.map(input =>
			{
				const type = input.getAttribute("type");

				return {
					querySelector: GenerateQuerySelector(input),
					value: type !== "radio" && type !== "checkbox" ? input.value : input.checked
				};
			}),
			...[...textareas, ...selections].map(input =>
			({
				querySelector: GenerateQuerySelector(input),
				value: input.value
			})
			)
		];

		all.forEach(element =>
		{
			const querySelector = element.querySelector;
			switch (document.querySelectorAll(querySelector).length)
			{
				case 0:
					console.warn(`malfunctional query selector "${querySelector}"}`);
					break;
				case 1:
					formElements.push(element);
					break;
				default:
					console.warn(`query selector not unique "${querySelector}"}`);
			}
		});

		return formElements;
	}

	function SetValue(element, value)
	{
		switch (element.tagName)
		{
			case "SELECT":
			case "TEXTAREA":
				element.value = value;
				break;
			case "INPUT":
				const type = element.getAttribute("type");
				if (type === "checkbox" || type === "radio")
					element.checked = value;
				else
					element.value = value;
				break;
		}
	}

	function Save(data)
	{
		const dataInputElements = GetDataInputElements();

		sessionStorage.setItem(SESSION_STORAGE_NAME, JSON.stringify(dataInputElements));

		if (!enabled)
			return;
	}

	function Load()
	{
		if (!enabled)
			return;

		const string = sessionStorage.getItem(SESSION_STORAGE_NAME);
		const json = JSON.parse(string);

		if (json)
		{
			json.forEach(entry =>
			{
				const querySelector = entry.querySelector;
				const value = entry.value;

				const applicableElements = document.querySelectorAll(querySelector);

				switch (applicableElements.length)
				{
					case 0:
						console.warn(`no such element found "${querySelector}"}`);
						break;
					case 1:
						SetValue(applicableElements[0], value);
						break;
					default:
						console.warn(`query selector not unique "${querySelector}"}`);

				}
			});
		}
	}

	function Clear()
	{
		sessionStorage.removeItem(SESSION_STORAGE_NAME);
	}

	return {
		get enabled() { return enabled; },
		set enabled(value) { enabled = value; },

		get includeDisabled() { return includeDisabled; },
		set includeDisabled(value) { includeDisabled = value; },
		
		get includeHidden() { return includeHidden; },
		set includeHidden(value) { includeHidden = value; },

		Save,
		Load,
		Clear,
	};
}

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

	console.log("Connecting to server for client events using Websocket.");

	var EventSocket = new WebSocket(Uri, ["ls"]);
	var PingTimer = null;

	EventSocket.onopen = async function ()
	{
		if (PageOutOfSync)
		{
			console.log("Websocket reopened. Reloading page to synchronize content.");

			await ClearCacheAsync(null);
			Reload(null);
			return;
		}

		console.log("Websocket opened. Registring Tab for client events: " + TabID);

		EventSocket.send(JSON.stringify({
			"cmd": "Register",
			"tabId": TabID,
			"location": window.location.href
		}));

		PingTimer = window.setInterval(function ()
		{
			if (EventSocket && EventSocket.readyState === WebSocket.OPEN)
			{
				console.log("Ping");

				EventSocket.send(JSON.stringify({
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

				if (EventCheckingEnabled)
				{
					PageOutOfSync = true;
					CheckEventsWS(TabID);
				}
			}
		}, 10000);
	};

	window.onbeforeunload = function ()
	{
		console.log("Unloading page.");

		EventCheckingEnabled = false;

		if (PingTimer)
		{
			window.clearInterval(PingTimer);
			PingTimer = null;
		}

		if (EventSocket && EventSocket.readyState === WebSocket.OPEN)
		{
			console.log("Unregistering Tab.");

			EventSocket.send(JSON.stringify({
				"cmd": "Unregister"
			}));

			EventSocket.close(1000, "Page closed.");
			EventSocket = null;
		}
	};

	EventSocket.onmessage = async function (event)
	{
		if (PageOutOfSync)
		{
			console.log("Reloading page to synchronize content.");

			await ClearCacheAsync(null);
			Reload(null);
			return;
		}

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

	EventSocket.onerror = function ()
	{
		console.log("Connection error.");

		if (PingTimer)
		{
			window.clearInterval(PingTimer);
			PingTimer = null;
		}

		if (EventCheckingEnabled)
		{
			window.setTimeout(function ()
			{
				PageOutOfSync = true;
				CheckEventsWS(TabID);
			}, 5000);
		}
	};
}

function CheckEventsXHTTP(TabID)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = async function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				if (PageOutOfSync)
				{
					console.log("Reloading page to synchronize content.");

					await ClearCacheAsync(null);
					Reload(null);
					return;
				}

				var Events;
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
						console.log("Ping");

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
						console.log("Reconnecting");

						PageOutOfSync = true;
						xhttp.open("POST", "/ClientEvents", true);
						xhttp.setRequestHeader("Content-Type", "text/plain");
						xhttp.setRequestHeader("X-TabID", TabID);
						xhttp.send(window.location.href);
					}, 5000);
				}
			}
		};
	};

	EventCheckingEnabled = true;

	console.log("Connecting to server for client events using XML/HTTP-Request.");

	xhttp.open("POST", "/ClientEvents", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("X-TabID", TabID);
	xhttp.send(window.location.href);
}

function EvaluateEvent(Event)
{
	if (Event && Event.type.match(/^[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)*$/g))
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
	console.log("Stopping event reception.");
	EventCheckingEnabled = false;
}

function ShowError(xhttp)
{
	console.log("Error received: " + xhttp.responseText);

	if (xhttp.responseText.length > 0)
		Popup.Alert(xhttp.responseText);
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

async function ClearCacheAsync(Data)
{
	try
	{
		console.log("Clearing cache.");

		var Keys = await caches.keys();
		var Tasks = Keys.map(Key => caches.delete(Key));

		await Promise.all(Tasks);

		console.log("Cache has been cleared.");
	}
	catch (e)
	{
		console.log(e);
	}
}

function Reload(Data)
{
	ClearReloadTimer();
	FormDataStore.Save();
	window.location.reload(false);
}

function ClearReloadTimer()
{
	if (ReloadTimer)
	{
		window.clearTimeout(ReloadTimer);
		ReloadTimer = null;
	}
}

function SetReloadTimer()
{
	ClearReloadTimer();
	ReloadTimer = window.setTimeout(async function ()
	{
		await ClearCacheAsync();
		Reload(null);
	}, 3000);
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
	};

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
		};

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

async function CallServer(Resource, RequestPayload, Language)
{
	var Request = new Promise((SetResult, SetException) =>
	{
		var xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function ()
		{
			if (xhttp.readyState == 4)
			{
				var Response = xhttp.responseText;

				if (xhttp.status === 200)
				{
					Response = JSON.parse(Response);
					SetResult(Response);
				}
				else
				{
					var Error =
					{
						"message": Response,
						"statusCode": xhttp.status,
						"statusMessage": xhttp.statusText
					};

					SetException(Error);
				}

				delete xhttp;
			}
		};

		if (RequestPayload)
		{
			xhttp.open("POST", Resource, true);

			if (!(RequestPayload instanceof FormData))
				xhttp.setRequestHeader("Content-Type", "application/json");
		}
		else
			xhttp.open("GET", Resource, true);

		xhttp.setRequestHeader("Accept", "application/json");
		xhttp.setRequestHeader("X-TabID", TabID);

		if (Language)
			xhttp.setRequestHeader("Accept-Language", Language);

		if (RequestPayload instanceof FormData)
			xhttp.send(RequestPayload);
		else
			xhttp.send(JSON.stringify(RequestPayload));
	});

	return await Request;
}

var ContentQueue = null;
var TabID;
var ServerID = "";
var EventCheckingEnabled = true;
var PageOutOfSync = false;
var ReloadTimer = null;

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
const FormDataStore = FormDataStoreHandler();

window.addEventListener("load", () =>
{
	FormDataStore.Load();
	FormDataStore.Clear();
}); 