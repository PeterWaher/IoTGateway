Title: Restore Backup
Description: Allows the user to restore the contents of a backup.
Date: 2018-09-14
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Settings/Next.js
JavaScript: /Settings/Restore.js
JavaScript: /Settings/XMPP.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Login: /Login.md

========================================================================

Restore Backup
===================

Do you want to restore a previous backup? If so, check the box below and provide the name of the backup file you wish to restore. If the backup file is
encrypted, you will need to provide a key file as well. If you don't want to restore a previous backup, just click the **Next** button.

<form>
<fieldset>
<legend>Restore contents from backup file</legend>

<p>
<input type="checkbox" name="RestoreBackup" id="RestoreBackup" onclick="ToggleRestoreBackup()"/>
<label for="RestoreBackup" title="If you want to restore the contents of a backup file, check this box.">Restore contents of Backup File.</label>
</p>

<div id="RestoreProperties" style="display:none">

<p>
<label for="BackupFile">Backup File:</label>  
<input id="BackupFile" name="BackupFile" type="file" title="Backup File to restore." accept="*/*" onchange="HideNext()"/>
</p>

<p>
<label for="KeyFile">Key File (if encrypted):</label>  
<input id="KeyFile" name="KeyFile" type="file" title="Key File to decrypt backup file." accept="*/*" onchange="HideNext()"/>
</p>

**Note**: By restoring the contents of a backup file, you may lose the contents of the current database. Check the following checkbox to replace any
existing data with the contents of the backup file. If not checked, the backup file will simply be verified

<p>
<input type="checkbox" name="OverwriteExisting" id="OverwriteExisting" onclick="ToggleOverwriteExisting()"/>
<label for="OverwriteExisting" title="If you want to replace existing data with the contents of the backup file, check this box.">Overwrite existing data.</label>
</p>

<p style="display:none">
<input type="checkbox" name="OnlySelectedCollections" id="OnlySelectedCollections" onclick="ToggleSelectedCollections()"/>
<label for="OnlySelectedCollections" title="If only selected collections are to be restored.">Only restore selected collections.</label>
</p>

<fieldset id="SelectedCollections" style="display:none">
<legend>Collections</legend>
<div id='Collections'>
{{foreach CollectionName in Database.GetCollections().Result do ]]
<p>
<input type="checkbox" name="Collection_((CollectionName))" data-collection="((CollectionName))" id="Collection_((CollectionName))"/>
<label for="Collection_((CollectionName))" title="If checked, objects in collection ((CollectionName)) will be restored.">((CollectionName))</label>
</p>[[}}
</div>
</fieldset>

</div>

<p>
{{if Configuring then ]]
<button id='NextButton' type='button' onclick='Next()'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}
<button id='RestoreButton' type='button' onclick='Restore()' style='display:none'>Verify</button>
</p>
</fieldset>

<fieldset id="RestorationStatus" style="display:none">
<legend>Status</legend>
<div id='Status'></div>
</fieldset>

</form>
