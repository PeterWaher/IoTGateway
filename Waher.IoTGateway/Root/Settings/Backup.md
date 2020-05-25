Title: Backup
Description: This page allows you to configure backups and perform manual exports of your data.
Date: 2017-01-27
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
UserVariable: User
Login: /Login.md
JavaScript: /Events.js
JavaScript: /Settings/Backup.js
JavaScript: /Settings/Next.js


Backups
==============

To protect your data, daily backups are performed. Below you can configure when these backups are performed, and where files will be stored.
Backup files can be restored on this machine, or another machine at a later time. You can also create manual backups and export the data
in the database. Click the **Next** button below to continue.

<form action="UpdateBackupSettings" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Backup settings</legend>

If you enable automatic backups, the system will automaticly backup your data every day at a given time.  

<p>
<input name="AutomaticBackups" id="AutomaticBackups" type="checkbox" {{(Export:=Waher.IoTGateway.Export).AutomaticBackups ? "checked" : ""}}/>
<label for="AutomaticBackups">Enable Automatic backups.</label>
</p>

<p>
<label for="BackupTime">Backup time:</label>  
<input name="BackupTime" id="BackupTime" type="time" value="{{Export.BackupTime}}" class="keepBackup"/>
</p>

<p>
<label for="KeepDays">Keep daily backups:</label>  
<input name="KeepDays" id="KeepDays" type="number" min="1" value="{{Export.BackupKeepDays}}" class="keepBackup"/> days
</p>

<p>
<label for="KeepMonths">Keep monthly backups:</label>  
<input name="KeepMonths" id="KeepMonths" type="number" min="1" value="{{Export.BackupKeepMonths}}" class="keepBackup"/> months
</p>

<p>
<label for="KeepYears">Keep yearly backups:</label>  
<input name="KeepYears" id="KeepYears" type="number" min="1" value="{{Export.BackupKeepYears}}" class="keepBackup"/> years
</p>

<p>
<button type="button" onclick="UpdateBackupSettings();">Update</button>
</p>

</fieldset>
</form>



<form action="UpdateBackupFolderSettings" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Backup Folder</legend>

You can redirect any exported files to another folder if you want, on the local machine, or in the network. 
Currently, backup files are stored in this folder:

```
{{Export.FullExportFolder}}
```

Keys are stored in this folder:

```
{{Export.FullKeyExportFolder}}
```

You can update these folders if you want below. By leaving the fields blank, the default backup folder is used. 

<label for="ExportFolder">Backup Folder:</label>  
<input id="ExportFolder" name="ExportFolder" type="text" value="{{Export.ExportFolder}}"/>

If you want encryption keys to be stored in a separate folder, you can provide an additional folder for these. 
If no key folder is provided, the keys will be placed in the same folder as the corresponding backup file.

<label for="KeyFolder">Key Folder:</label>  
<input id="KeyFolder" name="KeyFolder" type="text" value="{{Export.ExportKeyFolder}}"/>

<p>
<button type="button" onclick="UpdateBackupFolderSettings();">Update</button>
</p>

</fieldset>
</form>


<fieldset>
<legend>Manual Backup</legend>

<form id="ExportContents">

Choose what you want to be included in the manual backup, and press the Backup button below. You can also press
the Analyze button to analyze the database and generate a report.

<p>
<input id="Database" name="Database" type="checkbox" {{Export.ExportDatabase ? "checked" : ""}} onclick="ToggleSelectCollections()"/>
<label for="Database">Database contents.</label>
</p>

<p style="display:{{Export.ExportDatabase ? "block" : "none"}}">
<input type="checkbox" name="OnlySelectedCollections" id="OnlySelectedCollections" onclick="ToggleSelectedCollections()"/>
<label for="OnlySelectedCollections" title="If only selected collections are to be backed up.">Only backup selected collections.</label>
</p>

<p>
<input id="WebContent" name="WebContent" type="checkbox" {{Export.ExportWebContent ? "checked" : ""}}/>
<label for="WebContent">Web content.</label>
</p>

{{foreach Rec in Export.GetRegisteredFolders() do ]]

<p>
<input id="((CategoryId:=Rec.CategoryId))" name="((CategoryId))" type="checkbox" ((Export.GetExportFolderAsync(CategoryId).Result ? "checked" : ""))/>
<label for="((CategoryId))">((Rec.DisplayName))</label>
</p>

[[}}

<p>
<label for="TypeOfFile">Type of file:</label>  
<select id="TypeOfFile" name="TypeOfFile" style="width:auto">
<option value="XML"{{(ExportType:=Export.ExportType)=="XML" ? " selected" : ""}}>XML</option>
<option value="Binary"{{ExportType=="Binary" ? " selected" : ""}}>Raw Binary</option>
<option value="Compressed"{{ExportType=="Compressed" ? " selected" : ""}}>Compressed Binary</option>
<option value="Encrypted"{{ExportType=="Encrypted" ? " selected" : ""}}>Compressed and Encrypted Binary</option>
</select>
</p>

<fieldset id="SelectedCollections" style="display:none">
<legend>Collections</legend>
<div id='Collections'>
{{foreach CollectionName in Database.GetCollections().Result do ]]
<p>
<input type="checkbox" name="Collection_((CollectionName))" data-collection="((CollectionName))" id="Collection_((CollectionName))"/>
<label for="Collection_((CollectionName))" title="If checked, objects in collection ((CollectionName)) will be exported.">((CollectionName))</label>
</p>[[}}
</div>
</fieldset>

<p>
<button type="button" onclick="StartExport();">Backup</button>
<button type="button" onclick="StartAnalyze(false);">Analyze</button>
<button type="button" onclick="StartAnalyze(true);">Repair</button>
{{if Configuring then ]]
<button id='NextButton' type='button' onclick='Next()'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}
</p>

</form>



============================================================================================================================================

Available backup files
==============================

<div id="ExportFiles">

| File | Size | Created |    |
|:-----|:----:|:-------:|:--:|
{{Files:=Export.GetExportFiles();foreach File in Files do
	]]| [((File.Name.Replace(")","\\)").Replace("_","\\_");))](/((File.IsKey ? "Key" : "Export"))/((UrlEncode(File.Name).Replace(")","%29");))) | ((File.SizeStr)) | ((File.Created)) | <button class='posButtonSm' onclick='DeleteExport("((File.Name.Replace('"','\\"');))")'>Delete</button> |
[[;}}

</div>