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
		"Title of document: ",
		"Test_16_MetaData.md",
		"<br/>\r\n",
		"Subtitle of document: ",
		"Unit tests",
		"<br/>\r\n",
		"Author: ",
		"Peter Waher",
		"</p>\r\n"];
	return Segments.join("");
}
