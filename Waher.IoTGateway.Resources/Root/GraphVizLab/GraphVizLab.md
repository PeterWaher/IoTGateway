Title: GraphViz Laboratory
Description: This page allows the user to work with GraphViz, and view the visualization in real-time.
Author: Peter Waher
Date: 2021-03-31
Master: /Master.md
Javascript: GraphVizLab.js
CSS: GraphVizLab.css
UserVariable: User
Privilege: Admin.Lab.GraphViz
Privilege: Admin.Lab.Script
Login: /Login.md

<div id="Lab">
<section id="DotSection">
<div id="DotDiv">
GraphViz: ([documentation](https:/graphviz.org/))
<textarea id="Dot" autofocus="autofocus" wrap="hard" onkeydown="return DotKeyDown(this,event);"></textarea>

{{
foreach FileName in System.IO.Directory.GetFiles(System.IO.Path.Combine(Gateway.AppDataFolder,"Root","GraphVizLab","Examples"),"*.dot") do
	]]<button class="posButtonSm" type="button" onclick="ShowExample('((s:=System.IO.Path.GetFileName(FileName) ))')">((s.Substring(0,s.Length-4) ))</button>
[[
}}

</div>
</section>

<section id="GraphSection">
<div id="GraphDiv">
</div>
</section>
</div>