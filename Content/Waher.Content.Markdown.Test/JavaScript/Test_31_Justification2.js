function CreateInnerHTML(ElementId)
{
	var Element = document.getElementById(ElementId);
	if (Element)
		Element.innerHTML = CreateHTML();
}

function CreateHTML()
{
	var Segments = [
		"<div class='horizontalAlignMargins'>\r\n",
		"<ol>\r\n",
		"<li>",
		"<p>",
		"<strong>",
		"DEFINED TERMS ",
		"</strong>",
		"</p>\r\n",
		"<p>",
		"In these Terms the following words have the following meanings: ",
		"</p>\r\n",
		"<ol>\r\n",
		"<li>",
		"<p>",
		"\"",
		"<strong>",
		"Applicable Law",
		"</strong>",
		"\" means all laws, regulations, rules and regulatory guidance applicable to the operation of the Techical Operator’s Site.",
		"</p>\r\n",
		"</li>\r\n",
		"<li>",
		"<p>",
		"<strong>",
		"“A Beneficiary”",
		"</strong>",
		" means  a third party named in this agreement who will receive funds for the defined project/initiative in accordance to this this agreement and who agrees to use such funds solely for the purpose of the said project/initiative.",
		"</p>\r\n",
		"</li>\r\n",
		"</ol>\r\n",
		"</li>\r\n",
		"</ol>\r\n",
		"</div>\r\n"];
	return Segments.join("");
}
