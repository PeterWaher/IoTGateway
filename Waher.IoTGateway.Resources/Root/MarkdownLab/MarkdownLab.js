var EditTimer = null;

function MarkdownKeyDown(Control, Event)
{
	if (EditTimer)
		window.clearTimeout(EditTimer);

	EditTimer = window.setTimeout(UpdateHtml, 500);
}

function FormatButtonClicked(Button)
{
	var Loop = document.getElementById("HtmlSection");
	Loop = Loop.firstElementChild;

	while (Loop)
	{
		if (Loop.tagName == "BUTTON")
			Loop.className = "posButton";

		Loop = Loop.nextSibling;
	}

	Button.className = "posButtonPressed";

	UpdateHtml();
}

function UpdateHtml()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			var HtmlDiv = document.getElementById("HtmlDiv");
			SetDynamicHtml(HtmlDiv, xhttp.responseText);
		};
	}

	var Suffix = "Html";
	var Loop = document.getElementById("HtmlSection");
	Loop = Loop.firstElementChild;

	while (Loop)
	{
		if (Loop.tagName == "BUTTON" && Loop.className === "posButtonPressed")
		{
			Suffix = Loop.getAttribute("data-suffix");
			break;
        }

		Loop = Loop.nextSibling;
	}

	xhttp.open("POST", "MarkdownLab" + Suffix + ".md", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", "text/html");
	xhttp.send(document.getElementById("Markdown").value);
}
