Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowInlineScript:=exists(User) and User.HasPrivilege("Admin.Lab.Script");
Settings.AllowHtml:=Settings.AllowInlineScript;

Markdown:="BodyOnly: 1\r\nAllowScriptTag: " +
	(Settings.AllowInlineScript ? "true" : "false") +
	"\r\n\r\n"+Posted;

Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="text/markdown";
MarkdownContent(Doc.GenerateMarkdown(true))
