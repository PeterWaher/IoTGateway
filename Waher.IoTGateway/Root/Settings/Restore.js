function ToggleRestoreBackup()
{
    var CheckBox = document.getElementById("RestoreBackup");
    var Checked = CheckBox.checked;

    var Element = document.getElementById("RestoreProperties");
    Element.style.display = Checked ? "block" : "none";

    Element = document.getElementById("NextButton");
    Element.style.display = Checked ? "none" : "inline";

    Element = document.getElementById("RestoreButton");
    Element.style.display = Checked ? "inline" : "none";

    if (Checked)
        document.getElementById("BackupFile").focus();
}

function ToggleOverwriteExisting()
{
    var CheckBox = document.getElementById("OverwriteExisting");
    var OverwriteExisting = CheckBox.checked;

    var Element = document.getElementById("RestoreButton");
    Element.innerText = OverwriteExisting ? "Restore" : "Verify";

    HideNext();
}

function Restore()
{
    document.getElementById("Status").innerHTML = "";
    document.getElementById("RestorationStatus").style.display = 'block';

    var BackupFiles = document.getElementById("BackupFile").files;
    var KeyFiles = document.getElementById("KeyFile").files;
    var c = BackupFiles.length;
    var d = KeyFiles.length;
    var i;
    var File;
    var UploadState;

    UploadState = {
        "index": 0,
        "length": c + d,
        "pos": 0,
        "block": 0,
        "chunkSize": 256 * 1024,
        "files": new Array(c + d),
        "Advance": function ()
        {
            if (this.index < this.length)
            {
                var FileRec = this.files[this.index];
                var File = FileRec.file;
                var FileLength = File.size;

                if (this.pos < FileLength)
                {
                    var Reader = new FileReader();
                    var EndRange = this.pos + this.chunkSize;

                    if (EndRange > FileLength)
                        EndRange = FileLength;

                    var Blob = File.slice(this.pos, EndRange);

                    Reader.onload = function (e)
                    {
                        if (e.target.readyState == FileReader.DONE)
                        {
                            UploadState.pos = EndRange;

                            var xhttp = new XMLHttpRequest();
                            xhttp.onreadystatechange = function ()
                            {
                                if (xhttp.readyState == 4)
                                {
                                    if (xhttp.status == 200)
                                        UploadState.Advance();
                                    else
                                        ShowError(xhttp);

                                    delete xhttp;
                                }
                            };

                            xhttp.open("POST", FileRec.resource, true);
                            xhttp.setRequestHeader("Content-Type", "application/octet-stream");

                            if (UploadState.block == 0)
                                xhttp.setRequestHeader("X-FileName", FileRec.file.name);

                            xhttp.setRequestHeader("X-TabID", TabID);
                            xhttp.setRequestHeader("X-BlockNr", UploadState.block++);
                            xhttp.setRequestHeader("X-More", (EndRange < FileLength) ? "1" : "0");
                            xhttp.send(Reader.result);
                        }
                    };

                    Reader.readAsArrayBuffer(Blob);
                }
                else
                {
                    this.index++;
                    this.pos = 0;
                    this.block = 0;
                    this.Advance();
                }
            }
            else
            {
                var xhttp = new XMLHttpRequest();
                xhttp.onreadystatechange = function ()
                {
                    if (xhttp.readyState == 4)
                    {
                        if (xhttp.status != 200)
                            ShowError(xhttp);

                        delete xhttp;
                    }
                };

                xhttp.open("POST", "/Settings/Restore", true);
                xhttp.setRequestHeader("Content-Type", "application/json");
                xhttp.setRequestHeader("X-TabID", TabID);
                xhttp.send(JSON.stringify({
                    "overwrite": document.getElementById("OverwriteExisting").checked
                }));
            }
        }
    };

    for (i = 0; i < c; i++)
    {
        UploadState.files[i] = {
            "file": BackupFiles[i],
            "resource": "/Settings/UploadBackup"
        };
    }

    for (i = 0; i < d; i++)
    {
        UploadState.files[i + c] = {
            "file": KeyFiles[i],
            "resource": "/Settings/UploadKey"
        };
    }

    if (c + d == 0)
    {
        window.alert("No files selected.");
        return;
    }

    UploadState.Advance();
}

function RestoreFinished(Data)
{
    var Element = document.getElementById("NextButton");
    Element.style.display = Data.ok ? "inline" : "none";

    window.alert(Data.message);
}

function HideNext()
{
    var Element = document.getElementById("NextButton");
    Element.style.display = "none";
}
