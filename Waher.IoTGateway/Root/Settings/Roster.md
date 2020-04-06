Title: {{empty(Group) ? "Roster" : Group}}
Description: Allows the user to configure the roster.
Date: 2020-04-02
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Settings/Next.js
JavaScript: /Settings/Roster.js
JavaScript: /Settings/XMPP.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Login: /Login.md
Parameter: Group

========================================================================

Roster
===================

Communication between clients on the XMPP network can be done only if the clients have been properly authenticated first, and if there
exists explicit consent between them. Each client manages its list of consents in a *roster*, managed by its corresponding broker. The
broker uses the roster to forward *presence* information to approved contacts, giving them the possibility to know the online presence
of the sender, and enable the receiver to communicate with the corresponding client. Without *presence* information, a client cannot 
communicate directly with another. More detailed application-specific authorization can also be managed by ordering contacts into *groups*. 
The *groups* are also implicitly managed by the broker, as they are part of the roster.

Below, you can organize the roster, invite new contacts by sending presence subscriptions to them, revoke consents by removing their
presence subscriptions, unsubscribe from contacts or remove them. You can also assign contacts to groups, Depending on available applications,
these groups can provide a means for a more detailed authorization of access to available data.

<form>
<fieldset>
<legend>Roster</legend>

<div id='Roster'>
<table>
<thead>
<th>Contact</th>
<th colspan="2">Subscription</th>
<th colspan="2">Available</th>
<th>Groups</th>
<th></th>
</thead>
<tbody>
{{
Client:=Waher.IoTGateway.Gateway.XmppClient;
Contacts:=Client.Roster;
Requests:=Client.SubscriptionRequests;
Root:=Waher.IoTGateway.Gateway.RootFolder;
FileName:=System.IO.Path.Combine(Root,"Settings","RosterItems.md");
LoadMarkdown(FileName)
}}
</tbody>
</table>
</div>

<p>
{{if Configuring then ]]
<button id='NextButton' type='button' onclick='Next()'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}
</p>
</fieldset>

<fieldset>
<legend>Invite contact</legend>

<p>
<label for="ConnectToJID">To add a contact to the roster, enter its *Bare JID* (XMPP address) below and press **Connect**: (Your address is **{{Client.BareJID}}**)</label>  
<input id="ConnectToJID" name="ConnectToJID" type="email" autofocus="autofocus" onkeydown="return ConnectToKeyDown(this,event);"/>
</p>

<button class='posButton' type="button" onclick="ConnectToContact();">Connect</button>

</fieldset>
</form>
