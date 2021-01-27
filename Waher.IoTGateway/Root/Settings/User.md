Title: {{UserId}}
Description: Allows editing of a user.
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Users
Login: /Login.md
Parameter: UserId

========================================================================

{{
Item:=select top 1 * from Waher.Security.Users.User where UserName=UserId;
if count(Item)=0 then NotFound("User not found: "+UserId);

if exists(Posted) then
(
	if exists(Posted.MetaData) then
	(
		MetaData:=Create(System.Collections.Generic.List,Waher.Security.Users.UserMetaData);
		foreach Row in Posted.MetaData.Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries) do
		(
			i:=Row.IndexOf('=');
			if i>0 then 
			(
				Data:=Create(Waher.Security.Users.UserMetaData);
				Data.Name:=Row.Substring(0,i);
				Data.Value:=Row.Substring(i+1);
				MetaData.Add(Data)
			)
		);

		Item.MetaData:=MetaData.ToArray();
	)
	else
		Item.MetaData:=null;

	Item.RoleIds:=(Posted.Roles???"").Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries);
	
	UpdateObject(Item)
);

Item.UserName
}}
===================

<form action="User.md" method="post" enctype="multipart/form-data">
<fieldset>
<legend>User definition</legend>

<p>
<label for="Roles">Roles:</label>  
<textarea id="Roles" name="Roles">{{foreach RoleId in Item.RoleIds do ]]((RoleId))
[[}}</textarea>
</p>

<p>
<label for="MetaData">Meta-Data:</label>  
<textarea id="MetaData" name="MetaData">{{
if exists(Item.MetaData) then
(
	foreach Tag in Item.MetaData do
	(
		]]((Tag.Name))=((Tag.Value))
[[
	)
)}}</textarea>
<small>Meta-data is defined as a collection of meta-data tags, each consisting of a *Key* and a *Value*, separated by an equals sign (`=`).
The meaning of the meta-data is application-specific.</small>
</p>

<button type="submit" class="posButton">Apply</button>
<button type="button" class="negButton" onclick="Ok()">Cancel</button>

</fieldset>
</form>
