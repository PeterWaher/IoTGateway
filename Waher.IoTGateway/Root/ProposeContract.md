Title: Propose Contract
Description: This page allows the user to propose a new contract.
Date: 2020-06-26
Author: Peter Waher
Master: /Master.md
JavaScript: Events.js
JavaScript: ProposeContract.js
UserVariable: User
Login: /Login.md

================================================================

Propose a new Smart Contract
===============================

{{if !exists(CC:=Waher.IoTGateway.Gateway.ContractsClient) then ]]
You are not connected to a broker that supports smart contracts. For this reason, proposing new smart contracts has been disabled.
[[ else if exists(Page.Contract) then
(
	OneRow(s):=
	(
		s.Trim().Replace("\r\n","\n").Replace("\n","<br/>");
	);

	SignatureStyle(Signature):=
	(
		"<div style='overflow-wrap:break-word;max-width:20em;'>" + Base64Encode(Signature) + "</div>";
	);

	Languages:=Page.Contract.GetLanguages();
	if !exists(Languages) or Languages.Length=0 then
		Languages:=[Page.Contract.DefaultLanguage];

	foreach Language in Languages do
	(
		]]

<fieldset>
<legend>((Translator.GetLanguageAsync(Language).Result?.Name ?? Language))</legend>

Roles
----------

| Name | Min | Max | Description |
|:-----|----:|----:|:------------|[[;

		foreach Role in Page.Contract.Roles do
			]]
| ((MarkdownEncode(Role.Name) )) | ((Role.MinCount)) | ((Role.MaxCount)) | ((OneRow(Role.ToMarkdown(Language, Page.Contract) ) )) |[[;

		]]

Parts
----------

Parts Mode: **((Page.Contract.PartsMode))**

| Legal ID | Role |
|:---------|:-----|[[;

		foreach Part in (Page.Contract.Parts ?? []) do
			]]
| `((Part.LegalId))` | ((MarkdownEncode(Part.Role) )) |[[;

		]]

Parameters
-----------

| Name | Value | Description |
|:-----|:------|:------------|[[;

		foreach Parameter in (Page.Contract.Parameters ?? []) do
			]]
| **((MarkdownEncode(Parameter.Name) ))** | ((Parameter.ObjectValue)) | ((OneRow(Parameter.ToMarkdown(Language, Page.Contract) ) )) |[[;

		]]

Legal Text
-------------

((Page.Contract.ToMarkdown(Language) ))

</fieldset>
[[
	);

	]]

Machine-Readable Information
-------------------------------

```xml

((Page.Contract.ForMachines?.OuterXml))

```

	[[;

	if exists(Page.Contract.ServerSignature) then
		]]

Contract has been proposed. The state of the contract is now managed by `((Gateway.ContractsClient.ComponentAddress))`.

<button type="button" class="posButton" onclick='window.close()' title="Close window">OK</button>

[[
	else
		]]

<br/>
<button type="button" class="posButton" onclick='ProposeContract()' title="Propose Contract File">Propose</button>
<button type="button" class="negButton" onclick='CancelContract()' title="Cancel Contract File">Cancel</button>

	[[
)
else ]]
You can propose a new smart contract by uploading it to `((CC.ComponentAddress))`. Select the contract file below, and click `Upload`.

<fieldset>
<legend>Upload Smart Contract</legend>
<form action="ProposeContract" method="POST">

<p>
<label for="ContractFile">Contract File:</label>  
<input id="ContractFile" name="ContractFile" type="file" title="Contract File to upload." accept="application/xml"/>
</p>

<br/>
<button type="button" class="posButton" onclick='UploadContract()' title="Upload Contract File">Upload</button>

</form>
</fieldset>
[[}}
