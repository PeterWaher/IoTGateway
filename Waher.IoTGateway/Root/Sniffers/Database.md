Title: Database Sniffer
Description: Allows the user to view Database activity.
Date: 2022-06-02
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js
JavaScript: Sniffer.js
CSS: Sniffer.css
UserVariable: User
Privilege: Admin.Communication.Sniffer
Privilege: Admin.Data.Database
Login: /Login.md
Parameter: SnifferId

========================================================================

Database activity
===========================

On this page, you can follow objects as they are being added, updated or deleted in the database.
The sniffer will automatically be terminated after some time to avoid performance degradation and leaks. Sniffers should only be
used as a tool for troubleshooting.

{{GW:=Waher.IoTGateway.Gateway;GW.AddWebSniffer(SnifferId,Request,Waher.IoTGateway.Setup.DatabaseConfiguration.Instance.SniffableDatabase,"User",["Admin.Communication.Sniffer","Admin.Data.Database"])}}
