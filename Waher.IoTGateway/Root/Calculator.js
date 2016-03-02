function EvaluateExpression()
{
	var Script = document.getElementById("script");
	var Div = document.getElementById("Results");

	var Code = document.createElement("code");
	var TextNode = document.createTextNode(Script.value);
	var s = TextNode.nodeValue;
	s = s.replace(/(?:\r\n|\r|\n)/g, "<br/>").replace(' ', '&nbsp;').replace('\t', '&nbsp;&nbsp;&nbsp;');
	Code.innerHTML = s;
	var P = document.createElement('p');
	P.appendChild(Code);
	var Div2 = document.createElement('div');
	Div2.appendChild(P);
	Div2.setAttribute("class", "clickable");
	Div2.setAttribute("onclick", "SetScript(\"" + Script.value.replace('"', '\\"').replace('\'', '\\\'').replace('\r', '\\r').replace('\n', '\\n').replace('\t', '\\t') + "\");");

	Div.insertBefore(Div2, Div.firstChild);

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function() 
	{
		if (xhttp.readyState == 4 && xhttp.status == 200)
			Div.innerHTML = xhttp.responseText + Div.innerHTML;
	};

	xhttp.open("POST", "/Evaluate", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(Script.value);
	Script.value = "";
	Script.focus();
}

function SetScript(Text)
{
	var Script = document.getElementById("script");
	Script.value = Text;
	Script.select();
	Script.focus();
}

function ListVariables()
{
	var Script = document.getElementById("script");
	/*Script.value = "Request.Session.Push();\r\ntry\r\n\tforeach v in Request.Session : println(v);\r\n\finally\r\n\tRequest.Session.Pop();"; TODO: Replace to this version when member method calls are available. */
	Script.value = "foreach v in Request.Session : println(v);";
	EvaluateExpression();
}

function ClearAll()
{
	var Script = document.getElementById("script");
	var Div = document.getElementById("Results");
	Div.innerHTML = "";
	Script.value = "";
	Script.focus();
}

function ScriptKeyDown(Control,Event)
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
	else if (Event.keyCode == 13 && !Event.shiftKey)
	{
		EvaluateExpression();
		return false;
	}
}
