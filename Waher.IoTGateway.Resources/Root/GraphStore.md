Title: Graph Store
Description: Semantic Graph store
Date: 2023-07-05
Author: Peter Waher
Master: /Master.md
Javascript: GraphStore.js
Parameter: default
Parameter: graph
UserVariable: User
Privilege: Admin.Graphs.Get
Login: /Login.md

=====================================================

{{
CanAdd:=User.HasPrivilege("Admin.Graphs.Add");
CanUpdate:=User.HasPrivilege("Admin.Graphs.Update");
CanDelete:=User.HasPrivilege("Admin.Graphs.Delete");

if CanAdd or CanUpdate then ]]

Upload Graph
===============

To upload a new semantic graph to the graph store, enter the URI of the graph, select a *Turtle*, *RDF/XML* or *JSON-LD* file, 
and the *upload method* desired, and press the *Upload Graph* button below. This information is also available via the
[*default graph*](/rdf-graph-store?default), in semantic form. You can also browse through the existing graphs below, or
use the [SPARQL Endpoint](/Sparql.md) to query the graphs in the graph store (or external graphs, or a combination of both).

<form>

<p>
Graph URI:  
<input type="tet" id="GraphUri" name="GraphUri" title="URI of graph to upload." required/>
</p>

<p>
File to upload:  
<input type="file" id="GraphFile" name="File" title="Select either a Turtle, RDF/XML or JSON-LD file." accept="text/turtle, application/rdf+xml, application/ld+json" required/>
</p>

<p>
Upload mode:  
<select id="Method" name="Method" title="Select how to handle previous graph with the same URI, if one exists.">
((if CanUpdate then "<option selected value='POST'>Append to existing graph with same URI, if it exists.</option>" else ""))
((if CanAdd then "<option "+(CanUpdate?"":"selected ")+"value='PUT'>Replace existing graph with same URI, if it exists.</option>" else ""))
</select>
</p>

<button type="button" onclick="UploadFile()">Upload Graph</button>

</form>

=====================================================
[[
}}

Graph Store
===============

| Graph URI | Creator\(s) | Created || Updated || \#Files |    |
|:----------|:------------|----:|---:|----:|---:|--------:|:--:|
{{
DTMin:=System.DateTime.MinValue;

DateToStr(DT):=
(
	DT>DTMin ? DT.ToShortDateString() : ""
);

TimeToStr(DT):=
(
	DT>DTMin ? DT.ToLongTimeString() : ""
);

References:=select
	top 20 *
from
	Waher.Script.Persistence.SPARQL.Sources.GraphReference
order by
	Created desc;

foreach Ref in References do
	]]| <a href="/rdf-graph-store?graph=((UrlEncode(Ref.GraphUri) ))" target="_blank">`((Ref.GraphUri))`</a> | ((Concat(Ref.Creators??"",", ") )) | ((DateToStr(Ref.Created) )) | ((TimeToStr(Ref.Created) )) | ((DateToStr(Ref.Updated) )) | ((TimeToStr(Ref.Updated) )) | ((Ref.NrFiles)) | ((CanDelete ? "<button type='button' class='negButtonSm' onclick='DeleteGraph(this,\""+Ref.GraphUri+"\")'>Delete</button>" : "")) |
[[;

if count(References)==20 then ]]

<button id="LoadMoreButton" class='posButton' type="button" onclick='LoadMore(this,20,20)'>Load More</button>
[[
}}
