Title: SPARQL Endpoint
Description: SPARQL Endpoint evaluating federated SPARQL queries
Date: 2023-07-01
Author: Peter Waher
Master: /Master.md
Javascript: Sparql.js
CSS: Sparql.css
Parameter: query
Parameter: default-graph-uri
Parameter: named-graph-uri
UserVariable: User
Privilege: Admin.Lab.Script
Login: /Login.md

=====================================================

SPARQL Endpoint
==================

Below, you can enter a SPARQL query and ask the server to evaluate it.
Press `ENTER` evaluate the query you're writing. 
Press `SHIFT`+`ENTER` to add a newline to the query you're editing.
The query editor accepts `TAB` characters.

<form id="QueryForm" action="/sparql" method="post" enctype="application/x-www-form-urlencoded">

Query:  
<textarea id="query" name="query" autofocus="autofocus" wrap="hard" onkeydown="return QueryKeyDown(this,event);">{{query}}</textarea>

<button type="submit">Execute</button>
<button type="button" onclick="ClearAll();">Clear</button>

</form>