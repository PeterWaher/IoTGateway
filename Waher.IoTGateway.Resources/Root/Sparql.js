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
	var xhttp = new XMLHttpRequest();
	var IsHtml = false;
	var IsBinary = false;
	var IsPng = false;
	var IsSvg = false;

	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState == 4)
		{
			var Result = document.getElementById("Result");

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

			Result.setAttribute("style", "display:block");

			delete xhttp;
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
			xhttp.setRequestHeader("Accept", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			xhttp.responseType = "blob";
			break;

		case "Dot":
			xhttp.setRequestHeader("Accept", "text/vnd.graphviz");
			break;

		case "Png":
			IsPng = true;
			xhttp.setRequestHeader("Accept", "image/png");
			xhttp.responseType = "blob";
			break;

		case "Svg":
			IsSvg = true;
			xhttp.setRequestHeader("Accept", "image/svg+xml");
			break;
	}

	xhttp.send(Form);
}
