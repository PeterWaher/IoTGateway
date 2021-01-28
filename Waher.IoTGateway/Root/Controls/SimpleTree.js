function ExpandNode(Event, Node)
{
	Event.stopPropagation();

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var Response = JSON.parse(xhttp.responseText);
				var i, c = Response.length;

				if (!(Response instanceof Array))
				{
					window.alert("Expected response to be a JSON array.");
					return;
				}

				Node.className = "Collapsable";
				Node.setAttribute("onclick", "CollapseNode(event,this)");

				var Loop = Node.firstChild;
				while (Loop)
				{
					if (Loop.tagName == "SPAN")
					{
						Loop.innerHTML = Node.getAttribute("data-expandedimg");
						break;
					}
					else
						Loop = Loop.nextSibling;
				}

				if (Response[0] instanceof Array)
				{
					var Table = document.createElement("TABLE");
					Node.appendChild(Table);
					Table.setAttribute("onclick", "NoOperation(event,this)");

					var THead = document.createElement("THEAD");
					Table.appendChild(THead);

					var TBody = document.createElement("TBODY");
					Table.appendChild(TBody);

					var j, d;

					for (i = 0; i < c; i++)
					{
						var Rec = Response[i];
						var Tr = document.createElement("TR");

						if (i === 0)
							THead.appendChild(Tr);
						else
							TBody.appendChild(Tr);

						d = Rec.length;
						for (j = 0; j < d; j++)
						{
							var Td = document.createElement(i === 0 ? "TH" : "TD");
							Tr.appendChild(Td);

							Td.innerHTML = Rec[j];
						}
					}
				}
				else
				{
					var Ul = document.createElement("UL");
					Ul.className = "SimpleTree";
					Node.appendChild(Ul);

					for (i = 0; i < c; i++)
					{
						var Rec = Response[i];
						var Li = document.createElement("LI");

						Li.innerHTML = Rec.html;
						Li.setAttribute("data-id", Rec.id);

						if (Rec.expand)
						{
							Li.className = "Expandable";
							Li.setAttribute("data-expand", Rec.expand);
							Li.setAttribute("onclick", "ExpandNode(event,this)");
						}
						else
						{
							Li.className = "Leaf";
							Li.setAttribute("onclick", "NoOperation(event,this)");
						}

						if (Rec.expandedImg)
							Li.setAttribute("data-expandedimg", Rec.expandedImg);

						if (Rec.collapsedImg)
						{
							Li.setAttribute("data-collapsedimg", Rec.collapsedImg);

							var Span = document.createElement("SPAN");
							Span.className = "ItemImage";
							Span.innerHTML = Rec.collapsedImg;

							Li.insertBefore(Span, Li.firstChild);
						}

						Ul.appendChild(Li);
					}
				}
			}
			else
				window.alert(xhttp.responseText);
		}
	};

	xhttp.open("POST", Node.getAttribute("data-expand"), true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.setRequestHeader("Accept", "application/json");
	xhttp.send(JSON.stringify({
		"id": Node.getAttribute("data-id")
	}));
}

function CollapseNode(Event, Node)
{
	Event.stopPropagation();
	Node.className = "Expandable";
	Node.setAttribute("onclick", "ExpandNode(event,this)");

	var Loop = Node.firstChild;
	while (Loop)
	{
		if (Loop.tagName === "SPAN")
			Loop.innerHTML = Node.getAttribute("data-collapsedimg");
		else if (Loop.tagName === "UL" || Loop.tagName === "TABLE")
		{
			Node.removeChild(Loop);
			break;
		}

		Loop = Loop.nextSibling;
	}
}

function NoOperation(Event, Node)
{
	Event.stopPropagation();
}