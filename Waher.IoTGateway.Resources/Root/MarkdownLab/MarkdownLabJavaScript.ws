Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowInlineScript:=exists(User) and User.HasPrivilege("Admin.Lab.Script");
Settings.AllowHtml:=Settings.AllowInlineScript;

Markdown:="AllowScriptTag: " +
	(Settings.AllowInlineScript ? "true" : "false") +
	"\r\n\r\n"+Posted;

Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="application/javascript";
CustomEncode(Waher.Content.Markdown.JavaScript.JavaScriptExtensions.GenerateJavaScript(Doc),"application/javascript")