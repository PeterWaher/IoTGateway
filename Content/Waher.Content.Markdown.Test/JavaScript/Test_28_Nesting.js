function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
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
