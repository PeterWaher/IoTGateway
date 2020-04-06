function ConnectToContact()
{
	var Input = document.getElementById("ConnectToJID");
	var JID = Input.value;

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				switch (xhttp.responseText)
				{
					case "0":
						window.alert("Invalid address.");
						break;

					case "1":
					case "2":
						window.alert("Presence subscription sent to " + JID + ".");

						Input.value = "";
						Input.focus();
						break;

					case "3":
						window.alert("You are already connected to " + JID + ".");

						Input.value = "";
						Input.focus();
						break;

					default:
						window.alert("Unrecognized response returned from server.");
						break;
				}
			}
			else
				ShowError(xhttp);
		}
	}

	xhttp.open("POST", "/Settings/ConnectToJID", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(JID);
}

function ConnectToKeyDown(Control, Event)
{
	if (Event.keyCode === 13)
	{
		ConnectToContact();
		return false;
	}
	else
		return true;
}

function RemoveContact(Name, BareJid)
{
	var s = "Are you sure you want to remove " + Name;
	if (Name.toUpperCase() !== BareJid.toUpperCase())
		s += " (" + BareJid + ")";

	s += "?\n\nIf you want to add the contact after having removed it, you need to connect to it again.";

	if (window.confirm(s))
	{
		var xhttp = new XMLHttpRequest();
		xhttp.open("POST", "/Settings/RemoveContact", true);
		xhttp.setRequestHeader("Content-Type", "text/plain");
		xhttp.send(BareJid);
	}
}

