var EditTimer = null;

function DotKeyDown(Control, Event)
{
	if (EditTimer)
		window.clearTimeout(EditTimer);

	EditTimer = window.setTimeout(UpdateGraph, 1000);
}

function UpdateGraph()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			var GraphDiv = document.getElementById("GraphDiv");
			GraphDiv.innerHTML = xhttp.responseText;
		};
	}

	xhttp.open("POST", "GraphVizLabGraph.md", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", "text/html");
	xhttp.send(document.getElementById("Dot").value);
}

function ShowExample(Name)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			document.getElementById("Dot").value = xhttp.responseText;
			UpdateGraph();
		}
	}

	xhttp.open("GET", "Examples/"+Name, true);
	xhttp.send();
}