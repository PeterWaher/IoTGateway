﻿Title: {{UserId}}
Description: Allows editing of a user.
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: /Settings/Next.js
JavaScript: /Settings/XMPP.js
JavaScript: /Events.js
JavaScript: /Settings/AttachId.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Users
Login: /Login.md
Parameter: UserId
Parameter: Add

========================================================================

{{
Item:=select top 1 * from Waher.Security.Users.User where UserName=UserId;
if !exists(Item) then 
(
	if Add then
		Item:=Create(Waher.Security.Users.User)
	else
		NotFound("User not found: "+UserId);
);

if exists(Posted) then
(
	if exists(Posted.UserName) then
	(
		if empty(Posted.UserName) then BadRequest("User Name must not be empty.");
		Item.UserName:=Posted.UserName;
	);

	if exists(Posted.LegalId) then
		Item.UpdateLegalId(Posted.LegalId);

	if exists(Posted.PersonalNumber) then
		Item.PersonalNumber:=Posted.PersonalNumber;

	if exists(Posted.Country) then
		Item.Country:=Posted.Country;

	if (Posted.Password!=Item.PasswordHash) then
	(
		if empty(Posted.Password) then BadRequest("Passwords must not be empty.");
		Item.PasswordHash:=Base64Encode(Waher.Security.Users.Users.ComputeHash(Item.UserName,Posted.Password));
	);

	if exists(Posted.MetaData) then
	(
		MetaData:=Create(System.Collections.Generic.List,Waher.Security.Users.UserMetaData);
		foreach Row in Posted.MetaData.Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries) do
		(
			k:=Row.IndexOf('=');
			if k>0 then 
			(
				Data:=Create(Waher.Security.Users.UserMetaData);
				Data.Name:=Row.Substring(0,k);
				Data.Value:=Row.Substring(k+1);
				MetaData.Add(Data)
			)
		);

		Item.MetaData:=MetaData.ToArray();
	)
	else
		Item.MetaData:=null;

	Item.RoleIds:=(Posted.Roles???"").Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries);
	
	if empty(Item.ObjectId) then
	(
		SaveNewObject(Item);
		LogInformation("User created.",{"Object":Item.UserName,"Actor":User.UserName});
	)
	else
	(
		UpdateObject(Item);
		LogInformation("User updated.",{"Object":Item.UserName,"Actor":User.UserName});
	);

	Waher.Security.Users.Users.ClearCache();
	ReloadPage("/Settings/Users.md");
	
	if Add then SeeOther("User.md?UserId="+Item.UserName);
);

empty(Item.UserName) ? "Add new user" : Item.UserName
}}
===================

<form action="User.md" method="post">
<fieldset>
<legend>User definition</legend>



<div class="menuSplit">
	<div class="menuItemExpand">
		{{if Add then ]]
		<p>
		<label for="UserName">User Name:</label>  
		<input type="text" id="UserName" name="UserName" value="((Item.UserName))" autofocus required/>
		</p>
		[[}}

		<p>
		<label for="LegalId">Legal ID:</label>  
		<input type="text" id="LegalId" name="LegalId" value="{{Item.LegalId}}"/>
		</p>

		<p>
		<label for="PersonalNumber">Personal Number:</label>  
		<input type="text" id="PersonalNumber" name="PersonalNumber" value="{{Item.PersonalNumber}}"/>
		</p>

		<p>
		<label for="Country">Country Code:</label>  
		<input type="text" id="Country" name="Country" value="{{Item.Country}}"/>
		</p>

		<p>
		<label for="Password">Password:</label>  
		<input type="password" id="Password" name="Password" value='{{Item.PasswordHash}}' required/>
		</p>

		<button type='button' onclick='RandomizePassword()'>Create Random Password</button>

	</div>
	<div class="margin">
		<div id="attachIdCode" data-mode="image" data-purpose="To attach id to a user on {{Waher.IoTGateway.Gateway.Domain}}."></div>
	</div>
</div>

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

<button type="submit" class="posButton">{{Add?"Add":"Apply"}}</button>
<button type="button" class="negButton" onclick="Reload('')">Cancel</button>

<small>**Note**: Any changes made to the user will be available once the user logs out, and back in again.</small>

</fieldset>
</form>
