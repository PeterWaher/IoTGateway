Title: Markdown Laboratory XML
Description: This page converts Markdown text to XML.
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
Doc.ExportXml()
}}
```