function DeleteRole(RoleId)
{
    if (window.confirm("Are you sure you want to delete the role " + RoleId + "?"))
        POST({ "method": "Delete", "roleId": RoleId }, "/Settings/Roles.ws");
}