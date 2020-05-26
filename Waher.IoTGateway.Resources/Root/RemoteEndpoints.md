Title: Remote Endpoints
Description: Page containing a list of registered remote endpoints.
Date: 2020-05-25
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
Script: /Controls/SimpleTable.script
UserVariable: User
Login: /Login.md
Parameter: BlockedOnly

================================================================================================================================

Remote Endpoints
===================

{{PrepareTable(()->
(
	Page.Order:="Endpoint";
	if BlockedOnly then
		select * from Waher.Security.LoginMonitor.RemoteEndpoint where Blocked=true order by Endpoint
	else
		select * from Waher.Security.LoginMonitor.RemoteEndpoint order by Endpoint
))}}

The following table contains {{BlockedOnly?"blocked":""}} remote endpoints.

| {{Header("Endpoint","Endpoint")}} | {{Header("Created","Created")}} | {{Header("Last Protocol","LastProtocol")}} | {{Header("Blocked","Blocked")}} | {{Header("Last Failed","LastFailed")}} | {{Header("City","City")}} | {{Header("Region","Region")}} | {{Header("Country","Country")}} | {{Header("Code","Code")}} | {{Header("Flag","Flag")}} |
|:----------|:------:|:-------|:-------:|:----------:|:-----|:-----|:-----|:-----|:-----|
{{foreach EP in Page.Table do
	]]| <a target="_blank" href="/RemoteEndpoint.md?EP=((EP.Endpoint))">((EP.Endpoint))</a> | ((MarkdownEncode(EP.Created.ToShortDateString() ) )) | ((EP.LastProtocol)) | ((EP.Blocked ? "✔" : "✗")) | ((EP.LastFailed)) | ((MarkdownEncode(EP.City) )) | ((MarkdownEncode(EP.Region) )) | ((MarkdownEncode(EP.Country) )) | ((MarkdownEncode(EP.Code) )) | ((EP.Flag)) |
[[}}
