Title: Markdown Laboratory XAML
Description: This page converts Markdown text to plain text.
Author: Peter Waher
Date: 2021-11-23
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
Login: /Login.md
BodyOnly: 1

```
{{
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Posted,[]).Result;
Doc.GeneratePlainText().Result
}}
```