function UnsubscribeContact(Name, BareJid)
{
	var s = "Are you sure you want to unsubscribe from " + Name;
	if (Name.toUpperCase() !== BareJid.toUpperCase())
		s += " (" + BareJid + ")";

	s += "?\n\nSince connections are bidirectional, your contact may still be connected to you. You can remove the contact to remove its connection to you.";

	var xhttp = new XMLHttpRequest();
	xhttp.open("POST", "/Settings/UnsubscribeContact", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(BareJid);
}

function SubscribeContact(Name, BareJid)
{
	var s = "Are you sure you want to subscribe to " + Name;
	if (Name.toUpperCase() !== BareJid.toUpperCase())
		s += " (" + BareJid + ")";

	s += "?";

	var xhttp = new XMLHttpRequest();
	xhttp.open("POST", "/Settings/SubscribeToContact", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(BareJid);
}

function RenameContact(Name, BareJid)
{
	var NewName = window.prompt("What name do you want to assign to " + BareJid + "?", Name);
	if (NewName !== null)
	{
		var xhttp = new XMLHttpRequest();
		xhttp.open("POST", "/Settings/RenameContact", true);
		xhttp.setRequestHeader("X-BareJID", BareJid);
		xhttp.setRequestHeader("Content-Type", "text/plain");
		xhttp.send(NewName);
	}
}

function EditContactGroups(Control, BareJid)
{
	var tr = FindContactRow(BareJid);
	if (tr === null)
		return;

	var p = tr.parentNode;
	var tr2 = tr.nextSibling;
	var td;

	if (tr2 !== null)
	{
		td = tr2.firstChild;
		if (td !== null && td.tagName === "TD" && td.colSpan === 7)
		{
			p.removeChild(tr2);
			Control.className = "posButtonSm";
			return;
		}
	}

	var ObjectId = GetObjectId(Control.id);
	var Suffix = "_" + ObjectId;

	tr2 = document.createElement("TR");
	td = document.createElement("TD");
	td.colSpan = 7;
	tr2.appendChild(td);

	if (tr.nextSibling === null)
		p.appendChild(tr2);
	else
		p.insertBefore(tr2, tr.nextSibling);

	var Div = document.createElement("DIV");
	Div.id = "Groups" + Suffix;
	Div.className = "addGroup";
	td.appendChild(Div);

	var Div2 = document.createElement("DIV");
	Div2.className = "addGroupForm";
	Div.appendChild(Div2);

	var Text = document.createTextNode("Group Name:");
	Div2.appendChild(Text);

	var Br = document.createElement("BR");
	Div2.appendChild(Br);

	var Input = document.createElement("INPUT");
	Input.setAttribute("name", "GroupToAdd" + Suffix);
	Input.setAttribute("data-jid", BareJid);
	Input.setAttribute("type", "text");
	Input.setAttribute("style", "margin-right:1.5em;");
	Input.onkeydown = function ()
	{
		return AddGroupKeyDown(this, event);
	};
	Input.onblur = function ()
	{
		AddGroupDelayed(this, ObjectId);
	}
	Div2.appendChild(Input);

	var Button = document.createElement("BUTTON");
	Button.id = "AcceptGroups" + Suffix;
	Button.type = "button";
	Button.className = "posButtonSm";
	Button.innerText = "Apply";
	Button.onclick = function ()
	{
		AcceptContactGroups(this, tr2, Control, BareJid);
	};
	Div2.appendChild(Button);

	var Ul = document.createElement("UL");
	Ul.id = "GroupDropDown" + Suffix;
	Ul.className = "suggestionDropDownHidden";
	Div2.appendChild(Ul);

	var Div3 = document.createElement("DIV");
	Div3.className = "GroupList";
	Div.appendChild(Div3);

	Div3.appendChild(document.createElement("BR"));

	Ul = document.createElement("UL");
	Ul.id = "GroupList" + Suffix;
	Ul.className = "GroupList";
	Div3.appendChild(Ul);

	var Li = document.createElement("LI");
	Ul.appendChild(Li);

	var Div4 = document.createElement("DIV");
	Div4.className = "EndOfTags";
	Div3.appendChild(Div4);

	Control.className = "posButtonSmPressed";

	Input.focus();

	var Loop = tr.firstChild;
	var i = 0;

	while (Loop !== null)
	{
		if (Loop.tagName === "TD")
		{
			i++;
			if (i === 6)
			{
				Loop = Loop.firstChild;
				while (Loop !== null)
				{
					if (Loop.tagName === "DIV")
					{
						var Loop2 = Loop.firstChild;
						while (Loop2 !== null)
						{
							if (Loop2.tagName === "UL")
							{
								var Loop3 = Loop2.firstChild;
								var Added = false;

								while (Loop3 !== null)
								{
									if (Loop3.tagName === "LI" && Loop3.className === "GroupLink")
									{
										Li = document.createElement("LI");
										Li.setAttribute("data-tag", Loop3.innerText);
										Li.className = "Group";
										Li.innerText = Loop3.innerText;
										Li.onclick = function ()
										{
											this.parentNode.removeChild(this);
										};
										Ul.appendChild(Li);
										Added = true;
									}

									Loop3 = Loop3.nextSibling;
								}

								if (Added)
								{
									Li = document.createElement("LI");
									Ul.appendChild(Li);
								}
								break;
							}
							else
								Loop2 = Loop2.nextSibling;
						}
						break;
					}
					else
						Loop = Loop.nextSibling;
				}
				break;
			}
		}

		Loop = Loop.nextSibling;
	}
}

function AcceptContactGroups(Control, Row, Button, BareJid)
{
	var ObjectId = GetObjectId(Control.id);

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				Row.parentNode.removeChild(Row);
				Button.className = "posButtonSm";
			}
			else
				ShowError(xhttp);
		}
	};

	xhttp.open("POST", "/Settings/UpdateContactGroups", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");

	var TagList;
	var i = 0;

	if (ObjectId === null)
		TagList = document.getElementById("GroupList");
	else
		TagList = document.getElementById("GroupList_" + ObjectId);

	if (TagList !== null)
	{
		var Add = (TagList.parentNode.className === "GroupList");
		var Loop = TagList.firstChild;
		var Next;
		var s, s2;
		var Keep = false;

		while (Loop !== null)
		{
			Next = Loop.nextSibling;

			if (Loop.tagName === "LI" && (Loop.className === "Group" || Loop.className === "GroupLink"))
			{
				if (Add)
				{
					s = Loop.getAttribute("data-tag");
					if (s !== null && s.length > 0)
						xhttp.setRequestHeader("X-Group-" + (++i), encodeURI(s));
				}

				if (Loop.getAttribute("data-keep") === "1")
					Keep = true;
				else
					TagList.removeChild(Loop);
			}

			Loop = Next;
		}

		if (Add && !Keep)
		{
			s = "GroupsButton";
			if (ObjectId !== null)
				s += "_" + ObjectId;

			Loop = document.getElementById(s);
			if (Loop !== null)
				Loop.click();
		}
	}

	xhttp.send(BareJid);
}

