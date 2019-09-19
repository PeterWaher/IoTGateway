function ToggleDomainNameProperties()
{
    var CheckBox = document.getElementById("UseDomainName");
    var Checked = CheckBox.checked;

    var Div = document.getElementById("DomainNameProperties");
    Div.style.display = Checked ? "block" : "none";

    Div = document.getElementById("NotDomainNameProperties");
    Div.style.display = Checked ? "none" : "block";

    if (Checked)
        document.getElementById("DomainName").focus();
}

function DomainNameInput(Control)
{
    var Span = document.getElementById("DomainName2");
    var Prev = Span.innerText;
    var Input = Control.value;
    var Index = 0;
    var AltUpdated = false;

    Span.innerText = Input;

    if (Input.length > 0 && Input.length < 10 && "localhost".substring(0, Input.length) === Input)
        return;

    while ((Control = document.getElementById("AltDomainName" + Index)) !== null)
    {
        if (EndsWith(Control.value, "." + Prev))
        {
            Control.value = Control.value.substring(0, Control.value.length - Prev.length) + Input;
            AltUpdated = true;
        }

        Index++;
    }

    Control = document.getElementById("AltDomainName");
    if (Control !== null)
    {
        if (EndsWith(Control.value, "." + Prev))
        {
            Control.value = Control.value.substring(0, Control.value.length - Prev.length) + Input;
            AltUpdated = true;
        }

        if (!AltUpdated)
            Control.value = "www." + Input;
    }
}

function AddAltDomainName()
{
    var Index = 0;

    while (document.getElementById("AltDomainName" + Index) !== null)
        Index++;

    var AltDomainName = document.getElementById("AltDomainName");
    if (AltDomainName.value.trim() === "")
        return;

    var P = AltDomainName.parentNode;

    var P0 = document.createElement("P");
    P.parentNode.insertBefore(P0, P);

    var Label = document.createElement("LABEL");
    Label.for = "AltDomainName" + Index;
    Label.innerText = "Alternative Domain Name:";
    P0.appendChild(Label);

    var BR = document.createElement("BR");
    P0.appendChild(BR);

    var Input = document.createElement("INPUT");
    Input.id = "AltDomainName" + Index;
    Input.name = Input.id;
    Input.type = "text";
    Input.style.width = AltDomainName.style.width;
    Input.title = AltDomainName.title;
    Input.value = AltDomainName.value.trim();
    P0.appendChild(Input);

    var Button = document.createElement("BUTTON");
    Button.type = "button";
    Button.class = "negButtonSm";
    Button.onclick = function () { RemoveAltDomainName(Index); };
    Button.innerText = "Remove";

    AltDomainName.value = "";
    AltDomainName.focus();
}

function RemoveAltDomainName(Index)
{
    var Control = document.getElementById("AltDomainName" + Index);
    if (Control !== null)
    {
        var P = Control.parentNode;
        P.parentNode.removeChild(P);
    }
}

function TestNames()
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

    document.getElementById("Status").innerHTML = "";
    document.getElementById("ConnectionStatus").style.display = 'block';
    document.getElementById("TestError").style.display = 'none';
    document.getElementById("NextMessage").style.display = 'none';
    document.getElementById("Encryption").style.display = 'none';

    var Req = GetDomainNamesReq();

    xhttp.open("POST", "/Settings/TestDomainNames", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify(Req));
}

function GetDomainNamesReq()
{
    var Req =
	{
		"dynamicDns": document.getElementById("DynamicDns").checked,
		"checkIpScript": document.getElementById("CheckIpScript").value,
		"updateIpScript": document.getElementById("UpdateIpScript").value,
		"dynDnsAccount": document.getElementById("DynDnsAccount").value,
		"dynDnsPassword": document.getElementById("DynDnsPassword").value,
		"dynDnsInterval": parseInt(document.getElementById("DynDnsInterval").value),
        "domainName": document.getElementById("DomainName").value,
        "altDomainName": document.getElementById("AltDomainName").value
    };

    var Index = 0;
    var Control;

    while ((Control = document.getElementById("AltDomainName" + Index)) !== null)
    {
        Req["altDomainName" + Index] = Control.value;
        Index++;
    }

    return Req;
}

