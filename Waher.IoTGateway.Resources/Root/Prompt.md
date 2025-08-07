﻿Title: Prompt
Description: Prompt allowing the operator to evaluate script.
Date: 2016-03-01
Author: Peter Waher
Master: /Master.md
JavaScript: Prompt.js
Parameter: Expression
UserVariable: User
Privilege: Admin.Lab.Script
Login: /Login.md

Script prompt
=============================

Below, you can enter script and ask the server to evaluate it. You can review the [script reference page](Script.md) for more information about 
script syntax. Press `ENTER` evaluate the script you're writing. Press `SHIFT`+`ENTER` to add a newline to the script you're editing. The script 
editor accepts `TAB` characters.

Script:  
<textarea id="script" autofocus="autofocus" wrap="hard" onkeydown="return ScriptKeyDown(this,event);">{{Expression}}</textarea>

<button type="submit" onclick="EvaluateExpression();">Evaluate</button>
<button type="button" onclick="ClearAll();">Clear</button>
<button type="button" onclick="ListVariables();">Variables</button>

============================================================================================================================================

<div id="Results"></div>