function FindContactRow(BareJID)
{
	return FindBareJidRow(FindRosterTable(), BareJID);
}

function FindBareJidRow(Table, BareJID)
{
	BareJID = BareJID.toUpperCase();

	if (Table !== null)
	{
		var Loop = Table.firstElementChild;
		while (Loop !== null)
		{
			if (Loop.tagName === "TR")
			{
				var s = Loop.getAttribute("data-bare-jid");
				if (s && s.toUpperCase() === BareJID)
					return Loop;
			}

			Loop = Loop.nextElementSibling;
		}
	}

	return null;
}

function FindRosterTable()
{
	var Result = document.getElementById("Roster");
	if (Result !== null)
	{
		var Loop = Result.firstElementChild;
		while (Loop !== null && Loop.tagName !== "TABLE")
			Loop = Loop.nextElementSibling;

		if (Loop !== null)
		{
			Loop = Loop.firstElementChild;
			while (Loop !== null && Loop.tagName !== "TBODY")
				Loop = Loop.nextElementSibling;

			return Loop;
		}
	}

	return null;
}

function GetObjectId(Name)
{
	var i;

	i = Name.indexOf("_");
	if (i < 0)
		return null;
	else
		return Name.substring(i + 1, Name.length);
}

function AddGroupKeyDown(Control, Event)
{
	if (Event.keyCode === 13)
	{
		AddGroup(Control, GetObjectId(Control.name));
		return false;
	}
	else
	{
		if (addGroupKeyTimer !== null)
		{
			window.clearTimeout(addGroupKeyTimer);
			addGroupKeyTimer = null;
		}

		addGroupKeyTimer = window.setTimeout(AddGroupKeyTimeout, 250, Control);

		return true;
	}
}

var addGroupKeyTimer = null;

