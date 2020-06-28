function UploadContract()
{
	var ContractFileSelector = document.getElementById("ContractFile");
	var Files = ContractFileSelector.files;

	if (Files.length !== 1)
	{
		window.alert("Select one contract to upload.");
		return;
	}

	var Reader = new FileReader();

	Reader.readAsText(File);
	Reader.addEventListener('load', (event) =>
	{
		var xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function ()
		{
			if (xhttp.readyState === 4)
			{
				if (xhttp.status === 200)
				{
					ContractFileSelector.value = "";
					window.alert("Contract uploaded.");
				}
				else
					ShowError(xhttp);
			}
		};

		xhttp.open("POST", "/ProposeContract", true);
		xhttp.setRequestHeader("Content-Type", "application/xml");
		xhttp.setRequestHeader("X-TabID", TabID);
		xhttp.send(event.target.result);
	});
	reader.readAsText(file);
}