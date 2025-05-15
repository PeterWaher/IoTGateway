function DisplayAttachIdQR() {
    var Div = document.getElementById("attachIdCode");
    if (!Div)
        return;

    if (!Div.hasAttribute("data-done")) {
        Div.className = "QuickLogin";
        Div.setAttribute("data-done", "0");
    }
    else if (Div.getAttribute("data-done") == "1")
        return;

    var Mode = Div.getAttribute("data-mode");
    var Purpose = Div.getAttribute("data-purpose");
    var ServiceId = Div.hasAttribute("data-serviceId") ? Div.getAttribute("data-serviceId") : "";
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function () {
        if (xhttp.readyState === 4) {
            if (xhttp.status === 200) {
                try {
                    var Data = JSON.parse(xhttp.responseText);
                    var A = document.getElementById("quickLoginA");

                    if (!A) {
                        A = document.createElement("A");
                        A.setAttribute("id", "quickLoginA");
                        Div.appendChild(A);
                    }

                    A.setAttribute("href", Data.signUrl);

                    if (Data.text) {
                        var Pre = document.getElementById("quickLoginPre");

                        if (!Pre) {
                            Pre = document.createElement("PRE");
                            Pre.setAttribute("id", "quickLoginPre");
                            A.appendChild(Pre);
                        }

                        Pre.innerText = Data.text;

                        var Img = document.getElementById("quickLoginImg");
                        if (Img)
                            Img.parentNode.removeChild(Img);
                    }
                    else {
                        var Img = document.getElementById("quickLoginImg");

                        if (!Img) {
                            Img = document.createElement("IMG");
                            Img.setAttribute("id", "quickLoginImg");
                            A.appendChild(Img);
                        }

                        if (Data.base64)
                            Img.setAttribute("src", "data:" + Data.contentType + ";base64," + Data.base64) + "&fg=Theme&bg=Theme";
                        else if (Data.src)
                            Img.setAttribute("src", Data.src + "&fg=Theme&bg=Theme");
                        
                        Img.setAttribute("width", Data.width);
                        Img.setAttribute("height", Data.height);

                        var Pre = document.getElementById("quickLoginPre");
                        if (Pre)
                            Pre.parentNode.removeChild(Pre);
                    }


                    LoginTimer = window.setTimeout(DisplayAttachIdQR, 2000);
                }
                catch (e) {
                    console.log(xhttp.responseText);
                }
            }
            else
                ShowError(xhttp);
        };
    }

    var Domain = FindNeuronDomain();
    var Protocol = Domain === "localhost" || Domain.indexOf("localhost:") === 0 ? "http" : "https";
    var Uri = Protocol + "://" + Domain + "/QuickLogin";

    xhttp.open("POST", Uri, true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify(
        {
            "serviceId": ServiceId,
            "tab": TabID,
            "mode": Mode,
            "purpose": Purpose,
            "propertyFilter": "PNR,COUNTRY",
            "attachmentFilter": ""
        }));
}

function SignatureReceived(Data) {
    if (!Data)
        return

    if (Data.Id)
    {
        document.getElementById("LegalId").value = Data.Id
    }

    if (!Data.Properties)
        return

    if (Data.Properties.PNR)
        document.getElementById("PersonalNumber").value = Data.Properties.PNR

    if (Data.Properties.COUNTRY)
        document.getElementById("Country").value = Data.Properties.COUNTRY
}

var LoginTimer = null;

window.addEventListener("load", async () => {
    DisplayAttachIdQR()
})

setTimeout(() => {
    location.href = "/"
}, 5 * 60 * 1000)