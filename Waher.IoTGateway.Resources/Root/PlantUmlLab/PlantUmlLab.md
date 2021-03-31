Title: PlantUML Laboratory
Description: This page allows the user to work with PlantUML, and view the visualization in real-time.
Author: Peter Waher
Date: 2021-03-31
Master: /Master.md
Javascript: PlantUmlLab.js
CSS: PlantUmlLab.css
UserVariable: User
Privilege: Admin.Lab.PlantUml
Privilege: Admin.Lab.Script
Login: /Login.md

<div id="Lab">
<section id="UmlSection">
<div id="UmlDiv">
Plant UML: ([documentation](https://plantuml.com/))
<textarea id="Uml" autofocus="autofocus" wrap="hard" onkeydown="return UmlKeyDown(this,event);"></textarea>

{{
foreach FileName in System.IO.Directory.GetFiles(System.IO.Path.Combine(Gateway.AppDataFolder,"Root","PlantUmlLab","Examples"),"*.uml") do
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