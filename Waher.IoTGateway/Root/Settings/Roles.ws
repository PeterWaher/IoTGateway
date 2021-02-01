if !exists(User) or !User.HasPrivilege("Admin.Security.Roles") then
	Forbidden("Access denied.");

if Posted.data.method="Delete" then
(
	delete from Waher.Security.Users.Role where Id=Posted.data.roleId;
	LogInformation("Role deleted.",{"Object":Posted.data.roleId,"Actor":User.UserName});
	ReloadPage("/Settings/Roles.md");
)