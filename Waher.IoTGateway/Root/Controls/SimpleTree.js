async function SelectNode(Event, Node)
{
	try
	{
		if (Event)
			Event.stopPropagation();

		var ListView = Node.hasAttribute("data-listview") ? Node.getAttribute("data-listview") : null;
		var Response = await CallServer(Node.getAttribute("data-select"), {
			"id": Node.getAttribute("data-id")
		});
		
		if (ListView && Response.list) 
		{
			document.getElementById(ListView).innerHTML = Response.list;
			Response = Response.items;
		}
	}
	catch (e) 
	{
		console.log(e);
	}
}

async function SelectNodeLabel(Event, NodeLabel)
{
	SelectNode(Event, NodeLabel.parentElement);
}

async function ExpandNode(Event, Node)
{
	try
	{
		if (Event)
			Event.stopPropagation();

		var ListView = Node.hasAttribute("data-listview") ? Node.getAttribute("data-listview") : null;
		var IdAsDataId = Node.getAttribute("id") === Node.getAttribute("data-id");

		var Response = await CallServer(Node.getAttribute("data-expand"), {
			"id": Node.getAttribute("data-id")
		});

		if (ListView && Response.list && Response.items)
		{
			document.getElementById(ListView).innerHTML = Response.list;
			Response = Response.items;
		}

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

				if (IdAsDataId)
					Li.setAttribute("id", Rec.id);

				if (ListView)
					Li.setAttribute("data-listview", ListView);

				if (Rec.expand)
				{
					Li.className = "Expandable";
					Li.setAttribute("data-expand", Rec.expand);
					Li.setAttribute("onclick", "ExpandNode(event,this)");

					if (Rec.select)
						Li.setAttribute("data-select", Rec.select);
				}
				else if (Rec.select) 
				{
					Li.className = "Selectable";
					Li.setAttribute("data-select", Rec.select);
					Li.setAttribute("onclick", "SelectNode(event,this)");
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

					Loop = Li.firstElementChild;
					while (Loop)
					{
						Loop.setAttribute("onclick", "SelectNodeLabel(event,this)");
						Loop = Loop.nextElementSibling;
					}
				}

				Ul.appendChild(Li);
			}
		}
	}
	catch (e)
	{
		console.log(e);
	}
}

async function CollapseNode(Event, Node)
{
	if (Event)
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

	await SelectNode(Event, Node);
}

function NoOperation(Event, Node)
{
	Event.stopPropagation();
}