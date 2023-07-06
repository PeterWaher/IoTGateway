function LoadMore(Button, Offset, MaxCount, LegalDomain)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			Button.removeAttribute("data-scroll");

			if (xhttp.status === 200)
			{
				var Response = JSON.parse(xhttp.responseText);
				var i, c = Response.length;

				var Loop = Button.parentNode.firstChild;
				while (Loop.tagName !== "TABLE")
					Loop = Loop.nextSibling;

				var Table = Loop;
				Loop = Loop.firstChild;
				while (Loop)
				{
					if (Loop.tagName == "TBODY")
					{
						Table = Loop;
						Loop = Loop.firstChild;
					}
					else
						Loop = Loop.nextSibling;
				}

				for (i = 0; i < c; i++)
				{
					var Ref = Response[i];

					var Tr = document.createElement("TR");
					Table.appendChild(Tr);

					var Td = document.createElement("TD");
					Tr.appendChild(Td);
					Td.setAttribute("style", "text-align:left");
					Td.innerHTML = "<a href=\"/rdf-graph-store?graph=" + UrlEncode(Ref.graphUri) +
						"\" target=\"_blank\"><code>" + Ref.graphUri + "</code></a>";

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:left");
					Tr.appendChild(Td);
					Td.innerText = Ref.creators;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Ref.createdDate;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Ref.createdTime;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Ref.updatedDate;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Ref.updatedTime;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:right");
					Tr.appendChild(Td);
					Td.innerText = Ref.nrFiles;

					Td = document.createElement("TD");
					Td.setAttribute("style", "text-align:center");
					Tr.appendChild(Td);

					if (Ref.canDelete)
					{
						var Button = document.createElement("BUTTON");
						Button.setAttribute("type", "button");
						Button.setAttribute("class", "negButtonSm");
						Button.setAttribute("onclick", "DeleteGraph(this,'" + Ref.graphUri + "')");
						Button.innerText = "Delete";
						Td.appendChild(Button);
					}
				}

				if (c < MaxCount)
					Button.parentNode.removeChild(Button);
				else
					Button.setAttribute("onclick", "LoadMore(this," + (Offset + MaxCount) + "," + MaxCount + ")");
			}
		}
	};

	Button.setAttribute("data-scroll", "x");
	xhttp.open("POST", "GraphStore.ws", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.setRequestHeader("Accept", "application/json");
	xhttp.send(JSON.stringify(
		{
			"offset": Offset,
			"maxCount": MaxCount
		}
	));
}

function UploadFile()
{
	var Method = document.getElementById("Method").value;
	if (!Method)
	{
		window.alert("Upload method not selected.");
		return;
	}

	var GraphUri = document.getElementById("GraphUri").value;
	if (!GraphUri)
	{
		window.alert("Graph URI cannot be empty.");
		return;
	}

	try
	{
		new URL(GraphUri);
	}
	catch (e)
	{
		window.alert("Graph URI must be a URI: " + e);
		return;
	}

	var ContractFileSelector = document.getElementById("GraphFile");
	var Files = ContractFileSelector.files;

	if (Files.length !== 1)
	{
		window.alert("Select one graph file to upload.");
		return;
	}

	var File = Files[0];
	var Reader = new FileReader();

	Reader.addEventListener('load', (event) =>
	{
		var xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function ()
		{
			if (xhttp.readyState === 4)
			{
				if (xhttp.status >= 200 && xhttp.status < 300)
					window.location.reload(false);
				else
					window.alert(xhttp.responseText);
			}
		};

		xhttp.open(Method, "/rdf-graph-store?graph=" + encodeURIComponent(GraphUri), true);
		xhttp.setRequestHeader("Content-Type", "text/plain");
		xhttp.send(Reader.result);
	});

	Reader.readAsText(File);
}

function DeleteGraph(Control,GraphUri)
{
	if (!window.confirm("Are you sure you want to delete the graph <" + GraphUri + ">?"))
		return;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status >= 200 && xhttp.status < 300)
			{
				try
				{
					var Td = Control.parentNode;
					var Tr = Td.parentNode;
					Tr.parentNode.removeChild(Tr);
				}
				catch
				{
					window.location.reload(false);
				}
			}
			else
				window.alert(xhttp.responseText);
		}
	};

	xhttp.open("DELETE", "/rdf-graph-store?graph=" + encodeURIComponent(GraphUri), true);
	xhttp.send("");
}

window.onscroll = function ()
{
	var Button = document.getElementById("LoadMoreButton");

	if (Button)
	{
		var Rect = Button.getBoundingClientRect();
		if (Rect.top <= window.innerHeight * 2)
		{
			var Scroll = Button.getAttribute("data-scroll");
			if (Scroll !== "x")
				Button.click();
		}
	}
}
