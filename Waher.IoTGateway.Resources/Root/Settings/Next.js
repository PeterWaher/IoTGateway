function Next()
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status !== 200)
                ShowError(xhttp);
        }
    };

    xhttp.open("POST", "/Settings/ConfigComplete", true);
    xhttp.send("");
}
