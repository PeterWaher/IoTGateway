Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowInlineScript:=exists(User) and User.HasPrivilege("Admin.Lab.Script");
Settings.AllowHtml:=Settings.AllowInlineScript;

Markdown:="AllowScriptTag: " +
	(Settings.AllowInlineScript ? "true" : "false") +
	"\r\n\r\n"+Posted;

LaTeXSettings:=Create(Waher.Content.Markdown.Latex.LaTeXSettings);
LaTeXSettings.DocumentClass:=Waher.Content.Markdown.Latex.LaTeXDocumentClass.Report;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="application/x-latex";
CustomEncode(Waher.Content.Markdown.Latex.LatexExtensions.GenerateLaTeX(Doc,LaTeXSettings),"application/x-latex")
