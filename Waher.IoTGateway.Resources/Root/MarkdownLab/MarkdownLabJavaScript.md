Title: Markdown Laboratory JavaScript
Description: This page converts Markdown text to JavaScript that can be used to generate dynamic HTML.
Author: Peter Waher
Date: 2024-04-08
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
Login: /Login.md
BodyOnly: 1

```xml
{{
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Posted,[]);
Waher.Content.Markdown.JavaScript.JavaScriptExtensions.GenerateJavaScript(Doc)
}}
```