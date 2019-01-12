function TestAddresses()
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status !== 200)
                ShowError(xhttp);
            else
            {
                if (xhttp.responseText === "1")
                    document.getElementById("NextMessage").style.display = "block";
                else
                    document.getElementById("TestError").style.display = "block";
            }
        }
    };

    document.getElementById("TestError").style.display = "none";
    document.getElementById("NextMessage").style.display = "none";

    xhttp.open("POST", "/Settings/TestNotificationAddresses", true);
    xhttp.setRequestHeader("Content-Type", "text/plain");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(document.getElementById("NotificationAddresses").value);
}
