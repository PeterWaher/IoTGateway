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
		"Some",
		"<sub>",
		"subscript",
		"</sub>",
		"</p>\r\n",
		"<p>",
		"a",
		"<sub>",
		"i",
		"</sub>",
		"=A",
		"<sub>",
		"i,j",
		"</sub>",
		"</p>\r\n",
		"<p>",
		"It is important that sub",
		"<sub>",
		"indices",
		"</sub>",
		" are displayed correctly in flowing text.",
		"</p>\r\n"];
	return Segments.join("");
}
