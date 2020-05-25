Title: IoT Gateway
Description: Administration page of the IoT Broker
Date: 2020-04-06
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
CSS: /Admin.cssx
Javascript: /TargetBlank.js
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

MenuItem(Text,Url):=
(
	]]<div class="menuItem" onclick="OpenPage('((Url))')"><div class="menuItemContent">((Text))</div></div>
[[
);

MenuHeader("Configuration Settings");
foreach Item in Waher.IoTGateway.Gateway.GetSettingsMenu(Request,"User") do
	MenuItem(MarkdownEncode(Item.Title),Item.Url);
MenuFooter();

MenuHeader("Data");
MenuItem("Backup","/Settings/Backup.md");
MenuItem("Restore","/Settings/Restore.md");
MenuItem("Search Event Log","/EventLog.md");
MenuItem("Prompt","/Prompt.md");
MenuFooter();

}}