function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
{
	var Segments = [
		"<section",
		" style=\"-webkit-column-count:2;-moz-column-count:2;column-count:2\"",
		">\r\n",
		"<p>",
		"This is section 1.",
		"</p>\r\n",
		"<p>",
		"It is written in two columns.",
		"</p>\r\n",
		"</section>\r\n",
		"<section",
		">\r\n",
		"<p>",
		"This is section 2.",
		"</p>\r\n",
		"<p>",
		"It is written in one column.",
		"</p>\r\n",
		"</section>\r\n",
		"<section",
		" style=\"-webkit-column-count:3;-moz-column-count:3;column-count:3\"",
		">\r\n",
		"<p>",
		"This is section 3.",
		"</p>\r\n",
		"<p>",
		"It is written in three columns.",
		"</p>\r\n",
		"</section>\r\n"];
	return Segments.join("");
}
