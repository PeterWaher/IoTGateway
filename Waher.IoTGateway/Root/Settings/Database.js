function SelectDatabase(Control)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                Response = JSON.parse(xhttp.responseText);

                document.getElementById("PluginSettings").innerHTML = Response.html;
                document.getElementById("TestButton").style.display = Response.hasSettings ? "inline" : "none";
                document.getElementById("OkButton").style.display = Response.isDone ? "inline" : "none";
                document.getElementById("Ok").style.display = Response.isDone ? "inline" : "none";
                document.getElementById("Restart").style.display = Response.isDone && Response.restart ? "inline" : "none";
            }
            else
                ShowError(xhttp);
        }
    };

    document.getElementById("Ok").style.display = "none";
    document.getElementById("Fail").style.display = "none";
    document.getElementById("Restart").style.display = "none";

    xhttp.open("POST", "/Settings/SelectDatabase", true);
    xhttp.setRequestHeader("Content-Type", "text/plain");
    xhttp.send(Control.value);
}

function TestSettings(Save)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                document.getElementById("OkButton").style.display = "inline";
                document.getElementById("Ok").style.display = "inline";
                document.getElementById("Fail").style.display = "none";
                document.getElementById("Restart").style.display = xhttp.responseText === "2" ? "inline" : "none";

                if (Save)
                    Next();
            }
            else
            {
                document.getElementById("Ok").style.display = "none";
                document.getElementById("Fail").style.display = "inline";
                document.getElementById("Restart").style.display = "none";

                ShowError(xhttp);
            }
        }
    };

    var Request =
    {
        "save": Save
    };

    var Elements = document.getElementById("SettingsForm").elements;
    var i, c = Elements.length;

    for (i = 0; i < c; i++)
    {
        var element = Elements[i];

        if (element.tagName === "INPUT")
        {
            switch (element.type)
            {
                case "checkbox":
                    Request[element.name] = element.checked;
                    break;

                case "radio":
                    if (element.checked)
                        Request[element.name] = element.value;
                    break;

                default:
                    Request[element.name] = element.value;
                    break;
            }
        }
    }

    document.getElementById("Ok").style.display = "none";
    document.getElementById("Fail").style.display = "none";
    document.getElementById("Restart").style.display = "none";

    xhttp.open("POST", "/Settings/TestDatabase", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify(Request));
}