Parameter: Scale

<div class="MarkdownDiv" data-scale="{{(Exists(Scale) && (Scale = True || Scale = "")) ? "true" : "false"}}">
<div class="MarkdownEditorToolbar">
<button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorBold" onclick="MarkdownEditorBold(this)" title="Bold (CTRL+B)">
**B**</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorItalic" onclick="MarkdownEditorItalic(this)" title="Italic (CTRL+I)">
*I*</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorUnderline" onclick="MarkdownEditorUnderline(this)" title="Underline (CTRL+U)">
_U_</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorStrikeThrough" onclick="MarkdownEditorStrikeThrough(this)" title="Strike Through (CTRL+S)">
~S~</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorSuperscript" onclick="MarkdownEditorSuperscript(this)" title="Superscript (CTRL+P)">
x^2</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorSubscript" onclick="MarkdownEditorSubscript(this)" title="Subscript (CTRL+D)">
x[2]</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorInsert" onclick="MarkdownEditorInsert(this)" title="Insert (CTRL+INS)">
__x__</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorDelete" onclick="MarkdownEditorDelete(this)" title="Delete (CTRL+DEL)">
~~x~~</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorInlineCode" onclick="MarkdownEditorInlineCode(this)" title="Inline Code (CTRL+G)">
💻</button><span class="MarkdownEditorToolbarSeparator">|</span><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHeader1" onclick="MarkdownEditorHeader1(this)" title="Level 1 Header (CTRL+1)">
H1</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHeader2" onclick="MarkdownEditorHeader2(this)" title="Level 2 Header (CTRL+2)">
H2</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHeader3" onclick="MarkdownEditorHeader3(this)" title="Level 3 Header (CTRL+3)">
H3</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHeader4" onclick="MarkdownEditorHeader4(this)" title="Level 4 Header (CTRL+4)">
H4</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHeader5" onclick="MarkdownEditorHeader5(this)" title="Level 5 Header (CTRL+5)">
H5</button><span class="MarkdownEditorToolbarSeparator">|</span><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorBulletList" onclick="MarkdownEditorBulletList(this)" title="Bullet list (CTRL+*)*">
≔</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorNumberList" onclick="MarkdownEditorNumberList(this)" title="Numbered list (CTRK+-)">
1.</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorDefinitionList" onclick="MarkdownEditorDefinitionList(this)" title="Definition list (CTRL+§)">
§</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorTaskList" onclick="MarkdownEditorTaskList(this)" title="Task list (CTRL++)">
⌧</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorTable" title="Table">
𝄜<table>
<thead>
<tr><th></th><th colspan="7">Columns</th></tr>
</thead>
<tbody>
<tr><th rowspan="7" class="Rot90"><div>Rows</div></th>{{foreach Column in 1..7 do ]]<td title="Table with ((Column)) column(s) and 1 row"><button type="button" tabindex="-1" class="MarkdownEditorTableButton" onclick="MarkdownEditorTable(this,((Column)),1)" title="Table">((Column))⨯1</button></td>[[}}</tr>
{{foreach Row in 2..7 do 
(
	]]<tr>[[;
	foreach Column in 1..7 do ]]<td title="Table with ((Column)) column(s) and ((Row)) row(s)"><button type="button" tabindex="-1" class="MarkdownEditorTableButton" onclick="MarkdownEditorTable(this,((Column)),((Row)))" title="Table">((Column))⨯((Row))</button>[[;
	]]</tr>[[
)}}
</tbody>
</table></button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorQuote" onclick="MarkdownEditorQuote(this)" title="Block quote (CTRL+SHIFT+2)">
“”</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorAddedBlock" onclick="MarkdownEditorAddedBlock(this)" title="Added block (CTRL+SHIFT+INS)">
__“”__</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorDeletedBlock" onclick="MarkdownEditorDeletedBlock(this)" title="Deleted block (CTRL+SHIFT+DEL)">
~~“”~~</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorCodeBlock" onclick="MarkdownEditorCodeBlock(this)" title="Code Block (CTRL+SHIFT+G)">
💻</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHorizontalSeparator" onclick="MarkdownEditorHorizontalSeparator(this)" title="Horizontal separator (CTRL+SHIFT+-)">
⎯</button><span class="MarkdownEditorToolbarSeparator">|</span><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorLeftAlignment" onclick="MarkdownEditorLeftAlignment(this)" title="Left alignment (ALT+LEFT)">
⇤</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorCenterAlignment" onclick="MarkdownEditorCenterAlignment(this)" title="Center alignment (ALT+DOWN)">
↔</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorRightAlignment" onclick="MarkdownEditorRightAlignment(this)" title="Right alignment (ALT+RIGHT)">
⇥</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorMarginAlignment" onclick="MarkdownEditorMarginAlignment(this)" title="Margin alignment (ALT+UP)">
↹</button><span class="MarkdownEditorToolbarSeparator">|</span><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorLink" onclick="MarkdownEditorLink(this)" title="Hyperlink (CTRL+L)">
🔗</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorImage" onclick="MarkdownEditorImage(this)" title="Image (CTRL+M)">
🖼️</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorAbbreviation" onclick="MarkdownEditorAbbreviation(this)" title="Abbreviation (CTRL+R)">
a.</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHashTag" onclick="MarkdownEditorHashTag(this)" title="Hash-tag (#)">
#</button><span class="MarkdownEditorToolbarSeparator">|</span><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorPreview" onclick="MarkdownEditorPreview(this)" title="Preview (ALT+1)">
👁</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorBottomPreviewAndEdit" onclick="MarkdownEditorBottomPreviewAndEdit(this)" title="Edit and Bottom Preview (ALT+2)">
⌹</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorSidePreviewAndEdit" onclick="MarkdownEditorSidePreviewAndEdit(this)" title="Edit and Side Preview (ALT+2)">
•|•</button><button type="button" tabindex="-1" class="MarkdownEditorButton MarkdownEditorHelp" onclick="MarkdownEditorHelp(this)" title="Markdown Reference (F1)">
?</button>
</div>
</div>
<img src onerror="MarkdownEditorInitializeHack(event)"/>