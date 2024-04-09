function CreateInnerHTML(ElementId, Args)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML(Args);
}

function CreateHTML(Args)
{
	var Segments = [
		"<tr>\r\n",
		"<td style=\"text-align:left",
		"\">",
		"No",
		"</td>\r\n",
		"<td style=\"text-align:center",
		"\">",
		"headers",
		"</td>\r\n",
		"<td style=\"text-align:left",
		"\">",
		"table",
		"</td>\r\n",
		"</tr>\r\n"];
	return Segments.join("");
}
