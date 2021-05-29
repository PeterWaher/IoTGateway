var EditTimer = null;

function MarkdownKeyDown(Control, Event)
{
	if (EditTimer)
		window.clearTimeout(EditTimer);

	EditTimer = window.setTimeout(UpdateHtml, 500);
}

function UpdateHtml()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			var HtmlDiv = document.getElementById("HtmlDiv");
			HtmlDiv.innerHTML = xhttp.responseText;
		};
	}

	xhttp.open("POST", "MarkdownLabHtml.md", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", "text/html");
	xhttp.send(document.getElementById("Markdown").value);
}
