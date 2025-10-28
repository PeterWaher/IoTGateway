function QueryKeyDown(Control, Event)
{
	if (Event.keyCode === 9)
	{
		var Value = Control.value;
		var Start = Control.selectionStart;
		var End = Control.selectionEnd;
		Control.value = Value.substring(0, Start) + '\t' + Value.substring(End);
		Control.selectionStart = Control.selectionEnd = Start + 1;
		return false;
	}
	else if (Event.keyCode === 13 && !Event.shiftKey)
	{
		ExecuteQuery();
		return false;
	}
}

function ClearAll()
{
	var Script = document.getElementById("script");
	var Div = document.getElementById("Results");
	Div.innerHTML = "";
	Script.value = "";
	Script.focus();
}

function AddDefaultGraph()
{
	AddGraph("defaultGraph", "default-graph-uri");
}

function AddNamedGraph()
{
	AddGraph("namedGraph", "named-graph-uri");
}

function AddGraph(Id, Name)
{
	var Nr = 1;
	var Suffix = "1";
	var LastInput;
	var Temp = document.getElementById(Id + Suffix);

	while (Temp)
	{
		LastInput = Temp;
		Nr++;
		Suffix = Nr.toString();
		Temp = document.getElementById(Id + Suffix);
	}

	Temp = document.createElement("INPUT");
	Temp.setAttribute("id", Id + Suffix);
	Temp.setAttribute("type", "text");
	Temp.setAttribute("name", Name);

	LastInput.parentNode.insertBefore(Temp, LastInput.nextSibling);
}

