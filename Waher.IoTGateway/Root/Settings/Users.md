Title: Users
Description: Displays available users
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: Users.js
JavaScript: /Settings/Next.js
JavaScript: /TargetBlank.js
JavaScript: /Events.js
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Users
Login: /Login.md

========================================================================

Users
===================

Following table displays available users, and the corresponding roles they have.

<table>
<thead>
<tr>
<th>User</th>
<th>Role\(s)</th>
<th>Meta-data</th>
<th>Actions</th>
</tr>
</thead>
<tbody>
{{
Users:=select * from Waher.Security.Users.User order by UserName;
foreach Item in Users do
(
	]]<tr><td><a target="_blank" href="User.md?UserId=((Item.UserName))">`((Item.UserName))`</a></td><td>[[;
	
	foreach RoleId in Item.RoleIds do
		]]`((RoleId))`<br/>[[;
		
	]]</td><td>[[;
	
	if exists(Item.MetaData) then
	(
		foreach Tag in Item.MetaData do
			]]`((Tag.Name))` = `((Tag.Value))`<br/>[[;
	);
	
	]]</td><td>
<button type="button" class="negButtonSm" onclick="DeleteUser('((Item.UserName))')">Delete</button>
</td></tr>
[[;
)
}}
</tbody>
</table>

<button type="button" class="posButton" onclick="OpenPage('User.md?Add=1')">Add</button>
