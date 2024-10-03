Markdown:="BodyOnly: 1\r\nAllowScriptTag: false\r\n\r\n"+Posted;
Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowHtml:=false;
Settings.AllowInlineScript:=false;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="text/xml";
CustomEncode(Waher.Content.Markdown.Xml.XmlExtensions.ExportXml(Doc),"text/xml")