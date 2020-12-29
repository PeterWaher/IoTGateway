function OpenSniffer(Resource)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                if (Resource.indexOf("?") >= 0)
                    Resource += "&";
                else
                    Resource += "?";

                Resource += "SnifferId=" + encodeURI(xhttp.responseText);

                var Window = window.open(Resource, "_blank");
                Window.focus();
            }
            else
                ShowError(xhttp);
        }
    };

    xhttp.open("POST", "/Settings/RandomizePassword", true);
    xhttp.send("");
}

function Rx(Data)
{
    AddRow("rx", Data.timestamp, Data.message);
}

function Tx(Data)
{
    AddRow("tx", Data.timestamp, Data.message);
}

function Information(Data)
{
    AddRow("info", Data.timestamp, Data.message);
}

function Warning(Data)
{
    AddRow("warning", Data.timestamp, Data.message);
}

function Error(Data)
{
    AddRow("err", Data.timestamp, Data.message);
}

function Exception(Data)
{
    AddRow("ex", Data.timestamp, Data.message);
}

function AddRow(Type, Timestamp, Message)
{
    var TBody = document.getElementById("SnifferBody");

    var Tr = document.createElement("TR");
    Tr.setAttribute("class", Type);
    TBody.appendChild(Tr);

    var Td = document.createElement("TD");
    Td.innerText = Timestamp.substring(11, 19);
    Td.setAttribute("class", Type);
    Tr.appendChild(Td);

    Td = document.createElement("TD");
    Td.setAttribute("class", Type);
    Tr.appendChild(Td);

    Td = document.createElement("TD");
    Td.innerText = Message;
    Td.setAttribute("class", Type);
    Tr.appendChild(Td);
}