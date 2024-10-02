Markdown:="BodyOnly: 1\r\nAllowScriptTag: false\r\n\r\n"+Posted;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Markdown,[]);
Response.ContentType="text/xml";
CustomEncode(Waher.Content.Markdown.Xamarin.XamarinFormsExtensions.GenerateXamarinForms(Doc,Waher.Content.Xml.XML.WriterSettings(true,true)),"text/xml")