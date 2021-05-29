Title: Markdown Laboratory
Description: This page allows the user to work with Markdown, and view the visualization in real-time.
Author: Peter Waher
Date: 2021-05-29
Master: /Master.md
Javascript: MarkdownLab.js
CSS: MarkdownLab.css
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
Login: /Login.md

<div id="Lab">
<section id="MarkdownSection">
<div id="MarkdownDiv">
Markdown: ([documentation](/Markdown.md))
<textarea id="Markdown" autofocus="autofocus" wrap="hard" onkeydown="return MarkdownKeyDown(this,event);"></textarea>
</div>
</section>

<section id="HtmlSection">
<div id="HtmlDiv">
</div>
</section>
</div>