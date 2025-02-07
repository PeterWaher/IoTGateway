Title: Markdown Laboratory
Description: This page allows the user to work with Markdown, and view the visualization in real-time.
Author: Peter Waher
Date: 2021-05-29
Master: /Master.md
JavaScript: MarkdownLab.js
JavaScript: /Events.js
JavaScript: /MarkdownEditor.js
CSS: /MarkdownEditor.cssx
CSS: MarkdownLab.css
UserVariable: User
Privilege: Admin.Lab.Markdown
Privilege: Admin.Lab.Script
AllowScriptTag: 1
Login: /Login.md

<div id="Lab">
<section id="MarkdownSection">
![](/MarkdownEditor.md)
</section>

<section id="HtmlSection">

<button class="posButtonPressed" data-suffix="Html" data-type="text/html" onclick="FormatButtonClicked(this)" onchange="InitEditTimer()">HTML</button>
<button class="posButton" data-suffix="SmartContract" data-type="text/xml" onclick="FormatButtonClicked(this)">Contract</button>
<button class="posButton" data-suffix="Text" data-type="text/plain" onclick="FormatButtonClicked(this)">Text</button>
<button class="posButton" data-suffix="Markdown" data-type="text/markdown" onclick="FormatButtonClicked(this)">Markdown</button>
<button class="posButton" data-suffix="XamarinForms" data-type="text/xml" onclick="FormatButtonClicked(this)">Xamarin</button>
<button class="posButton" data-suffix="Xaml" data-type="text/xml" onclick="FormatButtonClicked(this)">XAML</button>
<button class="posButton" data-suffix="Xml" data-type="text/xml" onclick="FormatButtonClicked(this)">XML</button>
<button class="posButton" data-suffix="JavaScript" data-type="application/javascript" onclick="FormatButtonClicked(this)">JavaScript</button>
<button class="posButton" data-suffix="LaTeXBook" data-type="application/x-latex" onclick="FormatButtonClicked(this)">LaTeX Book</button>
<button class="posButton" data-suffix="LaTeXReport" data-type="application/x-latex" onclick="FormatButtonClicked(this)">LaTeX Report</button>
<button class="posButton" data-suffix="LaTeXArticle" data-type="application/x-latex" onclick="FormatButtonClicked(this)">LaTeX Article</button>
<button class="posButton" data-suffix="LaTeXStandalone" data-type="application/x-latex" onclick="FormatButtonClicked(this)">LaTeX Standalone</button>

<div><pre><code id="Output"></code></pre></div>
</section>
</div>