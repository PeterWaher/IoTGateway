function EvaluateExpression(Script)
{
	var Div = document.getElementById("Results");

	var P = document.createElement('p');
	P.appendChild(document.createTextNode(script.value));
	Div.insertBefore(P, Div.firstChild);

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function() 
	{
		if (xhttp.readyState == 4 && xhttp.status == 200)
		{
			P = document.createElement('p');
			P.appendChild(document.createTextNode(xhttp.responseText));
			Div.insertBefore(P, Div.firstChild);
		}
	};

	xhttp.open("POST", "/Evaluate", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(script.value);
	script.value = "";
}
