Title: Calculator
Description: Calculator based on script, as understood by the IoT Gateway.
Date: 2016-03-01
Author: Peter Waher
Copyright: Copyright.md
Master: Master.md
JavaScript: Calculator.js

Script calculator
=============================

Below, you can enter script and ask the server to evaluate it, to test script syntax. You can also review the [script reference page](Script.md)
for more information about script syntax. Press `ENTER` evaluate the script you're writing. Press `SHIFT`+`ENTER` to add a newline to the script
you're editing. The script editor accepts `TAB` characters.

Script:  
<textarea id="script" autofocus="autofocus" wrap="hard" onkeydown="return ScriptKeyDown(this,event);">
</textarea>

<button type="submit" onclick="EvaluateExpression();">Evaluate</button>
<button type="button" onclick="ClearAll();">Clear</button>
<button type="button" onclick="ListVariables();">Variables</button>

============================================================================================================================================

<div id="Results"></div>