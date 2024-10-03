Markdown:="AllowScriptTag: false\r\n\r\n"+Posted;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,[]);
Response.ContentType="application/javascript";
CustomEncode(Waher.Content.Markdown.JavaScript.JavaScriptExtensions.GenerateJavaScript(Doc),"application/javascript")