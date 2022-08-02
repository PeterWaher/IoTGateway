AuthenticateSession(Request,"User");
Authorize(User,"Admin.Security.Roles");

if Posted.data.method="Delete" then
(
	delete from Waher.Security.Users.Role where Id=Posted.data.roleId;
	LogInformation("Role deleted.",{"Object":Posted.data.roleId,"Actor":User.UserName});
	ReloadPage("/Settings/Roles.md");
)