function NameNotValid(DomainName)
{
    document.getElementById("InvalidDomainName").innerText = DomainName;
    document.getElementById("TestError").style.display = "block";
    document.getElementById("NextMessage").style.display = "none";
    document.getElementById("Encryption").style.display = "none";

    var Control = document.getElementById("AltDomainName");
    if (Control.value === DomainName)
        Control.focus();
    else
    {
        var Index = 0;

        while ((Control = document.getElementById("AltDomainName" + Index)) !== null && Control.value !== DomainName)
            Index++;

        if (Control !== null)
            Control.focus();
    }
}

function NamesOK()
{
    document.getElementById("TestError").style.display = "none";
    document.getElementById("NextMessage").style.display = "block";
    document.getElementById("Encryption").style.display = "block";
}

function ToggleEncryptionProperties()
{
    var CheckBox = document.getElementById("UseEncryption");
    var Checked = CheckBox.checked;

    var Div = document.getElementById("EncryptionProperties");
    Div.style.display = Checked ? "block" : "none";

    Div = document.getElementById("NotEncryptionProperties");
    Div.style.display = Checked ? "none" : "block";
}

function ToggleCustomCAProperties()
{
    var CheckBox = document.getElementById("CustomCA");
    var Checked = CheckBox.checked;

    document.getElementById("CustomCAProperties").style.display = Checked ? "block" : "none";

    if (Checked)
        document.getElementById("AcmeDirectory").focus();
}

function TestAcme()
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

    document.getElementById("Status").innerHTML = "";
    document.getElementById("ConnectionStatus").style.display = 'block';
    document.getElementById("PleaseWait").style.display = 'block';
    document.getElementById("CertificateError").style.display = 'none';
    document.getElementById("NextMessage2").style.display = 'none';

    var Req = GetDomainNamesReq();

    Req.useEncryption = document.getElementById("UseEncryption").checked;
    Req.contactEMail = document.getElementById("ContactEMail").value;
    Req.customCA = document.getElementById("CustomCA").checked;
    Req.acceptToS = document.getElementById("AcceptToS").checked;
    Req.acmeDirectory = document.getElementById("AcmeDirectory").value;

    xhttp.open("POST", "/Settings/TestCA", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify(Req));
}

function TermsOfService(URL)
{
    document.getElementById("ToS").href = URL;
    document.getElementById("ToSParagraph").style.display = "block";
}

function CertificateError(Message)
{
    document.getElementById("PleaseWait").style.display = 'none';

    var P = document.getElementById("CertificateError");
    P.innerHTML = Message;
    P.style.display = "block";

    ShowStatus(Message);
}

function CertificateOk(Data)
{
    document.getElementById("PleaseWait").style.display = 'none';
    document.getElementById("CertificateError").style.display = "none";
    document.getElementById("NextMessage2").style.display = "block";
    document.getElementById("NextButton").style.display = "inline-block";
}

function ToggleDynamicDnsProperties()
{
	var CheckBox = document.getElementById("DynamicDns");
	var Checked = CheckBox.checked;

	var Div = document.getElementById("DynDnsProperties");
	Div.style.display = Checked ? "block" : "none";
}

function TemplateChanged(Control)
{
	var IpScript;
	var UpdateScript;

	switch (Control.value)
	{
		case "LoopiaSe":
			IpScript = "Html:=HttpGet(\"https://dyndns.loopia.se/checkip\",{\"Accept\":\"text/html\",\"User-Agent\":\"Waher.IoTGateway\"});\r\n" +
				"s:=Html.Body.InnerHtml;\r\n" +
				"s like \"[^0-9]*(?'IP'\\\\d+[.]\\\\d+[.]\\\\d+[.]\\\\d+)\" ? IP : \"\"";
			UpdateScript = "Html:=HttpGet(\"https://dyndns.loopia.se/?system=custom&hostname=\"+Domain+\"&myip=\"+IP,{\"Accept\":\"text/html\",\"User-Agent\":\"Waher.IoTGateway\",\"Authorization\":\"Basic \"+Base64Encode(Encode(UserName+\":\"+Password)[0])})";
			break;

		default:
			return;
	}

	document.getElementById("CheckIpScript").value = IpScript;
	document.getElementById("UpdateIpScript").value = UpdateScript;
}

function DynamicDnsScriptUpdated(Control,Event)
{
	document.getElementById("DynDnsTemplate").value = "";
	return ScriptKeyDown(Control, Event);
}