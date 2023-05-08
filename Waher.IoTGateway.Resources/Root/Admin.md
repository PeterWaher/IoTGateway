Title: IoT Gateway
Description: Administration page of the IoT Broker
Date: 2020-04-06
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
CSS: /Admin.cssx
Javascript: /TargetBlank.js
JavaScript: /Sniffers/Sniffer.js
UserVariable: User
Login: /Login.md

================================================================================================================================

Administration Page
=======================

{{
MenuHeader(Text):=
(
	]]<h2 class="menuHeader">((Text))</h2>
<div class="menuItems"/>
[[
);

MenuFooter():=
(
	]]</div>
[[
);

MenuItem(Text,Url,Privileges[]):=
(
	if count(Privileges)=0 or And([foreach Privilege in Privileges do (User.HasPrivilege(Privilege)???false)]) then
		]]<div class="menuItem" onclick="OpenPage('((Url))')"><div class="menuItemContent">((Text))</div></div>
[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

SnifferItem(Text,Url,Privileges[]):=
(
	if And([foreach Privilege in Privileges do (User.HasPrivilege(Privilege)???false)]) then
		]]<div class="menuItem" onclick="OpenSniffer('((Url))')"><div class="menuItemContent">((Text))</div></div>
[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

MenuHeader("Communication");
MenuItem("Blocked Endpoints","/RemoteEndpoints.md?BlockedOnly=1","Admin.Communication.Endpoints");
MenuItem("Domain","/Settings/Domain.md","Admin.Communication.Domain");
MenuItem("Notification","/Settings/Notification.md","Admin.Communication.Notification");
MenuItem("Roster","/Settings/Roster.md","Admin.Communication.Roster");
MenuItem("XMPP","/Settings/XMPP.md","Admin.Communication.XMPP");
SnifferItem("XMPP Sniffer","/Sniffers/XMPP.md",["Admin.Communication.XMPP","Admin.Communication.Sniffer"]);
MenuFooter();

MenuHeader("Data");
MenuItem("Backup","/Settings/Backup.md","Admin.Data.Backup");
MenuItem("Database","/Settings/Database.md","Admin.Data.Database");
SnifferItem("Database Sniffer","/Sniffers/Database.md",["Admin.Data.Database","Admin.Communication.Sniffer"]);
MenuItem("Events","/Sniffers/EventLog.md","Admin.Data.Events");
MenuItem("Restore","/Settings/Restore.md","Admin.Data.Restore");
MenuFooter();

MenuHeader("Lab");
MenuItem("GraphViz","/GraphVizLab/GraphVizLab.md",["Admin.Lab.Script","Admin.Lab.GraphViz"]);
MenuItem("Markdown","/MarkdownLab/MarkdownLab.md",["Admin.Lab.Script","Admin.Lab.Markdown"]);
MenuItem("PlantUML","/PlantUmlLab/PlantUmlLab.md",["Admin.Lab.Script","Admin.Lab.PlantUml"]);
MenuItem("Script","/Prompt.md","Admin.Lab.Script");
MenuFooter();

MenuHeader("Legal");
MenuItem("Legal Identity","/Settings/LegalIdentity.md","Admin.Legal.ID");
MenuItem("Personal Data","/Settings/PersonalData.md","Admin.Legal.PersonalData");
MenuItem("Propose Contract","/ProposeContract.md","Admin.Legal.ProposeContract");
MenuItem("Signature Requests","/SignatureRequests.md","Admin.Legal.SignatureRequests");
MenuFooter();

MenuHeader("Presentation");
MenuItem("Theme","/Settings/Theme.md","Admin.Presentation.Theme");
MenuFooter();

MenuHeader("Security");
MenuItem("Roles","/Settings/Roles.md","Admin.Security.Roles");
MenuItem("Users","/Settings/Users.md","Admin.Security.Users");
MenuFooter();

MenuHeader("Session");
MenuItem("Logout","/Logout","");
MenuFooter();

MenuHeader("Software");
foreach Module in Waher.Runtime.Inventory.Types.Modules do
(
	if Module is Waher.IoTGateway.IConfigurableModule then
	(
		foreach Page in Module.GetConfigurablePages() do
			MenuItem(Page.Title,Page.ConfigurationPage,Page.Privileges)
	)
);
MenuFooter();
}}