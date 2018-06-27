function ConsentClicked()
{
    var CheckBox = document.getElementById("Consent");
    var Checked = CheckBox.checked;

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status !== 200)
                ShowError(xhttp);
        }
    };

    xhttp.open("POST", "/Settings/Consent", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify({ "consent": Checked }));
}

function ShowNext(Data)
{
    document.getElementById("NextButton").style.display = Data.consent ? "inline-block" : "none";
}