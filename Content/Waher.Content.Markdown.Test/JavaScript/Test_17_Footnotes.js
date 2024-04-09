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
		"Here is some text containing a footnote",
		"<sup id=\"fnref-1\"><a href=\"#fn-1\" class=\"footnote-ref\">1</a></sup>",
		". You can then continue your thought",
		"&hellip;",
		"</p>\r\n",
		"<p>",
		"Even go to a new paragraph and the footnotes with go to the bottom of the document",
		"<sup id=\"fnref-2\"><a href=\"#fn-2\" class=\"footnote-ref\">2</a></sup>",
		".",
		"</p>\r\n",
		"<p>",
		"This is an example of an inline footnote.",
		"<sup id=\"fnref-3\"><a href=\"#fn-3\" class=\"footnote-ref\">3</a></sup>",
		"</p>\r\n",
		"<div class=\"footnotes\">\r\n",
		"<hr />\r\n",
		"<ol>\r\n",
		"<li id=\"fn-1\">",
		"<p>",
		"Here is the text of the footnote itself.",
		"<a href=\"#fnref-1\" class=\"footnote-backref\">&#8617;</a>",
		"</p>\r\n",
		"</li>\r\n",
		"<li id=\"fn-2\">",
		"<p>",
		"Depending on the final form of your document, of course. See the documentation and experiment.",
		"</p>\r\n",
		"<p>",
		"This footnote has a second paragraph.",
		"<a href=\"#fnref-2\" class=\"footnote-backref\">&#8617;</a>",
		"</p>\r\n",
		"</li>\r\n",
		"<li id=\"fn-3\">",
		"<p>",
		"This is the ",
		"<em>",
		"actual",
		"</em>",
		" footnote.",
		"<a href=\"#fnref-3\" class=\"footnote-backref\">&#8617;</a>",
		"</p>\r\n",
		"</li>\r\n",
		"</ol>\r\n",
		"</div>\r\n"];
	return Segments.join("");
}
