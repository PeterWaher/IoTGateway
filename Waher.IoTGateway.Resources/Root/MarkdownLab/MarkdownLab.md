Title: Markdown Laboratory
Description: This page allows the user to work with Markdown, and view the visualization in real-time.
Author: Peter Waher
Date: 2021-05-29
Master: /Master.md
JavaScript: MarkdownLab.js
JavaScript: /Events.js
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

<button class="posButtonPressed" data-suffix="Html" onclick="FormatButtonClicked(this)">HTML</button>
<button class="posButton" data-suffix="SmartContract" onclick="FormatButtonClicked(this)">Contract</button>
<button class="posButton" data-suffix="Text" onclick="FormatButtonClicked(this)">Text</button>
<button class="posButton" data-suffix="XamarinForms" onclick="FormatButtonClicked(this)">Xamarin</button>
<button class="posButton" data-suffix="Xaml" onclick="FormatButtonClicked(this)">XAML</button>
<button class="posButton" data-suffix="Xml" onclick="FormatButtonClicked(this)">XML</button>
<button class="posButton" data-suffix="JavaScript" onclick="FormatButtonClicked(this)">JavaScript</button>
<button class="posButton" data-suffix="LaTeXBook" onclick="FormatButtonClicked(this)">LaTeX Book</button>
<button class="posButton" data-suffix="LaTeXReport" onclick="FormatButtonClicked(this)">LaTeX Report</button>
<button class="posButton" data-suffix="LaTeXArticle" onclick="FormatButtonClicked(this)">LaTeX Article</button>
<button class="posButton" data-suffix="LaTeXStandalone" onclick="FormatButtonClicked(this)">LaTeX Standalone</button>

<div id="HtmlDiv">
</div>
</section>
</div>