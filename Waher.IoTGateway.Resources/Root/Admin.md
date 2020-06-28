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

MenuHeader("Communication");
MenuItem("Blocked Endpoints","/RemoteEndpoints.md?BlockedOnly=1");
MenuItem("Domain","/Settings/Domain.md");
MenuItem("Notification","/Settings/Notification.md");
MenuItem("Roster","/Settings/Roster.md");
MenuItem("XMPP","/Settings/XMPP.md");
MenuFooter();

MenuHeader("Data");
MenuItem("Backup","/Settings/Backup.md");
MenuItem("Database","/Settings/Database.md");
MenuItem("Prompt","/Prompt.md");
MenuItem("Restore","/Settings/Restore.md");
MenuItem("Search Event Log","/EventLog.md");
MenuFooter();

MenuHeader("Legal");
MenuItem("Legal Identity","/Settings/LegalIdentity.md");
MenuItem("Personal Data","/Settings/PersonalData.md");
MenuItem("Propose Contract","/ProposeContract.md");
MenuItem("Signature Requests","/SignatureRequests.md");
MenuFooter();

MenuHeader("Presentation");
MenuItem("Theme","/Settings/Theme.md");
MenuFooter();

MenuHeader("Session");
MenuItem("Logout","/Logout");
MenuFooter();
}}