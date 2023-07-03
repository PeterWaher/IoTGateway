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
		var Form = document.getElementById("QueryForm");
		Form.submit();
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
