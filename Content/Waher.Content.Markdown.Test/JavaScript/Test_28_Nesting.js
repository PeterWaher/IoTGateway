function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHtml();
}

function CreateHTML()
{
	var Segments = [
		"<p>",
		"Some ",
		"<strong>",
		"bold text with ",
		"<em>",
		"italics",
		"</em>",
		"</strong>",
		"</p>\r\n"];
	return Segments.join("");
}
