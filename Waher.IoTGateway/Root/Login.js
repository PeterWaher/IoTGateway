function DisplayQuickLogin()
{
    var Div = document.getElementById("quickLoginCode");
    if (!Div)
        return;

    if (!Div.hasAttribute("data-done"))
    {
        Div.className = "QuickLogin";
        Div.setAttribute("data-done", "0");
    }
    else if (Div.getAttribute("data-done") == "1")
        return;

    var Mode = Div.getAttribute("data-mode");
    var Purpose = Div.getAttribute("data-purpose");
    var ServiceId = Div.hasAttribute("data-serviceId") ? Div.getAttribute("data-serviceId") : "";

    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState === 4)
        {
            if (xhttp.status === 200)
            {
                try
                {
                    var Data = JSON.parse(xhttp.responseText);
                    var A = document.getElementById("quickLoginA");

                    if (!A)
                    {
                        A = document.createElement("A");
                        A.setAttribute("id", "quickLoginA");
                        Div.appendChild(A);
                    }

                    A.setAttribute("href", Data.signUrl);

                    if (Data.text)
                    {
                        var Pre = document.getElementById("quickLoginPre");

                        if (!Pre)
                        {
                            Pre = document.createElement("PRE");
                            Pre.setAttribute("id", "quickLoginPre");
                            A.appendChild(Pre);
                        }

                        Pre.innerText = Data.text;

                        var Img = document.getElementById("quickLoginImg");
                        if (Img)
                            Img.parentNode.removeChild(Img);
                    }
                    else
                    {
                        var Img = document.getElementById("quickLoginImg");

                        if (!Img)
                        {
                            Img = document.createElement("IMG");
                            Img.setAttribute("id", "quickLoginImg");
                            A.appendChild(Img);
                        }

                        if (Data.base64)
                            Img.setAttribute("src", "data:" + Data.contentType + ";base64," + Data.base64);
                        else if (Data.src)
                            Img.setAttribute("src", Data.src);

                        Img.setAttribute("width", Data.width);
                        Img.setAttribute("height", Data.height);

                        var Pre = document.getElementById("quickLoginPre");
                        if (Pre)
                            Pre.parentNode.removeChild(Pre);
                    }
                    if (!hightCalibraded)
                    {
                        loginCarousel.CalibrateHeight()
                        hightCalibraded = true
                    }

                    LoginTimer = window.setTimeout(DisplayQuickLogin, 2000);
                }
                catch (e)
                {
                    console.log(e);
                    console.log(xhttp.responseText);
                }
            }
            else
                ShowError(xhttp);
        };
    }

    var Uri = window.location.protocol + "//" + FindNeuronDomain() + "/QuickLogin";

    xhttp.open("POST", Uri, true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify(
        {
            "serviceId": ServiceId,
            "tab": TabID,
            "mode": Mode,
            "purpose": Purpose
        }));
}

function SignatureReceivedBE(Empty)
{
    if (LoginTimer)
        window.clearTimeout(LoginTimer);

    var s = window.location.href;
    var i = s.indexOf("from=");
    if (i > 0)
    {
        s = s.substring(i + 5);
        window.location.href = unescape(s);
    }
}


var LoginTimer = null;
let hightCalibraded = false;
let loginCarousel = null;

window.addEventListener("load", () => {
    function SetLoginMethod(method) 
    {
        fetch("/GatewayRoot/LoginMethod.ws", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json",
            },
            body: JSON.stringify({method:method})
        })
    }

    DisplayQuickLogin();
    loginCarousel = Carousel("login-carousel")
    loginCarousel.container.addEventListener("elementchanged", () => {
        const method = loginCarousel.current.getAttribute("data-login-method")

        if (typeof method === "string" && method !== "")
            SetLoginMethod(method)
    })
});
