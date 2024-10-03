Markdown:="AllowScriptTag: false\r\n\r\n"+Posted;
Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
LaTeXSettings:=Create(Waher.Content.Markdown.Latex.LaTeXSettings);
LaTeXSettings.DocumentClass:=Waher.Content.Markdown.Latex.LaTeXDocumentClass.Article;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="application/x-latex";
CustomEncode(Waher.Content.Markdown.Latex.LatexExtensions.GenerateLaTeX(Doc,LaTeXSettings),"application/x-latex")
