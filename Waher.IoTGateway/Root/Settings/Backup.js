function StartExport(ExportOnly)
{
	var SelectedCollections = [];
	var Collections = document.getElementById("Collections");
	var Loop = Collections.firstElementChild;
	var Next;

	while (Loop)
	{
		Next = Loop.nextElementSibling;

		if (Loop.tagName === "P")
		{
			Loop = Loop.firstElementChild;
			if (Loop.tagName === "INPUT" && Loop.checked)
				SelectedCollections.push(Loop.getAttribute("data-collection"));
		}

		Loop = Next;
	}

	var Request = {
		"TypeOfFile": document.getElementById("TypeOfFile").value,
		"selectedCollections": SelectedCollections,
		"exportOnly": ExportOnly
	};
	var ExportContents = document.getElementById("ExportContents");
	var Elements = ExportContents.elements;
	var i, c = Elements.length;

	for (i = 0; i < c; i++)
	{
		var Element = Elements[i];
		if (Element.tagName === "INPUT" && Element.type === "checkbox")
			Request[Element.name] = Element.checked;
	}

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				AddExportFile(xhttp.responseText, false);
			else
				ShowError(xhttp);
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
	while (Loop && Loop.tagName !== "TABLE")
		Loop = Loop.nextSibling;

	Parent = Loop;
	Loop = Loop.firstChild;
	while (Loop)
	{
		if (Loop.tagName === "TBODY")
		{
			Parent = Loop;
			Loop = Loop.firstChild;
		}
		else
		{
			if (Loop.tagName === "TR")
			{
				if (FirstRow)
				{
					AddExportRow(Parent, Loop, FileName, IsKey);
					return;
				}
				else
					FirstRow = Loop;
			}
			else if (Loop.tagName === "THEAD")
				FirstRow = Loop;

			Loop = Loop.nextSibling;
		}
	}

	if (FirstRow)
	{
		Loop = FirstRow.nextSibling;
		while (Loop)
		{
			if (Loop.tagName === "TBODY")
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

	if (Before)
		Parent.insertBefore(Tr, Before);
	else
		Parent.appendChild(Tr);

	var i;

	for (i = 0; i < 4; i++)
	{
		var Td = document.createElement("TD");
		Tr.appendChild(Td);

		if (i === 0)
		{
			var a = document.createElement("A");
			Td.appendChild(a);

			a.setAttribute("target", "_blank");
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
	while (Loop && Loop.tagName !== "TABLE")
		Loop = Loop.nextSibling;

	Parent = Loop;
	Loop = Loop.firstChild;
	while (Loop)
	{
		if (Loop.tagName === "TBODY")
			Loop = Loop.firstChild;
		else
		{
			if (Loop.tagName === "TR")
			{
				var Loop2 = Loop.firstChild;

				while (Loop2 && Loop2.tagName !== "TD")
					Loop2 = Loop2.nextSibling;

				if (Loop2)
				{
					var Loop3 = Loop2.firstChild;

					while (Loop3 && Loop3.tagName !== "A")
						Loop3 = Loop3.nextSibling;

					if (Loop3 && Loop3.innerText === FileName)
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
	if (!Td)
	{
		AddExportFile(Data.fileName, Data.isKey);
		Td = FindExportFile(Data.fileName);
	}

	var i = 0;

	Td = Td.nextSibling;
	while (Td)
	{
		if (Td.tagName === "TD")
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
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var Td = FindExportFile(FileName);
				var Tr = Td.parentNode;
				Tr.parentNode.removeChild(Tr);
			}
			else
				ShowError(xhttp);
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
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				window.alert("Backup settings have been successfully updated.");
			else
				ShowError(xhttp);
		}
	};

	xhttp.open("POST", "/UpdateBackupSettings", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify(
		{
			"AutomaticBackups": document.getElementById("AutomaticBackups").checked,
			"BackupTime": document.getElementById("BackupTime").value,
			"KeepDays": document.getElementById("KeepDays").value,
			"KeepMonths": document.getElementById("KeepMonths").value,
			"KeepYears": document.getElementById("KeepYears").value
		}));
}

function UpdateBackupFolderSettings()
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				window.alert("Backup folder settings have been successfully updated.");
			else
				ShowError(xhttp);
		}
	};

	xhttp.open("POST", "/UpdateBackupFolderSettings", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify(
		{
			"ExportFolder": document.getElementById("ExportFolder").value,
			"KeyFolder": document.getElementById("KeyFolder").value,
			"BackupHosts": document.getElementById("BackupHosts").value,
			"KeyHosts": document.getElementById("KeyHosts").value
		}));
}

function FileDeleted(Data)
{
	var Td = FindExportFile(Data.fileName);
	if (Td)
	{
		var Tr = Td.parentNode;
		Tr.parentNode.removeChild(Tr);
	}
}

function StartAnalyze(Repair)
{
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
				AddExportFile(xhttp.responseText, false);
			else
				ShowError(xhttp);
		}
	};

	xhttp.open("POST", "/StartAnalyze", true);
	xhttp.setRequestHeader("Content-Type", "application/json");
	xhttp.send(JSON.stringify({
		"repair": Repair
	}));
}

function ToggleSelectCollections()
{
	var CheckBox = document.getElementById("Database");
	var Ledger = document.getElementById("Ledger");
	var ExportDatabase = CheckBox.checked;
	var ExportLedger = Ledger && Ledger.checked;

	var CheckBox2 = document.getElementById("OnlySelectedCollections");
	CheckBox2.parentElement.style.display = ExportDatabase || ExportLedger ? "block" : "none";
}

function ToggleSelectedCollections()
{
	var CheckBox = document.getElementById("OnlySelectedCollections");
	var OnlySelectedCollections = CheckBox.checked;

	var Element = document.getElementById("SelectedCollections");
	Element.style.display = OnlySelectedCollections ? "block" : "none";
}
