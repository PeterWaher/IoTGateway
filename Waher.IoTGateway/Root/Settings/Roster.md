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
CT:=Waher.Content.CommonTypes;

SubState:=Waher.Networking.XMPP.SubscriptionState;
None:=SubState.None;
To:=SubState.To;
From:=SubState.From;
Both:=SubState.Both;
Remove:=SubState.Remove;

SubscriptionIcon(Item):=
(
	State:=Item.State;

	if State=None then
		""
	else if State=To then
		":arrow_right:"
	else if State=From then
		":arrow_left:"
	else if State=Both then
		":left_right_arrow:"
	else if State=Remove then
		":x:"
	else
		":question:";
);

SubscriptionLabel(Item):=
(
	State:=Item.State;

	if State=To then
		"To"
	else if State=From then
		"From"
	else if State=Both then
		"Both"
	else 
		"";
);

Availability:=Waher.Networking.XMPP.Availability;
Online:=Availability.Online;
Offline:=Availability.Offline;
Away:=Availability.Away;
Chat:=Availability.Chat;
DoNotDisturb:=Availability.DoNotDisturb;
ExtendedAway:=Availability.ExtendedAway;

PresenceIcon(Presence):=
(
	if exists(Presence) then
	(
		State:=Presence.Availability;

		if State=Online then
			":white_check_mark:"
		else if State=Offline then
			":new_moon:"
		else if State=Away then
			":sleeping:"
		else if State=Chat then
			":grin:"
		else if State=DoNotDisturb then
			":no_entry:"
		else if State=ExtendedAway then
			":dizzy_face:"
		else
			":question:";
	)
	else
		"";
);

PresenceLabel(Presence):=
(
	if exists(Presence) then
	(
		State:=Presence.Availability;

		if State=Online then
			"Online"
		else if State=Offline then
			"Offline"
		else if State=Away then
			"Away"
		else if State=Chat then
			"Chat"
		else if State=DoNotDisturb then
			"Don't Disturb"
		else if State=ExtendedAway then
			"Extended Away"
		else
			"";
	)
	else
		"";
);

foreach RosterItem in Contacts do
(
	if empty(Group) or Group in RosterItem.Groups then
	(
		Jid:=RosterItem.BareJid;
		Resources:=RosterItem.Resources;
		n:=Resources.Length;
		RowSpan:=n>1 ? " rowspan='"+n+"'" : "";
		FirstResource:=n>0?Resources[0];null;
		]]<tr data-bare-jid='((Jid))' data-full-jid='((exists(FirstResource)?FirstResource?.From))'>
<td((RowSpan))>((empty(RosterItem.Name) ? MarkdownEncode(RosterItem.BareJid) : "<span title=\""+HtmlAttributeEncode(RosterItem.BareJid)+"\">"+MarkdownEncode(RosterItem.Name)+"</span>"))</td>
<td((RowSpan))>((SubscriptionIcon(RosterItem) ))</td>
<td((RowSpan))>((SubscriptionLabel(RosterItem) ))</td>
<td>((PresenceIcon(RosterItem.LastPresence) ))</td>
<td>((PresenceLabel(RosterItem.LastPresence) ))</td>
<td((RowSpan))><div class="TagContainer"><ul class="GroupList">[[;
		First:=true;
		foreach GroupName in RosterItem.Groups do
		(
			if First then
			(
				First:=false;
				]]<li></li>[[
			);
			]]<li class="GroupLink" onclick="OpenGroup('((CT.JsonStringEncode(GroupName).Replace("'", "\\'") ))')">((MarkdownEncode(GroupName) ))</li>[[;
		);
		]]<li></li></ul><div class="EndOfTags"></div></div></td>
<td((RowSpan))><button type='button' class='posButtonSm' onclick='RenameContact("((Name))","((Jid))");'>Rename</button> <button type='button' class='negButtonSm' onclick='RemoveContact("((Name))","((Jid))");'>Remove</button> <button type='button' (((x:=RosterItem.State+0)>3 ? "style='display:none'")) class='((x=1 or x=3?"negButtonSm":x=0 or x=2?"posButtonSm"))' onclick='((x=1 or x=3 ? "Unsubscribe" : x=0 or x=2 ? "Subscribe"))Contact("((Name))","((Jid))");'>((x=1 or x=3 ? "Unsubscribe" : x=0 or x=2 ? "Subscribe"))</button> <button id='GroupsButton_((System.Guid.NewGuid();))' type='button' class='posButtonSm' onclick='EditContactGroups(this,"((Jid))");'>Groups</button></td>
</tr>
[[;
		if n>1 then
		(
			for Index:=1 to n-1 do
			(
				Resource:=Resources[Index];
				]]<tr data-bare-jid='((Jid))' data-full-jid='((Resource.From))'>
<td>((PresenceIcon(Resource) ))</td>
<td>((PresenceLabel(Resource) ))</td>
</tr>
[[;
			)
		)
	)
)
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
