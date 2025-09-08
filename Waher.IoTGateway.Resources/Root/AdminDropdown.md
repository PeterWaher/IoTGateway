{{
Categories := [
    {
        header: "Communication",
        items: [
            { label: "Blocked Endpoints", page: "/RemoteEndpoints.md?BlockedOnly=1", privilage: "Admin.Communication.Endpoints" },
            { label: "Domain", page: "/Settings/Domain.md", privilage: "Admin.Communication.Domain" },
            { label: "Notification", page: "/Settings/Notification.md", privilage: "Admin.Communication.Notification" },
            { label: "Roster", page: "/Settings/Roster.md", privilage: "Admin.Communication.Roster" },
            { label: "XMPP", page: "/Settings/XMPP.md", privilage: "Admin.Communication.XMPP" },
            { label: "XMPP Sniffer", page: "/Sniffers/XMPP.md", privilage: ["Admin.Communication.XMPP", "Admin.Communication.Sniffer"], sniffer: true }
        ]
    },
    {
        header: "Data",
        items: [
            { label: "Backup", page: "/Settings/Backup.md", privilage: "Admin.Data.Backup" },
            { label: "Database", page: "/Settings/Database.md", privilage: "Admin.Data.Database" },
            { label: "Database Sniffer", page: "/Sniffers/Database.md", privilage: ["Admin.Data.Database", "Admin.Communication.Sniffer"], sniffer: true },
            { label: "Events", page: "/Sniffers/EventLog.md", privilage: "Admin.Data.Events", sniffer: true },
            { label: "Graph Store", page: "/GraphStore.md", privilage: "Admin.Graphs.Get" },
            { label: "Restore", page: "/Settings/Restore.md", privilage: "Admin.Data.Restore" },
            { label: "SPARQL", page: "/Sparql.md", privilage: "Admin.Graphs.Query" }
        ]
    },
    {
        header: "Lab",
        items: [
            { label: "GraphViz", page: "/GraphVizLab/GraphVizLab.md", privilage: ["Admin.Lab.Script", "Admin.Lab.GraphViz"] },
            { label: "Markdown", page: "/MarkdownLab/MarkdownLab.md", privilage: ["Admin.Lab.Script", "Admin.Lab.Markdown"] },
            { label: "PlantUML", page: "/PlantUmlLab/PlantUmlLab.md", privilage: ["Admin.Lab.Script", "Admin.Lab.PlantUml"] },
            { label: "Script", page: "/Prompt.md", privilage: "Admin.Lab.Script" }
        ]
    },
    {
        header: "Legal",
        items: [
            { label: "Legal Identity", page: "/Settings/LegalIdentity.md", privilage: "Admin.Legal.ID" },
            { label: "Personal Data", page: "/Settings/PersonalData.md", privilage: "Admin.Legal.PersonalData" },
            { label: "Propose Contract", page: "/ProposeContract.md", privilage: "Admin.Legal.ProposeContract" },
            { label: "Signature Requests", page: "/SignatureRequests.md", privilage: "Admin.Legal.SignatureRequests" }
        ]
    },
    {
        header: "Presentation",
        items: [
            { label: "favicon.ico", page: "/Settings/EditFavIcon.md", privilage: "Admin.Presentation.Edit.FavIconIco" },
            { label: "Theme", page: "/Settings/Theme.md", privilage: "Admin.Presentation.Theme" }
        ]
    },
    {
        header: "Security",
        items: [
            { label: "robots.txt", page: "/Settings/EditRobots.md", privilage: "Admin.Security.Edit.RobotsTxt" },
            { label: "Roles", page: "/Settings/Roles.md", privilage: "Admin.Security.Roles" },
            { label: "Users", page: "/Settings/Users.md", privilage: "Admin.Security.Users" }
        ]
    },
    {
        header: "Session",
        items: [
            { label: "Change Password", page: "/Settings/ChangePassword.md", privilage: "" },
            { label: "Logout", page: "/Logout", privilage: "" }
        ]
    },
    {
        header: "Software",
        items: [
        ]
    }
];

foreach Module in Waher.Runtime.Inventory.Types.Modules do (
    if Module is Waher.IoTGateway.IConfigurableModule then
    (
        foreach ConfigurablePage in Module.GetConfigurablePages() do 
            Categories[Count(Categories) - 1].items := PushLast({
                label: ConfigurablePage.Title,
                page: ConfigurablePage.ConfigurationPage,
                privilage: ConfigurablePage.Privileges ?? ""
            }, Categories[Count(Categories) - 1].items);
    )
);
}}
![Admin](/AdminDropdownComponent.md)