function ToggleLegalIdentityProperties()
{
    var CheckBox = document.getElementById("UseLegalIdentity");
    var Checked = CheckBox.checked;

    var Div = document.getElementById("LegalIdentityProperties");
    Div.style.display = Checked ? "block" : "none";

    Div = document.getElementById("NotLegalIdentityProperties");
    Div.style.display = Checked ? "none" : "block";

    if (Checked)
        document.getElementById("FirstName").focus();
}

function AddAltField()
{
    var Index = 0;

    while (document.getElementById("AltFieldName" + Index) !== null)
        Index++;

    var AltFieldName = document.getElementById("AltFieldName");
    if (AltFieldName.value.trim() === "")
        return;

    var AltFieldValue = document.getElementById("AltFieldValue");
    var P = AltFieldName.parentNode;

    var P0 = document.createElement("P");
    P.parentNode.insertBefore(P0, P);

    var Label = document.createElement("LABEL");
    Label.for = "AltFieldName" + Index;
    Label.innerText = "Alternative Field:";
    P0.appendChild(Label);

    var BR = document.createElement("BR");
    P0.appendChild(BR);

    var Input = document.createElement("INPUT");
    Input.id = "AltFieldName" + Index;
    Input.name = Input.id;
    Input.type = "text";
    Input.style.width = AltFieldName.style.width;
    Input.title = AltFieldName.title;
    Input.value = AltFieldName.value.trim();
    P0.appendChild(Input);

    Input = document.createElement("INPUT");
    Input.id = "AltFieldValue" + Index;
    Input.name = Input.id;
    Input.type = "text";
    Input.style.width = AltFieldValue.style.width;
    Input.title = AltFieldValue.title;
    Input.value = AltFieldValue.value.trim();
    P0.appendChild(Input);

    var Button = document.createElement("BUTTON");
    Button.type = "button";
    Button.class = "negButtonSm";
    Button.onclick = function () { RemoveAltField(Index); };
    Button.innerText = "Remove";

    AltFieldName.value = "";
    AltFieldValue.value = "";
    AltFieldName.focus();
}

function RemoveAltField(Index)
{
    var Control = document.getElementById("AltFieldName" + Index);
    if (Control !== null)
    {
        var P = Control.parentNode;
        P.parentNode.removeChild(P);
    }
}

function ApplyIdentity()
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

    document.getElementById("ApplyError").style.display = 'none';
    document.getElementById("NextMessage").style.display = 'none';

    var Req = GetLegalIdentityReq();

    xhttp.open("POST", "/Settings/ApplyLegalIdentity", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(JSON.stringify(Req));
}

function GetLegalIdentityReq()
{
    var Index = 0;
    var Alternative = { };
    var Name;
    var Value;

    while ((Name = document.getElementById("AltFieldName" + Index)) !== null &&
        (Value = document.getElementById("AltFieldValue" + Index)) !== null)
    {
        Alternative[Name.value] = Value.value;
        Index++;
    }

    var Req =
    {
        "protectWithPassword": document.getElementById("ProtectWithPassword").checked,
        "password": document.getElementById("Password").value,
        "password2": document.getElementById("Password2").value,
        "firstName": document.getElementById("FirstName").value,
        "middleName": document.getElementById("MiddleName").value,
        "lastName": document.getElementById("LastName").value,
        "pNr": document.getElementById("PNr").value,
        "address": document.getElementById("Address").value,
        "address2": document.getElementById("Address2").value,
        "postalCode": document.getElementById("PostalCode").value,
        "area": document.getElementById("Area").value,
        "city": document.getElementById("City").value,
        "region": document.getElementById("Region").value,
        "country": document.getElementById("Country").value,
        "alternative": Alternative
    };

    return Req;
}

function ApplicationOK()
{
    document.getElementById("ApplyError").style.display = "none";
    document.getElementById("NextMessage").style.display = "block";
    document.getElementById("NextButton").style.display = "block";
}

function ApplicationError(Message)
{
    var ApplyError = document.getElementById("ApplyError");
    ApplyError.innerText = Message;
    ApplyError.style.display = "block";

    document.getElementById("NextMessage").style.display = "none";
}

function UpdateIdentityTable(HTML)
{
    var Identities = document.getElementById("Identities");
    Identities.innerHTML = HTML;
}

function TogglePasswordProperties()
{
    var CheckBox = document.getElementById("ProtectWithPassword");
    var Checked = CheckBox.checked;

    var Div = document.getElementById("PasswordProperties");
    Div.style.display = Checked ? "block" : "none";

    if (Checked)
        document.getElementById("Password").focus();
}