function AddGroupKeyTimeout(Control)
{
	addGroupKeyTimer = null;

	var AddGroup = Control.value;
	var ObjectId = GetObjectId(Control.name);

	if (AddGroup === "")
	{
		HideGroupSuggestions(ObjectId);
		return;
	}

	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = function ()
	{
		if (xhttp.readyState === 4)
		{
			if (xhttp.status === 200)
			{
				var Suggestions;

				try
				{
					Suggestions = JSON.parse(xhttp.responseText);
				}
				catch (Exception)
				{
					window.alert("Invalid JSON: " + xhttp.responseText);
					Suggestions = null;
				}

				if (Suggestions !== null && Suggestions.groups !== null && Suggestions.groups.length > 0)
					ShowGroupSuggestions(Control, AddGroup, Suggestions.groups, ObjectId);
				else
					HideGroupSuggestions(ObjectId);
			}
			else
				ShowError(xhttp);
		};
	}

	xhttp.open("POST", "/Settings/GetGroups", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(AddGroup);
}

function ShowGroupSuggestions(Control, AddGroup, AddGroups, ObjectId)
{
	if (Control.value !== AddGroup)
		return;

	var Suffix = ObjectId === null ? "" : "_" + ObjectId;
	var i = 0;
	var c = AddGroups.length;

	var ul = document.getElementById("GroupDropDown" + Suffix);
	var Loop;
	var Next;

	Loop = ul.firstChild;
	while (Loop !== null)
	{
		Next = Loop.nextSibling;

		if (Loop.tagName === "LI")
		{
			if (i >= c)
				ul.removeChild(Loop);
			else
				Loop.innerText = AddGroups[i++];
		}

		Loop = Next;
	}

	while (i < c)
	{
		Loop = document.createElement("LI");
		Loop.innerText = AddGroups[i++];
		Loop.onclick = function ()
		{
			SelectGroup(Control, this.innerText, ObjectId);
		};

		ul.appendChild(Loop);
	}

	ul.className = "suggestionDropDown";
}

function SelectGroup(Control, Group, ObjectId)
{
	if (addGroupDelayedHandler !== null)
	{
		window.clearTimeout(addGroupDelayedHandler);
		addGroupDelayedHandler = null;
	}

	HideGroupSuggestions(ObjectId);

	Control.value = Group;
	AddGroup(Control, ObjectId);
	Control.focus();
}

function HideGroupSuggestions(ObjectId)
{
	var Id = "GroupDropDown";
	if (ObjectId !== null)
		Id += "_" + ObjectId;

	var ul = document.getElementById(Id);
	if (ul !== null)
		ul.className = "suggestionDropDownHidden";
}

function AddGroupDelayed(Control, ObjectId)
{
	if (addGroupDelayedHandler !== null)
	{
		window.clearTimeout(addGroupDelayedHandler);
		addGroupDelayedHandler = null;
	}

	addGroupDelayedHandler = window.setTimeout(AddGroupTimeout, 250, { control: Control, oid: ObjectId });
}

function AddGroupTimeout(P)
{
	addGroupDelayedHandler = null;
	AddGroup(P.control, P.oid);
}

var addGroupDelayedHandler = null;

function AddGroup(Control, ObjectId)
{
	if (addGroupDelayedHandler)
	{
		window.clearTimeout(addGroupDelayedHandler);
		addGroupKeyTimer = null;
	}

	HideGroupSuggestions(ObjectId);

	var Tag = Control.value;
	if (Tag === "")
		return;

	Control.value = "";

	var TagListId = "GroupList";

	if (ObjectId !== null)
		TagListId += "_" + ObjectId;

	var TagList = document.getElementById(TagListId);
	var Item;

	Item = document.createElement("LI");
	Item.setAttribute("class", "Group");
	Item.setAttribute("data-tag", Tag);
	Item.innerText = Tag;

	TagList.appendChild(Item);

	Item.onclick = function ()
	{
		TagList.removeChild(this);
	};

	if (Item.previousSibling !== null && Item.previousSibling.tagName === "LI" && Item.previousSibling.innerText === "" && Item.previousSibling.previousSibling !== null)
		TagList.removeChild(Item.previousSibling);

	var Item2 = document.createElement("LI");
	TagList.appendChild(Item2);

	return Item;
}

function OpenGroup(Group)
{
	var Window = window.open("/Settings/Roster.md?Group=" + encodeURI(Group), "_blank");
	Window.focus();
}

function RemoveRosterItem(Data)
{
	var tr = FindContactRow(Data.bareJid);
	if (tr)
		tr.parentNode.removeChild(tr);
}

function UpdateRosterItem(Data)
{
	var Loop;
	var BareJid = Data.bareJid.toUpperCase();
	var Contacts = FindRosterTable();
	var tr = FindBareJidRow(Contacts, BareJid);
	var s;

	var TBody = document.createElement("TBODY");
	TBody.innerHTML = Data.html;

	while ((Loop = TBody.firstElementChild) !== null)
	{
		TBody.removeChild(Loop);

		if (tr)
			Contacts.insertBefore(Loop, tr);
		else
			Contacts.appendChild(Loop);
	}

	while (tr)
	{
		Loop = tr.nextElementSibling;

		s = tr.getAttribute("data-bare-jid").toUpperCase();
		if (s === BareJid)
			Contacts.removeChild(tr);
		else
			break;

		tr = Loop;
	}
}

function UpdateRoster(Data)
{
	var Contacts = FindRosterTable();
	Contacts.innerHTML = Data.html;
}

function AcceptRequest(Jid)
{
	var xhttp = new XMLHttpRequest();
	xhttp.open("POST", "/Settings/AcceptRequest", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(Jid);
}

function DeclineRequest(Jid)
{
	var xhttp = new XMLHttpRequest();
	xhttp.open("POST", "/Settings/DeclineRequest", true);
	xhttp.setRequestHeader("Content-Type", "text/plain");
	xhttp.send(Jid);
}
