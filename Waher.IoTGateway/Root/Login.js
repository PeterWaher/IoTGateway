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

					var Img = document.getElementById("quickLoginImg");
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

						if (Img)
							Img.parentNode.removeChild(Img);
					}
					else
					{
						if (!Img)
						{
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

					Img.addEventListener("load", () =>
					{
						LoginCarousel.CalibrateHeight()
					})

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
	else
		window.location.reload();
}


var LoginTimer = null;
let LoginCarousel = null;
const VirtualTimerInterval = 1000 / 5.0
let VirtualTimerIntervalLastTimeStamp = -1

window.addEventListener("load", () =>
{

	function SetLoginMethod(method) 
	{
		fetch("/LoginMethod.ws",
			{
				method: "POST",
				headers:
				{
					"Content-Type": "application/json",
					"Accept": "application/json",
				},
				body: JSON.stringify({ method: method })
			})

	}

	DisplayQuickLogin();

	LoginCarousel = Carousel("login-carousel")
	LoginCarousel.container.addEventListener("elementchanged", () => 
	{
		const method = LoginCarousel.current.getAttribute("data-login-method")

		if (typeof method === "string" && method !== "")
			SetLoginMethod(method)
	})

	SetLoginMethod(LoginCarousel.current.getAttribute("data-login-method"))

	document.addEventListener("visibilitychange", () => 
	{
		location.reload()
	})
});

function Base64Encode(Data)
{
	const Base64Alphabet =
		[
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
			"N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
			"n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "+", "/"
		];

	var Result = "";
	var i;
	var c = Data.length;

	for (i = 2; i < c; i += 3)
	{
		Result += Base64Alphabet[Data[i - 2] >> 2];
		Result += Base64Alphabet[((Data[i - 2] & 0x03) << 4) | (Data[i - 1] >> 4)];
		Result += Base64Alphabet[((Data[i - 1] & 0x0F) << 2) | (Data[i] >> 6)];
		Result += Base64Alphabet[Data[i] & 0x3F];
	}

	if (i === c)
	{
		Result += Base64Alphabet[Data[i - 2] >> 2];
		Result += Base64Alphabet[((Data[i - 2] & 0x03) << 4) | (Data[i - 1] >> 4)];
		Result += Base64Alphabet[(Data[i - 1] & 0x0F) << 2];
		Result += "=";
	}
	else if (i === c + 1)
	{
		Result += Base64Alphabet[Data[i - 2] >> 2];
		Result += Base64Alphabet[(Data[i - 2] & 0x03) << 4];
		Result += "==";
	}

	return Result;
}

async function CheckEnter(e)
{
	if (e.keyCode === 13)
	{
		e.preventDefault();
		document.getElementById("LoginButton").click();
	}
}

async function CalcPasswordHash(UserName, Domain, Password, Nonce)
{
	var Utf8 = new TextEncoder("utf-8");
	var H1 = sha3_256.arrayBuffer(Utf8.encode(UserName + ":" + Domain + ":" + Password));

	var Algorithm = await window.crypto.subtle.importKey("raw", Utf8.encode(Nonce),
		{
			"name": "HMAC",
			"hash": "SHA-256"
		}, false, ["sign"]);

	var H2 = await window.crypto.subtle.sign("HMAC", Algorithm, H1);

	return this.Base64Encode(new Uint8Array(H2));
}

async function DoLogin(From, Domain)
{
	var UserName = document.getElementById("UserName").value;
	var Password = document.getElementById("Password").value;
	var Nonce = document.getElementById("Nonce").value;

	var Result = await CallServer("/Login",
		{
			"UserName": UserName,
			"PasswordHash": CalcPasswordHash(UserName, Domain, Password, Nonce),
			"Nonce": Nonce
		});

	if (Result.ok)
		window.location.href = From;
	else
		await Popup.Alert(Result.message);
}

setTimeout(() => 
{
	location.href = "/"
}, 5 * 60 * 1000)