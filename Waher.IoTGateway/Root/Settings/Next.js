function Next()
{
    ConfigComplete("ConfigComplete");
}

function ConfigComplete(Command)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status !== 200)
                ShowError(xhttp);

            window.setTimeout(function ()
            {
                NeedsReload = true;
                Reload(null);
            }, 1000);
        }
    };

    CloseEvents();

    xhttp.open("POST", "/Settings/" + Command, true);
    xhttp.setRequestHeader("Connection", "close");
    xhttp.send("");
}

function Ok()
{
    if (window.history.length === 1)
        window.close();
    else
        window.history.back();
}