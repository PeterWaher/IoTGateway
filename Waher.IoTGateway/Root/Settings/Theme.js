function SetTheme(ThemeId)
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

    xhttp.open("POST", "/Settings/SetTheme", true);
    xhttp.setRequestHeader("Content-Type", "text/plain");
    xhttp.setRequestHeader("X-TabID", TabID);
    xhttp.send(ThemeId);
}

function ThemeOk(Data)
{
    var ThemeId = Data.themeId;
    var Themes = document.getElementById("themes");
    var Loop = Themes.firstChild;

    while (Loop !== null)
    {
        if (Loop.tagName === "DIV")
        {
            if (ThemeId === Loop.getAttribute("data-theme-id"))
                Loop.className = "themeSelected";
            else
                Loop.className = "theme";
        }

        Loop = Loop.nextSibling;
    }

    Loop = document.head.firstChild;
    while (Loop !== null)
    {
        if (Loop.tagName === "LINK" && Loop.rel === "stylesheet" && Loop.href.indexOf("/Themes") >= 0)
        {
            Loop.href = Data.cssUrl;
            break;
        }

        Loop = Loop.nextSibling;
    }

    document.getElementById("NextButton").style.display = "inline-block";
}