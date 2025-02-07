async function DeleteUser(UserName)
{
    if ((await Popup.Confirm("Are you sure you want to delete the user " + UserName + "?")))
        POST({ "method": "Delete", "userName": UserName }, "/Settings/Users.ws");
}