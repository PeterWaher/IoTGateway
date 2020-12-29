Title: XMPP Sniffer
Description: Allows the user to view XMPP communication.
Date: 2020-12-29
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js
JavaScript: /Settings/Sniffer.js
CSS: /Settings/Sniffer.css
UserVariable: User
Login: /Login.md
Parameter: SnifferId

========================================================================

XMPP Communication
===========================

On this page, you can follow the [XMPP](https://xmpp.org/) communication made from the machine (as a client) to its (parent) broker.
The sniffer will automatically be terminated after some time to avoid performance degradation and leaks. Sniffers should only be
used as a tool for troubleshooting.

{{GW:=Waher.IoTGateway.Gateway;GW.AddWebSniffer(SnifferId,Request,GW.XmppClient,"User",["Sniffers"])}}
