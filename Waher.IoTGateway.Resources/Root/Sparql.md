Title: SPARQL Endpoint
Description: SPARQL Endpoint evaluating federated SPARQL queries
Date: 2023-07-01
Author: Peter Waher
Master: /Master.md
JavaScript: Sparql.js
CSS: Sparql.css
Parameter: query
Parameter: default-graph-uri
Parameter: named-graph-uri
UserVariable: User
Privilege: Admin.Graphs.Query
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
<textarea id="query" name="query" autofocus="autofocus" wrap="hard" onkeydown="return QueryKeyDown(this,event);">{{
if empty(query) then
(
	Ontologies:=[foreach T in Waher.Runtime.Inventory.Types.GetTypesImplementingInterface(IOntology):Create(T)];
	Ontologies:=Sort(Ontologies,(o1,o2)->o1.OntologyPrefix.CompareTo(o2.OntologyPrefix));
	Prefixes:=Ontologies.OntologyPrefix;
	MaxLen:=max(len(Prefixes));
	MaxLen+=2;
	Spaces:=Create(System.String,' ',MaxLen);
	foreach Ontology in Ontologies do if Ontology.ShowByDefault then
		]]PREFIX ((Ontology.OntologyPrefix)):((left(Spaces,MaxLen-Len(Ontology.OntologyPrefix) ) ))<((Ontology.OntologyNamespace))>
[[;
	]]
SELECT [[
)
else
	]]((query))[[
}}</textarea>

Default Graph\(s): (Will be loaded before executing script. Can be omitted if specified in the query.)  
<input type="text" id="defaultGraph1" name="default-graph-uri"/>

Named Graph\(s): (Only loaded if used in query. Can be omitted if specified in the query.)  
<input type="text" id="namedGraph1" name="named-graph-uri"/>

Return results as:  
<select id="ReturnType">
<option selected value="Xml">XML</option>
<option value="Json">JSON</option>
<option value="Csv">CSV</option>
<option value="Tsv">TSV</option>
<option value="Html">HTML</option>
<option value="Text">Text</option>
{{if exists(TAG.Content.Microsoft.Content.ExcelCodec) then ]]<option value="Excel">Excel</option>[[}}
</select>

<button type="button" onclick="ExecuteQuery()">Execute</button>
<button type="button" onclick="ClearAll();">Clear</button>
<button type="button" onclick="AddDefaultGraph();">Add Default Graph</button>
<button type="button" onclick="AddNamedGraph();">Add Named Graph</button>

</form>

<fieldset id="Result" style="display:none">
<legend>Result</legend>
</fieldset>
