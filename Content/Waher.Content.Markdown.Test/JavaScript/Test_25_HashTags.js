function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
{
	var Segments = [
		"<h1",
		" id=\"header\"",
		">",
		"Header",
		"</h1>\r\n",
		"<p>",
		"This ",
		"<mark",
		">paragraph</mark>",
		" contains ",
		"<mark",
		">3</mark>",
		" ",
		"<mark",
		">hashtags</mark>",
		".",
		"</p>\r\n",
		"<p>",
		"<mark",
		">Hashtags</mark>",
		" at the beginning of a row works, as headers require space after # and the tag.",
		"</p>\r\n"];
	return Segments.join("");
}
