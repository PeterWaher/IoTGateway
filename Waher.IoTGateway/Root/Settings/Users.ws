if !exists(User) or !User.HasPrivilege("Admin.Security.Users") then
	Forbidden("Access denied.");

if Posted.data.method="Delete" then
(
	delete from Waher.Security.Users.User where UserName=Posted.data.userName;
	LogInformation("User deleted.",{"Object":Posted.data.userName,"Actor":User.UserName});
	ReloadPage("/Settings/Users.md");
)