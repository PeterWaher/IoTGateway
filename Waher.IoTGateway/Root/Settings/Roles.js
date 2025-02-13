function DeleteRole(RoleId)
{
    if ((await Popup.Confirm("Are you sure you want to delete the role " + RoleId + "?")))
        POST({ "method": "Delete", "roleId": RoleId }, "/Settings/Roles.ws");
}