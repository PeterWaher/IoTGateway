Markdown:="AllowScriptTag: false\r\n\r\n"+Posted;
Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowHtml:=false;
Settings.AllowInlineScript:=false;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="application/javascript";
CustomEncode(Waher.Content.Markdown.JavaScript.JavaScriptExtensions.GenerateJavaScript(Doc),"application/javascript")