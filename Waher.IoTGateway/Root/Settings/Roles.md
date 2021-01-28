Title: Roles
Description: Displays available roles
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: /Settings/Next.js
JavaScript: /TargetBlank.js
JavaScript: /Events.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Roles
Login: /Login.md

========================================================================

Roles
===================

Following table displays available roles, and the corresponding privileges they provide.

<table>
<thead>
<tr>
<th>Role</th>
<th>Description</th>
<th>Privileges</th>
</tr>
</thead>
<tbody>
{{
Roles:=select * from Waher.Security.Users.Role order by Id;
foreach Role in Roles do
(
	]]<tr><td><a target="_blank" href="Role.md?RoleId=((Role.Id))">`((Role.Id))`</a></td><td>

((Role.Description))

</td><td>

[[;

	foreach Privilege in Role.Privileges do
	(
		if Privilege.Include then ]]+[[ else ]]-[[;
		]]`((Privilege.Expression))`  
[[
	);

	]]

</td></tr>
[[;
)
}}
</tbody>
</table>

<button type="button" class="posButton" onclick="OpenPage('Role.md?Add=1')">Add</button>
