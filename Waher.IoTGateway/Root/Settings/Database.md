Title: Database Settings
Description: Allows the user to configure database settings.
Date: 2019-02-15
Author: Peter Waher
Copyright: /Copyright.md
Master: Master.md
JavaScript: /Events.js
JavaScript: /Settings/Database.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Login: /Login.md

========================================================================

Database
===================

The following options  let you decide where data is to be stored. For simple installations, a local database might be sufficient. For
clustered services requireing a shared data source, a remote database is required. Consider the options and select the alternative that 
best suits your needs.

<form id="SettingsForm" name="SettingsForm">
<fieldset>
<legend>Database Settings</legend>

{{Config:=Waher.IoTGateway.Setup.DatabaseConfiguration.Instance;
i:=0;
foreach PluginType in Waher.Runtime.Inventory.Types.GetTypesImplementingInterface(Waher.IoTGateway.Setup.Databases.IDatabasePlugin) do
(
	Plugin:=Create(PluginType);
	Title:=Plugin.Name(Language);
	]]<p>
<input name="Plugin" id="Plugin((i))" type="radio" value="((FN:=String(PluginType) ))" onclick="SelectDatabase(this);"((Config.DatabasePluginName=FN ? " checked" : ""))/>
<label for="Plugin((i++))">((MarkdownEncode(Title) ))</label>
</p>
[[;
)}}

<div id="PluginSettings">
{{if (HasSettings:=!empty(Resource:=Config.DatabasePlugin?.SettingsResource)) then
(
	Root:=Waher.IoTGateway.Gateway.RootFolder;
	if (Resource.StartsWith("/")) then
		Resource:=Resource.Substring(1);
	FileName:=System.IO.Path.Combine(Root,Resource);
	]]
((LoadMarkdown(FileName);))
[[
)}}
</div>

<p id="Fail" class="error" style="display:none">Unable to connect to database. Check settings and try again.</p>
<p id="Ok" class="message" style="display:none">
Database connection successful. Press the Next button to save settings and continue.
<span id="Restart" class="error" style="display:none">You will need to restart the service or computer before the changes come into effect.</span>
</p>

<p>
<button id="TestButton" type='button' onclick='TestSettings(false)' style='display:{{HasSettings?"inline":"none"}}'>Test</button>
{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button id='OkButton' type='button' onclick='TestSettings(true)' style='display:((Config.Step>0?"inline":"none"))'>Next</button>
[[ else ]]
<button id='OkButton' type='button' onclick='TestSettings(true)' style='display:((Config.Step>0?"inline":"none"))'>OK</button>
[[}}
</p>

</fieldset>
</form>

