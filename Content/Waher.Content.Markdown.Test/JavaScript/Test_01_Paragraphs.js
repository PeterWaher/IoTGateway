function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML();
}

function CreateHTML()
{
	var Segments = [
		"<p>",
		"This is paragraph 1",
		"</p>\r\n",
		"<p>",
		"This is paragraph 2",
		"</p>\r\n",
		"<p>",
		"This is the third paragraph.",
		"</p>\r\n",
		"<p>",
		"In this fourth paragraph",
		"<br/>\r\n",
		"a line break is inserted.",
		"</p>\r\n",
		"<p>",
		"The fifth paragraph is separated from the fourth by a line containing only spaces.",
		"</p>\r\n"];
	return Segments.join("");
}
