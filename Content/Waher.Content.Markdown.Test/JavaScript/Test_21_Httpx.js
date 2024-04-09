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
		"När Ossian kom på besök:",
		"</p>\r\n",
		"<figure><img src=\"/HttpxProxy/httpx://gw1@kode.im/Uploaded/2016/06/18/20160504_195607.jpg\" alt=\"20160504_195607.jpg\" class=\"aloneUnsized\"/><figcaption>20160504_195607.jpg</figcaption></figure>\r\n",
		"<figure><img src=\"/HttpxProxy/httpx://gw1@kode.im/Uploaded/2016/06/18/20160504_220012.jpg\" alt=\"20160504_220012.jpg\" class=\"aloneUnsized\"/><figcaption>20160504_220012.jpg</figcaption></figure>\r\n"];
	return Segments.join("");
}
