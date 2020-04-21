function EvaluateExpression()
{
	function Segment()
	{
		return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
	}

	var Tag = Segment() + Segment() + '-' + Segment() + '-' + Segment() + '-' + Segment() + '-' + Segment() + Segment() + Segment();

	var Script = document.getElementById("script");
	var Expression = Script.value;
	if (Expression === "")
		return;

	Expressions[Tag] = Expression;

	var Div = document.getElementById("Results");
	var Code = document.createElement("code");
	Code.innerHTML = Expression.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/(?:\r\n|\r|\n)/g, "<br/>").replace(/ /g, '&nbsp;').replace(/\t/g, '&nbsp;&nbsp;&nbsp;');
	var P = document.createElement('p');
	P.appendChild(Code);
	var Div2 = document.createElement('div');
	Div2.appendChild(P);
	Div2.setAttribute("class", "clickable");
	Div2.setAttribute("data-tag", Tag);
	Div2.setAttribute("onclick", "SetScript(this);");

	Div.insertBefore(Div2, Div.firstChild);

	var ResultDiv = document.createElement('div');
	Div.insertBefore(ResultDiv, Div.firstChild);

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			ResultDiv.innerHTML = xhttp.responseText;

			if (xhttp.getResponseHeader("X-More") === "1")
			{
				xhttp.open("POST", "/Evaluate", true);
				xhttp.setRequestHeader("Content-Type", "text/plain");
				xhttp.setRequestHeader("X-TAG", Tag);
				xhttp.send("");
			}
		};
	}

	xhttp.open("POST", "/Evaluate", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("X-TAG", Tag);
	xhttp.send(Expression);

	Script.value = "";
	Script.focus();
}

function SetScript(Div)
{
	var Script = document.getElementById("script");
	var Loop = Div.firstElementChild;
	var Expression = "";

	while (Loop !== null)
	{
		if (Loop.tagName === "CODE")
		{
			Expression = Loop.innerText;
			break;
		}
		else
			Loop = Loop.nextElementSibling;
	}

	if (Expression === "")
	{
		var Tag = Div.getAttribute("data-tag");
		Expression = Expressions[Tag];
	}

	Script.value = Expression;
	Script.select();
	Script.focus();
	document.body.scrollTop = document.documentElement.scrollTop = 0;
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
	else if (Event.keyCode === 13 && !Event.shiftKey)
	{
		EvaluateExpression();
		return false;
	}
}

function GraphClicked(Image, Event, Tag)
{
	var x, y;

	if (Event.offsetX)
	{
		x = Event.offsetX;
		y = Event.offsetY;
	}
	else
	{
		x = Event.pageX - Image.offsetLeft;
		y = Event.pageY - Image.offsetTop;
	}

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4 && xhttp.status === 200)
		{
			var Script = document.getElementById("script");

			Script.value = xhttp.responseText;
			EvaluateExpression();
		};
	}

	xhttp.open("POST", "/Evaluate", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.setRequestHeader("X-TAG", Tag);
	xhttp.setRequestHeader("X-X", x);
	xhttp.setRequestHeader("X-Y", y);
	xhttp.send("");
}

var Expressions = {};