function SelectServer(Host)
{
    SetInputValue("XmppServer", Host);
    SetInputValue("Port", 5222);
    SetInputValue("BoshUrl", "");
}

function ConnectToHost()
{
    document.getElementById("XmppServerError").style.display = "none";
    document.getElementById("PortError").style.display = "none";
    document.getElementById("BoshUrlError").style.display = "none";
    document.getElementById("Password2Error").style.display = "none";
    document.getElementById("ConnectError").style.display = "none";
    document.getElementById("NextMessage").style.display = "none";
    document.getElementById("Fail1").style.display = "none";
    document.getElementById("Fail2").style.display = "none";
    document.getElementById("ServerFeatures").style.display = "none";
    document.getElementById("ConnectMessage").style.display = "block";

    var Host = GetInputValue("XmppServer");
    if (Host === "")
    {
        document.getElementById("XmppServer").focus();
        document.getElementById("XmppServerError").style.display = "block";
        return;
    }

    var Transport = GetInputValue("Transport");
    var Port = parseInt(GetInputValue("Port"));
    var BoshUrl = GetInputValue("BoshUrl");
    var WsUrl = GetInputValue("WsUrl");

    switch (Transport)
    {
        case "C2S":
            if (Port < 1 || Port > 65536)
            {
                document.getElementById("Port").focus();
                document.getElementById("PortError").style.display = "block";
                return;
            }
            break;

        case "WS":
            if (WsUrl === "")
            {
                document.getElementById("WsUrl").focus();
                document.getElementById("WsUrlError").style.display = "block";
                return;
            }
            break;

        case "BOSH":
            if (BoshUrl === "")
            {
                document.getElementById("BoshUrl").focus();
                document.getElementById("BoshUrlError").style.display = "block";
                return;
            }
            break;
    }

    var Account = GetInputValue("Account");
    var Password = GetInputValue("Password");
    var CreateAccount = GetInputChecked("CreateAccount");
    var Password2 = GetInputValue("Password2");
    var AccountName = GetInputValue("AccountName");

    if (CreateAccount && Password !== Password2)
    {
        document.getElementById("Password2").focus();
        document.getElementById("Password2Error").style.display = "block";
        return;
    }

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status !== 200)
                ShowError(xhttp);
        }
    };

    document.getElementById("Status").innerHTML = "";
    document.getElementById("ConnectionStatus").style.display = 'block';
    document.getElementById("Success0").style.display = 'none';

    xhttp.open("POST", "/Settings/ConnectToHost", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify({
        "host": Host,
        "transport": Transport,
        "port": Port,
        "wsUrl": WsUrl,
        "boshUrl": BoshUrl,
        "account": Account,
        "password": Password,
        "createAccount": CreateAccount,
        "accountName": AccountName,
        "customBinding": GetInputChecked("Custom"),
        "trustServer": GetInputChecked("TrustServer"),
        "insecureMechanisms": GetInputChecked("InsecureMechanisms"),
        "storePassword": GetInputChecked("StorePassword"),
        "sniffer": GetInputChecked("Sniffer")
    }));
}

function RandomizePassword()
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                var Input = document.getElementById("Password");

                Input.value = xhttp.responseText;
                Input.type = "text";

                Input = document.getElementById("Password2");

                Input.value = xhttp.responseText;
                Input.type = "text";
            }
            else
                ShowError(xhttp);
        }
    };

    xhttp.open("POST", "/Settings/RandomizePassword", true);
    xhttp.send("");
}

function GetInputValue(Id)
{
    var Element = document.getElementById(Id);
    if (Element === null)
        return null;

    return Element.value;
}

function GetInputChecked(Id)
{
    var Element = document.getElementById(Id);
    if (Element === null)
        return null;

    return Element.checked;
}

function SetInputValue(Id, Value)
{
    var Element = document.getElementById(Id);
    if (Element !== null)
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
    document.getElementById("C2S").style.display = Transport === "C2S" ? "block" : "none";
    document.getElementById("WS").style.display = Transport === "WS" ? "block" : "none";
    document.getElementById("BOSH").style.display = Transport === "BOSH" ? "block" : "none";
}

function ShowTransport(Data)
{
    document.getElementById("Transport").value = Data.method;
    ToggleTransport();
}

function ToggleCreateAccount()
{
    var CreateAccount = document.getElementById("CreateAccount").checked;
    document.getElementById("Create").style.display = CreateAccount ? "block" : "none";
}

function ShowFail1(data)
{
    ShowStatus(data);
    document.getElementById("Fail1").style.display = "block";
}

function ShowFail2(data)
{
    ShowStatus(data);
    document.getElementById("Fail2").style.display = "block";
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
    var ServerOk = data.offlineMsg && data.blocking && data.reporting && data.abuse && data.spam && data.pep && data.thingRegistry && data.provisioning && data.pubSub;

    ShowStatus(data.msg);
    document.getElementById("NextButton").style.display = "inline";
    document.getElementById("ConnectMessage").style.display = "none";
    document.getElementById("NextMessage").style.display = "block";
    document.getElementById("WarningMessage").style.display = ServerOk ? "none" : "block";

    document.getElementById("OfflineMessages").innerText = data.offlineMsg ? "✓" : "✗";
    document.getElementById("Blocking").innerText = data.blocking ? "✓" : "✗";
    document.getElementById("Reporting").innerText = data.reporting ? "✓" : "✗";
    document.getElementById("AbuseReporting").innerText = data.abuse ? "✓" : "✗";
    document.getElementById("SpamReporting").innerText = data.spam ? "✓" : "✗";
    document.getElementById("Pep").innerText = data.pep ? "✓" : "✗";
    document.getElementById("PepJID").innerText = data.pep;
    document.getElementById("ThingRegistry").innerText = data.thingRegistry ? "✓" : "✗";
    document.getElementById("ThingRegistryJID").innerText = data.thingRegistry;
    document.getElementById("Provisioning").innerText = data.provisioning ? "✓" : "✗";
    document.getElementById("ProvisioningJID").innerText = data.provisioning;
    document.getElementById("PubSub").innerText = data.pubSub ? "✓" : "✗";
    document.getElementById("PubSubJID").innerText = data.pubSub;
    document.getElementById("ServerFeatures").style.display = "block";
}

function ConnectionError(data)
{
    ShowStatus(data);
    document.getElementById("ConnectError").style.display = "block";
}