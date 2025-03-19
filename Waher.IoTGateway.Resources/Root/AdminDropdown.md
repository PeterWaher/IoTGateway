{{

IsAuthorized(Privileges[]):=
(
	if !exists(User) || User is not Waher.Security.IUser then
		false
	else if count(Privileges)=0 then
		true
	else
		And([foreach Privilege in Privileges do (User.HasPrivilege(Privilege)???false)])
);

MenuItem(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<a href="((Url))">((Text))</a>
[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

MenuItem2(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<li><a href="((Url))">((Text))</a></li>[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

SnifferItem(Text,Url,Privileges[]):=
(
	if IsAuthorized(Privileges) then
		]]<a href="((Url))">((Text))</a>
[[ else ]]<div class="menuItemDisabled"><div class="menuItemContent">((Text))</div></div>
[[
);

}}
[Admin](Admin.md)
<ul>
    <li>
        <p>Communication</p>
        <ul>
            <li>{{MenuItem("Blocked Endpoints","/RemoteEndpoints.md?BlockedOnly=1","Admin.Communication.Endpoints");}}</li>
            <li>{{MenuItem("Domain","/Settings/Domain.md","Admin.Communication.Domain");}}</li>
            <li>{{MenuItem("Notification","/Settings/Notification.md","Admin.Communication.Notification");}}</li>
            <li>{{MenuItem("Roster","/Settings/Roster.md","Admin.Communication.Roster");}}</li>
            <li>{{MenuItem("XMPP","/Settings/XMPP.md","Admin.Communication.XMPP");}}</li>
            <li>{{SnifferItem("XMPP Sniffer","/Sniffers/XMPP.md",["Admin.Communication.XMPP","Admin.Communication.Sniffer"]);}}</li>
        </ul>
    </li>
    <li>
        <p>Data</p>
        <ul>
            <li>{{MenuItem("Backup","/Settings/Backup.md","Admin.Data.Backup");}}</li>
            <li>{{MenuItem("Database","/Settings/Database.md","Admin.Data.Database");}}</li>
            <li>{{SnifferItem("Database Sniffer","/Sniffers/Database.md",["Admin.Data.Database","Admin.Communication.Sniffer"]);}}</li>
            <li>{{MenuItem("Events","/Sniffers/EventLog.md","Admin.Data.Events");}}</li>
            <li>{{MenuItem("Graph Store","/GraphStore.md","Admin.Graphs.Get");}}</li>
            <li>{{MenuItem("Restore","/Settings/Restore.md","Admin.Data.Restore");}}</li>
            <li>{{MenuItem("SPARQL","/Sparql.md","Admin.Graphs.Query");}}</li>
        </ul>
    </li>
    <li>
        <p>Lab</p>
        <ul>
            <li>{{MenuItem("GraphViz","/GraphVizLab/GraphVizLab.md",["Admin.Lab.Script","Admin.Lab.GraphViz"]);}}</li>
            <li>{{MenuItem("Markdown","/MarkdownLab/MarkdownLab.md",["Admin.Lab.Script","Admin.Lab.Markdown"]);}}</li>
            <li>{{MenuItem("PlantUML","/PlantUmlLab/PlantUmlLab.md",["Admin.Lab.Script","Admin.Lab.PlantUml"]);}}</li>
            <li>{{MenuItem("Script","/Prompt.md","Admin.Lab.Script");}}</li>
        </ul>
    </li>
    <li>
        <p>Legal</p>
        <ul>
            <li>{{MenuItem("Legal Identity","/Settings/LegalIdentity.md","Admin.Legal.ID");}}</li>
            <li>{{MenuItem("Personal Data","/Settings/PersonalData.md","Admin.Legal.PersonalData");}}</li>
            <li>{{MenuItem("Propose Contract","/ProposeContract.md","Admin.Legal.ProposeContract");}}</li>
            <li>{{MenuItem("Signature Requests","/SignatureRequests.md","Admin.Legal.SignatureRequests");}}</li>
        </ul>
    </li>
    <li>
        <p>Presentation</p>
        <ul>
            <li>{{MenuItem("favicon.ico","/Settings/EditFavIcon.md","Admin.Presentation.Edit.FavIconIco");}}</li>
            <li>{{MenuItem("Theme","/Settings/Theme.md","Admin.Presentation.Theme");}}</li>
        </ul>
    </li>
    <li>
        <p>Security</p>
        <ul>
            <li>{{MenuItem("robots.txt","/Settings/EditRobots.md","Admin.Security.Edit.RobotsTxt");}}</li>
            <li>{{MenuItem("Roles","/Settings/Roles.md","Admin.Security.Roles");}}</li>
            <li>{{MenuItem("Users","/Settings/Users.md","Admin.Security.Users");}}</li>
        </ul>
    </li>
    <li>
        <p>Session</p>
        <ul>
            <li>{{MenuItem("Change Password","/Settings/ChangePassword.md","");}}</li>
            <li>{{MenuItem("Logout","/Logout","");}}</li>
        </ul>
    </li>
    <li>
        <p>Software</p>
        <ul>
{{
foreach Module in Waher.Runtime.Inventory.Types.Modules do
(
if Module is Waher.IoTGateway.IConfigurableModule then
(
foreach ConfigurablePage in Module.GetConfigurablePages() do 
MenuItem2(ConfigurablePage.Title,ConfigurablePage.ConfigurationPage,ConfigurablePage.Privileges)
)
);
}}
        </ul>
    </li>
</ul>