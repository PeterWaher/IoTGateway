function CheckEvents(TabID)
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
					window.location.reload(false);
					return;
				}

				var Events;
				var Event;
				var i, c;
				try
				{
					Events = JSON.parse(xhttp.responseText);
				}
				catch (Exception)
				{
					window.alert("Invalid JSON: " + xhttp.responseText);
					Events = null;
				}

				if (Events != null)
				{
					c = Events.length;
					for (i = 0; i < c; i++)
					{
						Event = Events[i];

						try
						{
							if (Event.type.match(/^[a-zA-Z0-9]+$/g))
							{
								try
								{
									eval(Event.type + "(Event.data)");
								}
								catch (Exception)
								{
									window.alert("Error occurred while processing event: " + Exception + "\n\nJSON: " + xhttp.responseText);
								}
							}
							else
								window.alert("Invalid event received: " + Event.type);
						}
						catch (Exception)
						{
							window.alert("Error occurred while processing event: " + Exception + "\n\nJSON: " + xhttp.responseText);
						}
					}
				}

				xhttp.open("POST", "/ClientEvents", true);
				xhttp.setRequestHeader("Content-Type", "text/plain");
				xhttp.setRequestHeader("X-TabID", TabID);
				xhttp.send(window.location.href);
			}
			else
			{
				ShowError(xhttp);

				window.setTimeout(function ()
				{
					NeedsReload = true;
					xhttp.open("POST", "/ClientEvents", true);
					xhttp.setRequestHeader("Content-Type", "text/plain");
					xhttp.setRequestHeader("X-TabID", TabID);
					xhttp.send(window.location.href);
				}, 5000);
			}
		};
	}

	xhttp.open("POST", "/ClientEvents", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("X-TabID", TabID);
	xhttp.send(window.location.href);
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
	// TODO: Reload, without using contents in forms.
}

var TabID = CreateGUID();
CheckEvents(TabID);
