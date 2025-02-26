
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
			var Output = document.getElementById("Output");
			Output.innerText = xhttp.responseText;
		};
	}

	var Suffix = "Html";
	var Type = "text/html";
	var Loop = document.getElementById("HtmlSection");
	Loop = Loop.firstElementChild;

	while (Loop)
	{
		if (Loop.tagName == "BUTTON" && Loop.className === "posButtonPressed")
		{
			Suffix = Loop.getAttribute("data-suffix");
			Type = Loop.getAttribute("data-type");
			break;
		}

		Loop = Loop.nextSibling;
	}

	xhttp.open("POST", "MarkdownLab" + Suffix + ".ws", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("Accept", Type);
	xhttp.send(document.getElementById("LabMarkdownEditorInput").value);
}

window.addEventListener("load", () =>
{
	let updated;

	setInterval(() =>
	{
		if (updated)
		{
			updated = false;
			UpdateHtml()
		}
	}, 1000)

	document.getElementById("LabMarkdownEditorInput").addEventListener("input", () => updated = true)
})