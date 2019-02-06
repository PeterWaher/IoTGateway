function StartExport()
{
    var Request = {
        "TypeOfFile": document.getElementById("TypeOfFile").value
    };
    var ExportContents = document.getElementById("ExportContents");
    var Elements = ExportContents.elements;
    var i, c = Elements.length;

    for (i = 0; i < c; i++)
    {
        var Element = Elements[i];
        if (Element.tagName == "INPUT" && Element.type == "checkbox")
            Request[Element.name] = Element.checked;
    }

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState == 4)
		{
			if (xhttp.status == 200)
				AddExportFile(xhttp.responseText, false);
			else
				ShowError(xhttp);

			delete xhttp;
		}
	};

	xhttp.open("POST", "/StartExport", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify(Request));
}

function AddExportFile(FileName, IsKey)
{
	var Loop = document.getElementById("ExportFiles");
	var FirstRow = null;
	var Parent;

	Loop = Loop.firstChild;
	while (Loop != null && Loop.tagName != "TABLE")
		Loop = Loop.nextSibling;

	Parent = Loop;
	Loop = Loop.firstChild;
	while (Loop != null)
	{
		if (Loop.tagName == "TBODY")
		{
			Parent = Loop;
			Loop = Loop.firstChild;
		}
		else
		{
			if (Loop.tagName == "TR")
			{
				if (FirstRow == null)
					FirstRow = Loop;
				else
				{
					AddExportRow(Parent, Loop, FileName, IsKey);
					return;
				}
			}
			else if (Loop.tagName == "THEAD")
				FirstRow = Loop;

			Loop = Loop.nextSibling;
		}
	}

	if (FirstRow != null)
	{
		Loop = FirstRow.nextSibling;
		while (Loop != null)
		{
			if (Loop.tagName=="TBODY")
			{
				AddExportRow(Loop, null, FileName, IsKey);
				return;
			}

			Loop = Loop.nextSibling;
		}

		AddExportRow(FirstRow.parentNode, null, FileName, IsKey);
	}
}

function AddExportRow(Parent, Before, FileName, IsKey)
{
	var Tr = document.createElement("TR");

	if (Before == null)
		Parent.appendChild(Tr);
	else
		Parent.insertBefore(Tr, Before);

	var i;

	for (i = 0; i < 4; i++)
	{
		var Td = document.createElement("TD");
		Tr.appendChild(Td);

		if (i == 0)
		{
			var a = document.createElement("A");
			Td.appendChild(a);

			a.innerText = FileName;

			if (IsKey)
				a.href = "/Key/" + FileName;
			else
				a.href = "/Export/" + FileName;
		}
	}
}

function FindExportFile(FileName)
{
	var Loop = document.getElementById("ExportFiles");

	Loop = Loop.firstChild;
	while (Loop != null && Loop.tagName != "TABLE")
		Loop = Loop.nextSibling;

	Parent = Loop;
	Loop = Loop.firstChild;
	while (Loop != null)
	{
		if (Loop.tagName == "TBODY")
			Loop = Loop.firstChild;
		else
		{
			if (Loop.tagName == "TR")
			{
				var Loop2 = Loop.firstChild;

				while (Loop2 != null && Loop2.tagName != "TD")
					Loop2 = Loop2.nextSibling;

				if (Loop2 != null)
				{
					var Loop3 = Loop2.firstChild;

					while (Loop3 != null && Loop3.tagName != "A")
						Loop3 = Loop3.nextSibling;

					if (Loop3 != null && Loop3.innerText == FileName)
						return Loop2;
				}
			}

			Loop = Loop.nextSibling;
		}
	}

	return null;
}

function UpdateExport(Data)
{
	var Td = FindExportFile(Data.fileName);
	if (Td == null)
	{
		AddExportFile(Data.fileName, Data.isKey);
		Td = FindExportFile(Data.fileName);
	}

	var i = 0;

	Td = Td.nextSibling;
	while (Td != null)
	{
		if (Td.tagName == "TD")
		{
			i++;
			switch (i)
			{
				case 1:
					Td.innerText = Data.size;
					break;

				case 2:
					Td.innerText = Data.created;
					break;

				case 3:
					Td.innerHTML = Data.button;
					return;
			}
		}

		Td = Td.nextSibling;
	}
}

function BackupFailed(Data)
{
	alert(Data.message);
	DeleteExport(Data.fileName);
}

function DeleteExport(FileName)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState == 4)
		{
			if (xhttp.status == 200)
			{
				var Td = FindExportFile(FileName);
				var Tr = Td.parentNode;
				Tr.parentNode.removeChild(Tr);
			}
			else
				ShowError(xhttp);

			delete xhttp;
		}
	};

	xhttp.open("POST", "/DeleteExport", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(FileName);
}

function UpdateBackupSettings()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState == 4)
		{
			if (xhttp.status == 200)
				window.alert("Backup settings have been successfully updated.");
			else
				ShowError(xhttp);

			delete xhttp;
		}
	};

	xhttp.open("POST", "/UpdateBackupSettings", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(document.getElementById("AutomaticBackups").checked + '\n' +
		document.getElementById("BackupTime").value + '\n' + 
		document.getElementById("KeepDays").value + '\n' + 
		document.getElementById("KeepMonths").value + '\n' + 
		document.getElementById("KeepYears").value);
}

function UpdateBackupFolderSettings()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState == 4)
		{
			if (xhttp.status == 200)
				window.alert("Backup folder settings have been successfully updated.");
			else
				ShowError(xhttp);

			delete xhttp;
		}
	};

	xhttp.open("POST", "/UpdateBackupFolderSettings", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(document.getElementById("ExportFolder").value + '\n' +
		document.getElementById("KeyFolder").value);
}

function FileDeleted(Data)
{
	var Td = FindExportFile(Data.fileName);
	if (Td == null)
		return;

	var Tr = Td.parentNode;

	Tr.parentNode.removeChild(Tr);
}

function StartAnalyze(Repair)
{
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function ()
    {
        if (xhttp.readyState == 4)
        {
            if (xhttp.status == 200)
                AddExportFile(xhttp.responseText, false);
            else
                ShowError(xhttp);

            delete xhttp;
        }
    };

    xhttp.open("POST", "/StartAnalyze", true);
    xhttp.setRequestHeader("Content-Type", "application/json");
    xhttp.send(JSON.stringify({
        "repair": Repair
    }));
}
