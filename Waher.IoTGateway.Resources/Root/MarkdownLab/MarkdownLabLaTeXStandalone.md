Title: Markdown Laboratory LaTeX Standalone
Description: This page converts Markdown text to LaTeX book, using the book document class.
Author: Peter Waher
Date: 2023-02-15
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
Login: /Login.md
BodyOnly: 1

```
{{
Settings:=Create(Waher.Content.Markdown.MarkdownSettings);
LaTeXSettings:=Create(Waher.Content.Markdown.Latex.LaTeXSettings);
LaTeXSettings.DocumentClass:=Waher.Content.Markdown.Latex.LaTeXDocumentClass.Standalone;
Doc:=Waher.Content.Markdown.MarkdownDocument.CreateAsync(Posted,Settings,[]);
Waher.Content.Markdown.Latex.LatexExtensions.GenerateLaTeX(Doc,LaTeXSettings)
}}
```