function ExecuteQuery()
{
	var Result = document.getElementById("Result");
	var xhttp = new XMLHttpRequest();
	var IsHtml = false;
	var IsBinary = false;
	var IsPng = false;
	var IsSvg = false;
	var IsBlob = false;

	xhttp.onreadystatechange = function ()
	{
		switch (xhttp.readyState)
		{
			case 2:
				Result.innerHTML = "<legend>Receiving</legend><p>Response is being received...</p>";
				break;

			case 4:
				switch (xhttp.status)
				{
					case 200:
						if (IsHtml)
							Result.innerHTML = "<legend>Result</legend>" + xhttp.responseText;
						else
						{
							Result.innerHTML = "<legend>Result</legend>";

							if (IsBinary)
							{
								var Blob = xhttp.response;
								var Url = window.URL.createObjectURL(Blob);

								var A = document.createElement("A");
								A.href = Url;
								A.download = "SPARQL Result Set.xlsx";

								Result.appendChild(A);
								A.click();
								Result.removeChild(A);

								var P = document.createElement("P");
								P.innerText = "The result has been downloaded.";
								Result.appendChild(P);

								window.URL.revokeObjectURL(Url);
							}
							else if (IsSvg)
							{
								var Div = document.createElement("DIV");
								Result.appendChild(Div);

								Div.innerHTML = xhttp.responseText;
							}
							else if (IsPng)
							{
								var Blob = xhttp.response;
								var Reader = new FileReader();

								Reader.onload = function (e)
								{
									var Img = document.createElement("IMG");
									Img.src = e.target.result;
									Img.alt = "SPARQL Result Image";

									Result.appendChild(Img);
								};

								Reader.readAsDataURL(Blob);
							}
							else
							{
								var Pre = document.createElement("PRE");
								Result.appendChild(Pre);

								var Code = document.createElement("CODE");
								Pre.appendChild(Code);

								Code.innerText = xhttp.responseText;
							}
						}
						break;

					default:
						var s = "<legend>Error " + xhttp.status + " - ";

						if (xhttp.statusText)
							s += xhttp.statusText;
						else
							s += GetStatusText(xhttp.status);

						s += "</legend>";

						if (IsBlob)
						{
							var Reader = new FileReader();

							Reader.onload = function (e)
							{
								s += "<pre><code class='nohighlight'>" + ExtractMessage(Reader.result) + "</code></pre>";
								Result.innerHTML = s;
							};

							Reader.readAsText(xhttp.response);
						}
						else
						{
							s += "<pre><code class='nohighlight'>" + ExtractMessage(xhttp.responseText) + "</code></pre>";
							Result.innerHTML = s;
						}
						break;
				}

				Result.setAttribute("style", "display:block");

				delete xhttp;
				break;
		}
	};

	var Form = "pretty=true&query=" + encodeURIComponent(document.getElementById("query").value);
	var Input;
	var Nr = 1;

	while (Input = document.getElementById("defaultGraph" + Nr))
	{
		Form += "&default-graph-uri=" + encodeURIComponent(Input.value);
		Nr++;
	}

	Nr = 1;

	while (Input = document.getElementById("namedGraph" + Nr))
	{
		Form += "&named-graph-uri=" + encodeURIComponent(Input.value);
		Nr++;
	}

	xhttp.open("POST", "/sparql", true);
	xhttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

	switch (document.getElementById("ReturnType").value)
	{
		case "Xml":
			xhttp.setRequestHeader("Accept", "application/sparql-results+xml, application/rdf+xml;q=0.9, text/xml;q=0.1");
			break;

		case "Json":
			xhttp.setRequestHeader("Accept", "application/sparql-results+json; application/json;q=0.1");
			break;

		case "Csv":
			xhttp.setRequestHeader("Accept", "text/csv, text/plain;q=0.2, text/*;q=0.1");
			break;

		case "Tsv":
			xhttp.setRequestHeader("Accept", "text/tab-separated-values, text/plain;q=0.2, text/*;q=0.1");
			break;

		case "Html":
			IsHtml = true;
			xhttp.setRequestHeader("Accept", "text/html, text/plain;q=0.2, text/*;q=0.1");
			break;

		case "Text":
			xhttp.setRequestHeader("Accept", "text/turtle, text/csv;q=0.9, text/tab-separated-values;q=0.9, text/plain;q=0.2, text/*;q=0.1");
			break;

		case "Xlsx":
			IsBinary = true;
			IsBlob = true;
			xhttp.setRequestHeader("Accept", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			xhttp.responseType = "blob";
			break;

		case "Dot":
			xhttp.setRequestHeader("Accept", "text/vnd.graphviz");
			break;

		case "Png":
			IsPng = true;
			IsBlob = true;
			xhttp.setRequestHeader("Accept", "image/png");
			xhttp.responseType = "blob";
			break;

		case "Svg":
			IsSvg = true;
			xhttp.setRequestHeader("Accept", "image/svg+xml");
			break;
	}

	Result.setAttribute("style", "display:block");
	Result.innerHTML = "<legend>Processing</legend><p>Query is being processed...</p>";

	xhttp.send(Form);
}

function ExtractMessage(Html)
{
	return GetCode(GetPre(GetBody(Html)));
}

function GetBody(Html)
{
	return GetElementContent(Html, "body")
}

function GetPre(Html)
{
	return GetElementContent(Html, "pre")
}

function GetCode(Html)
{
	return GetElementContent(Html, "code")
}

function GetElementContent(Html, TagName)
{
	if (!Html)
		return Html;

	var Lower = Html.toLowerCase();
	var StartTagIndex = Lower.indexOf("<" + TagName);
	if (StartTagIndex < 0)
		return Html;

	var StartTagClose = Lower.indexOf(">", StartTagIndex);
	if (StartTagClose < 0)
		return Html;

	var EndTagIndex = Lower.indexOf("</" + TagName, StartTagClose + 1);
	if (EndTagIndex < 0)
		return Html;

	return Html.substring(StartTagClose + 1, EndTagIndex);
}

function GetStatusText(status)
{
	switch (status)
	{
		case 100: return "Continue";
		case 101: return "Switching Protocols";
		case 102: return "Processing";
		case 103: return "Early Hints";

		case 200: return "OK";
		case 201: return "Created";
		case 202: return "Accepted";
		case 203: return "Non-Authoritative Information";
		case 204: return "No Content";
		case 205: return "Reset Content";
		case 206: return "Partial Content";
		case 207: return "Multi-Status";
		case 208: return "Already Reported";
		case 226: return "IM Used";

		case 300: return "Multiple Choices";
		case 301: return "Moved Permanently";
		case 302: return "Found";
		case 303: return "See Other";
		case 304: return "Not Modified";
		case 305: return "Use Proxy";
		case 307: return "Temporary Redirect";
		case 308: return "Permanent Redirect";

		case 400: return "Bad Request";
		case 401: return "Unauthorized";
		case 402: return "Payment Required";
		case 403: return "Forbidden";
		case 404: return "Not Found";
		case 405: return "Method Not Allowed";
		case 406: return "Not Acceptable";
		case 407: return "Proxy Authentication Required";
		case 408: return "Request Timeout";
		case 409: return "Conflict";
		case 410: return "Gone";
		case 411: return "Length Required";
		case 412: return "Precondition Failed";
		case 413: return "Content Too Large";
		case 414: return "URI Too Long";
		case 415: return "Unsupported Media Type";
		case 416: return "Range Not Satisfiable";
		case 417: return "Expectation Failed";
		case 418: return "I'm a Teapot";
		case 421: return "Misdirected Request";
		case 422: return "Unprocessable Content";
		case 423: return "Locked";
		case 424: return "Failed Dependency";
		case 425: return "Too Early";
		case 426: return "Upgrade Required";
		case 428: return "Precondition Required";
		case 429: return "Too Many Requests";
		case 431: return "Request Header Fields Too Large";
		case 451: return "Unavailable For Legal Reasons";

		case 500: return "Internal Server Error";
		case 501: return "Not Implemented";
		case 502: return "Bad Gateway";
		case 503: return "Service Unavailable";
		case 504: return "Gateway Timeout";
		case 505: return "HTTP Version Not Supported";
		case 506: return "Variant Also Negotiates";
		case 507: return "Insufficient Storage";
		case 508: return "Loop Detected";
		case 510: return "Not Extended";
		case 511: return "Network Authentication Required";

		default: return "Unknown Status";
	}
}
