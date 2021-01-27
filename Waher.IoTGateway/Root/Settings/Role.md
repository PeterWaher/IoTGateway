Title: {{RoleId}}
Description: Allows editing of a role.
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Roles
Login: /Login.md
Parameter: RoleId

========================================================================

{{
Role:=select top 1 * from Waher.Security.Users.Role where Id=RoleId;
if count(Role)=0 then NotFound("Role not found: "+RoleId);

if exists(Posted) then
(
	Privileges:=Create(System.Collections.Generic.List,Waher.Security.Users.PrivilegePattern);
	foreach Privilege in (Posted.Privileges???"").Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries) do
	(
		if Privilege.StartsWith("+") then
			Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege.Substring(1), true))
		else if Privilege.StartsWith("-") then
			Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege.Substring(1), false))
		else
			Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege, true));
	);

	Role.Description:=Posted.Description ??? "";
	Role.Privileges:=Privileges.ToArray();
	
	UpdateObject(Role)
);

Role.Id
}}
===================

<form action="Role.md" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Role definition</legend>

<p>
<label for="Description">Description:</label>  
<textarea id="Description" name="Description">{{Role.Description}}</textarea>
</p>

<p>
<label for="Privileges">Privileges:</label>  
<textarea id="Privileges" name="Privileges">{{
foreach Privilege in Role.Privileges do
(
	if Privilege.Include then ]]+[[ else ]]-[[;
	]]((Privilege.Expression))
[[
)}}</textarea>
<small>Privileges are defined as a set of [regular expressions](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference), 
each row an expression. The first character in each row defines if the expression adds a privilege (`+`) or removes a privilege (`-`). When determining if a role has a
privilege, the list is processed from top to bottom.</small>
</p>

<button type="submit" class="posButton">Apply</button>
<button type="button" class="negButton" onclick="Ok()">Cancel</button>

</fieldset>
</form>
