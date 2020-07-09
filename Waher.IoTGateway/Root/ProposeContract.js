function UploadContract()
{
	var ContractFileSelector = document.getElementById("ContractFile");
	var Files = ContractFileSelector.files;

	if (Files.length !== 1)
	{
		window.alert("Select one contract to upload.");
		return;
	}

	var File = Files[0];
	var Reader = new FileReader();

	Reader.addEventListener('load', (event) =>
	{
		var xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function ()
		{
			if (xhttp.readyState === 4)
			{
				if (xhttp.status === 200)
					Reload(null);
				else
					ShowError(xhttp);
			}
		};

		xhttp.open("POST", "/ProposeContract", true);
		xhttp.setRequestHeader("Content-Type", "application/xml");
		xhttp.send(event.target.result);
	});

	Reader.readAsText(File);
}

function ProposeContract()
{
	ContractCommand(true);
}

function CancelContract()
{
	ContractCommand(false);
}

function ContractCommand(Command)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				Reload(null);
			else
				ShowError(xhttp);
		}
	};

	xhttp.open("POST", "/ProposeContract", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify(Command));
}