var CallbackTimer = null;

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

    ClearTimer();

    xhttp.open("POST", "/Settings/Consent", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify({ "consent": Checked }));

    SetTimer();
}

function ClearTimer()
{
    if (CallbackTimer)
    {
        clearTimeout(CallbackTimer);
        CallbackTimer = null;
    }
}

function SetTimer()
{
    CallbackTimer = setTimeout(function ()
    {
        window.location.reload(false);
    }, 2000);
}

function ShowNext(Data)
{
    ClearTimer();
    ShowNext2(Data.consent);
}

function ShowNext2(Consent)
{
    document.getElementById("NextButton").style.display = Consent ? "inline-block" : "none";
}