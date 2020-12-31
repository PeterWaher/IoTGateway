UserVariable: User
Privilege: Admin.Legal.ID
Login: /Login.md


| ID | Created || Name  | PNr | Residence | Other | From | To | State |
|:---|:----|:---|:------|:----|:----------|:------|:-----|:---|:------|
{{
JoinName(First,Middle,Last):=
(
	s:=First;

	if !empty(Middle) then
		s+=" "+Middle;

	if !empty(Last) then
		s+=" "+Last;

	s
);

JoinResidence(Identity):=
(
	First:=true;
	v:=[Identity.ADDR, Identity.ADDR2, Identity.ZIP, Identity.AREA, Identity.CITY, Identity.REGION, Identity.COUNTRY];
	s:="";
	foreach s2 in v do
	(
		if !empty(s2) then
		(
			if First then
				First:=false
			else
				s+=", ";
			
			s+=MarkdownEncode(s2)
		)
	);
	s
);

JoinOther(Properties[],Attachments[]):=
(
	First:=true;
	s:="";
	foreach Property in Properties do
	(
		Name:=Property.Name;
		if (Name!="FIRST" and Name!="MIDDLE" and Name!="LAST" and Name!="PNR" and Name!="ADDR" and Name!="ADDR2" and Name!="ZIP" and Name!="AREA" and Name!="CITY" and Name!="REGION" and Name!="COUNTRY") then
		(
			if First then
				First:=false
			else
				s+=", ";
			
			s+=MarkdownEncode(Name)+"\\="+MarkdownEncode(Property.Value)
		)
	);

	if exists(Attachments) then
	(
		foreach Attachment in Attachments do
		(
			if First then
				First:=false
			else
				s+=", ";
			
			s+="<a target=\"blank\" href=\"/Attachments/"+Attachment.Id+"\">"+Attachment.FileName+"</a>";
		)
	);

	s
);

foreach ID in (Config.AllIdentities ?? []) do
	]]| `((ID.Id))` ||||||||||
|  | ((MarkdownEncode(ID.Created.ToShortDateString() ) )) | ((MarkdownEncode(ID.Created.ToLongTimeString() ) )) | ((MarkdownEncode(JoinName(ID["FIRST"],ID["MIDDLE"],ID["LAST"]) ) )) | ((MarkdownEncode(ID["PNR"]) )) | ((JoinResidence(ID) )) | ((JoinOther(ID.Properties,ID.Attachments) )) | ((MarkdownEncode(ID.From.ToShortDateString() ) )) | ((MarkdownEncode(ID.To.ToShortDateString() ) )) | ((ID.State.ToString() )) |
[[;
}}
