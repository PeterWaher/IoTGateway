if Posted.data.method="Delete" then
(
	delete from Waher.Security.Users.User where UserName=Posted.data.userName;
	ReloadPage("/Settings/Users.md");
)