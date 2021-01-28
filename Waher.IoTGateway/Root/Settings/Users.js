function DeleteUser(UserName)
{
    if (window.confirm("Are you sure you want to delete the user " + UserName + "?"))
        POST({ "method": "Delete", "userName": UserName }, "/Settings/Users.ws");
}