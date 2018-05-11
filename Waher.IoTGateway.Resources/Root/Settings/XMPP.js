function SelectServer(Host)
{
    SetInputValue("XmppServer", Host);
    SetInputValue("Port", 5222);
    SetInputValue("BoshUrl", "");
}

function ConnectToHost()
{
    var Host = GetInputValue("XmppServer");
    if (Host == "")
    {
        window.alert("You must select an XMPP server.");
        return;
    }

    var Transport = GetInputValue("Transport");
    var Port = parseInt(GetInputValue("Port"));
    var BoshUrl = GetInputValue("BoshUrl");

    switch (Transport)
    {
        case "C2S":
            if (Port < 1 || Port > 65536)
            {
                window.alert("Invalid port number.");
                return;
            }
            break;

        case "BOSH":
            if (BoshUrl == "")
            {
                window.alert("You must provide a URL to connect to.");
                return;
            }
            break;
    }

    var Account = GetInputValue("Account");
    var Password = GetInputValue("Password");
    var CreateAccount = GetInputChecked("CreateAccount");
    var Password2 = GetInputValue("Password2");

    if (CreateAccount && Password != Password2)
    {
        window.alert("Passwords do not match.");
        return;
    }

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState == 4)
        {
            if (xhttp.status != 200)
                ShowError(xhttp);
        };
    }

    document.getElementById("Status").innerHTML = "";
    document.getElementById("ConnectionStatus").style.display = 'block';
    document.getElementById("Success0").style.display = 'none';

    xhttp.open("POST", "ConnectToHost", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify({
        "host": Host,
        "transport": Transport,
        "port": Port,
        "boshUrl": BoshUrl,
        "account": Account,
        "password": Password,
        "createAccount": CreateAccount,
        "trustServer": GetInputChecked("TrustServer")
    }));
}

function Next()
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState == 4)
        {
            if (xhttp.status != 200)
                ShowError(xhttp);
        };
    }

    xhttp.open("POST", "XmppComplete", true);
    xhttp.send("");
}

function GetInputValue(Id)
{
    var Element = document.getElementById(Id);
    if (Element == null)
        return null;

    return Element.value;
}

function GetInputChecked(Id)
{
    var Element = document.getElementById(Id);
    if (Element == null)
        return null;

    return Element.checked;
}

function SetInputValue(Id, Value)
{
    var Element = document.getElementById(Id);
    if (Element != null)
        Element.value = Value;
}

function ShowStatus(data)
{
    var Div = document.getElementById("Status");
    Div.innerHTML = Div.innerHTML + "<p>" + data + "</p>";
}

function ToggleCustomProperties()
{
    var CheckBox = document.getElementById("Custom");
    var Div = document.getElementById("CustomProperties");
    Div.style.display = CheckBox.checked ? "block" : "none";
}

function ShowCustomProperties(Data)
{
    document.getElementById("Custom").checked = Data.visible;
    ToggleCustomProperties();
}

function ToggleTransport()
{
    var Transport = document.getElementById("Transport").value;
    document.getElementById("C2S").style.display = (Transport == "C2S" ? "block" : "none");
    document.getElementById("BOSH").style.display = (Transport == "BOSH" ? "block" : "none");
}

function ShowTransport(Data)
{
    document.getElementById("Transport").value = Data.method;
    ToggleTransport();
}

function ToggleCreateAccount()
{
    var CreateAccount = document.getElementById("CreateAccount").checked;
    document.getElementById("Create").style.display = (CreateAccount ? "block" : "none");
}

function ShowFail1(data)
{
    document.getElementById("Fail1").style.display = "block";
}

function ConnectionOK0(data)
{
    ShowStatus(data);
    document.getElementById("Credentials").style.display = "block";
    document.getElementById("Success0").style.display = "block";
    document.getElementById("Account").focus();
    document.getElementById("ConnectionStatus").style.display = "none";
}

function ConnectionOK1(data)
{
    ShowStatus(data);
    document.getElementById("NextButton").style.display = "inline";
    window.alert("Connection successful. Press the Next button to save settings and continue.");
}

function Next()
{
}

function ConnectionError(data)
{
    ShowStatus(data);
    window.alert("Unable to connect to the server. Please verify your connection details and try again.");
}