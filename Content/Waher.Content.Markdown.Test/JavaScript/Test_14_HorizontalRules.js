function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
{
	var Segments = [
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n",
		"<hr/>\r\n"];
	return Segments.join("");
}
