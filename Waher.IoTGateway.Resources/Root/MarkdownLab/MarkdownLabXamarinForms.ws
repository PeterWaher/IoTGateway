Markdown:="BodyOnly: 1\r\nAllowScriptTag: false\r\n\r\n"+Posted;
Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
Settings.AllowHtml:=false;
Settings.AllowInlineScript:=false;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,Settings,[]);
Response.ContentType="text/xml";
WriterSettings:=Waher.Content.Xml.XML.WriterSettings(true,true);
CustomEncode(Waher.Content.Markdown.Xamarin.XamarinFormsExtensions.GenerateXamarinForms(Doc,WriterSettings),"text/xml")