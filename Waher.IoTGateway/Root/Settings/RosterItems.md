UserVariable: User
Login: /Login.md

{{
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
		":grey_question:";
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
			":grey_question:";
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

foreach Request in Requests do
(
	]]<tr data-bare-jid='((Jid:=Request.FromBareJID))'>
<td>((MarkdownEncode(Jid) ))</td>
<td>:question:</td>
<td>Request subscription</td>
<td colspan="3"></td>
<td><button type='button' class='posButtonSm' onclick='AcceptRequest("((Jid))");'>Accept</button> <button type='button' class='negButtonSm' onclick='DeclineRequest("((Jid))");'>Decline</button>
</tr>
[[
);

foreach RosterItem in Contacts do
(
	if empty(Group) or Group in RosterItem.Groups then
	(
		Jid:=RosterItem.BareJid;
		Resources:=RosterItem.Resources;
		n:=Resources.Length;
		RowSpan:=n>1 ? " rowspan='"+n+"'" : "";
		FirstResource:=n>0?Resources[0]:null;
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
<td((RowSpan))><button type='button' class='posButtonSm' onclick='RenameContact("((Name:=RosterItem.NameOrBareJid))","((Jid))");'>Rename</button> <button type='button' class='negButtonSm' onclick='RemoveContact("((Name))","((Jid))");'>Remove</button> <button type='button' (((x:=RosterItem.State+0)>3 ? "style='display:none'")) class='((x=1 or x=3?"negButtonSm":x=0 or x=2?"posButtonSm"))' onclick='((x=1 or x=3 ? "Unsubscribe" : x=0 or x=2 ? "Subscribe"))Contact("((Name))","((Jid))");'>((x=1 or x=3 ? "Unsubscribe" : x=0 or x=2 ? "Subscribe"))</button> <button id='GroupsButton_((System.Guid.NewGuid();))' type='button' class='posButtonSm' onclick='EditContactGroups(this,"((Jid))");'>Groups</button></td>
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