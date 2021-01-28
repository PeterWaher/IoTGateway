if Posted.data.method="Delete" then
(
	delete from Waher.Security.Users.Role where Id=Posted.data.roleId;
	ReloadPage("/Settings/Roles.md");
)