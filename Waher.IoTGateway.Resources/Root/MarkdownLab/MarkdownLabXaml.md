Title: Markdown Laboratory XAML
Description: This page converts Markdown text to XAML that can be displayed in WPF applications.
Author: Peter Waher
Date: 2021-11-23
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
Login: /Login.md
BodyOnly: 1

```xml
{{
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Posted,[]);
Doc.GenerateXAML(Waher.Content.Xml.XML.WriterSettings(true,true))
}}
```