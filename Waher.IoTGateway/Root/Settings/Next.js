function Next()
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

    xhttp.open("POST", "/Settings/ConfigComplete", true);
    xhttp.setRequestHeader("Connection", "close");
    xhttp.send("");
}

function Ok()
{
    window.history.back();
}