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
		"<a href=\"http://example.com/",
		"\" target=\"_blank",
		"\">http://example.com/</a>",
		"</p>\r\n",
		"<p>",
		"<a href=\"&#x6D;&#x61;&#x69;&#x6C;&#x74;&#x6F;&#x3A;&#x61;&#x64;&#x64;&#x72;&#x65;&#x73;&#x73;&#x40;&#x65;&#x78;&#x61;&#x6D;&#x70;&#x6C;&#x65;&#x2E;&#x63;&#x6F;&#x6D;\">&#x61;&#x64;&#x64;&#x72;&#x65;&#x73;&#x73;&#x40;&#x65;&#x78;&#x61;&#x6D;&#x70;&#x6C;&#x65;&#x2E;&#x63;&#x6F;&#x6D;</a>",
		"</p>\r\n",
		"<table>",
		"\r\n    ",
		"<tr>",
		"\r\n        ",
		"<td>",
		"Foo",
		"</td>",
		"\r\n    ",
		"</tr>",
		"\r\n",
		"</table>",
		"\r\n",
		"<p>",
		"This is an example of ",
		"<b>",
		"bold text",
		"</b>",
		".",
		"</p>\r\n",
		"<p>",
		"&copy;",
		" Waher Data AB 2016-2025. All rights reserved.",
		"</p>\r\n",
		"<p>",
		"AT",
		"&amp;T",
		"</p>\r\n",
		"<p>",
		"4 &lt; 5",
		"</p>\r\n",
		"<p>",
		"&#124;",
		"</p>\r\n",
		"<p>",
		"&#124;",
		"</p>\r\n",
		"<span class=\"test\">",
		"This is a test in a SPAN tag.",
		"</span>",
		"\r\n",
		"<p>",
		"User Name:",
		"<br/>\r\n",
		"<input id=\"UserName\" name=\"UserName\" type=\"text\" autofocus=\"autofocus\" style=\"width:20em\" />",
		"</p>\r\n",
		"<textarea id=\"command\" autofocus=\"autofocus\" wrap=\"hard\" onkeydown=\"return CommandKeyDown(this,event);\">",
		"Command",
		"</textarea>",
		"\r\n"];
	return Segments.join("");
}
