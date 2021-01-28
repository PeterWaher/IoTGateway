if !exists(User) or !User.HasPrivilege("Admin.Security.Roles") then
	Forbidden("Access denied.");

FileFolderImg:=MarkdownToHtml(":file_folder:");
OpenFileFolderImg:=MarkdownToHtml(":open_file_folder:");

[foreach Privilege in select * from Waher.Security.Users.Privilege where ParentFullId=Posted.id do 
	{
		"id":Privilege.FullId, 
		"expand": "ExpandPrivilege.ws", 
		"html": "<code>"+Privilege.LocalId+"</code>",
		"collapsedImg": FileFolderImg,
		"expandedImg": OpenFileFolderImg
	}
]