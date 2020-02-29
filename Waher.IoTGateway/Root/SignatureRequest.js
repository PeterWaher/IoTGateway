function ContractAction(RequestId, Protect, Sign)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                var Signed = JSON.parse(xhttp.responseText);

                if (Signed)
                    window.alert("Contract successfully signed.");
                else
                    window.alert("Contract rejected. Request has been removed.");

                Ok();
            }
            else
            {
                ShowError(xhttp);

                if (Protect)
                    document.getElementById("Password").value = "";
            }
        }
    };

    var Req =
    {
        "requestId": RequestId,
        "sign": Sign,
        "protect": Protect
    };

    if (Protect)
        Req.password = document.getElementById("Password").value;

    xhttp.open("POST", "/Settings/ContractAction", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify(Req));
}