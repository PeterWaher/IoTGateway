Title: Signature Request
Description: This page displays a request to sign a smart contract.
Date: 2020-02-28
Author: Peter Waher
Master: /Master.md
JavaScript: Events.js
JavaScript: SignatureRequest.js
JavaScript: Settings/Next.js
UserVariable: User
Privilege: Admin.Legal.SignatureRequests
Login: /Login.md
Parameter: RequestId

================================================================

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

A contract has been received for signature
===============================================

| `{{
if (try Page.Request.ObjectId!=RequestId catch true) then
(
	Page.Request:=select top 1 * from ContractSignatureRequest where ObjectId=RequestId;
	if !exists(Page.Request) then NotFound("Signature Request not found.");
);
Contract:=Page.Request.Contract;
Contract.ContractId}}` ||
|:-------------|:---------------|
| Received:    | {{MarkdownEncode((Page.Request.Received?.ToShortDateString() ?? "") + (Page.Request.Received?.ToLongTimeString() ?? ""))}} |
| Role:        | {{MarkdownEncode(Page.Request.Role)}} |
| Purpose:     | {{MarkdownEncode(Page.Request.Purpose)}} |
| Provider:    | `{{Contract.Provider}}` |
| Module:      | `{{Page.Request.Module}}` |

{{
OneRow(s):=
(
	s.Trim().Replace("\r\n","\n").Replace("\n","<br/>");
);

SignatureStyle(Signature):=
(
	"<div style='overflow-wrap:break-word;max-width:20em;'>" + Base64Encode(Signature) + "</div>";
);

Languages:=Contract.GetLanguages();
if !exists(Languages) or Languages.Length=0 then
	Languages:=[Contract.DefaultLanguage];

foreach Language in Languages do
(
	]]

<fieldset>
<legend>((Translator.GetLanguageAsync(Language).Result?.Name ?? Language))</legend>

Roles
----------

| Name | Min | Max | Description |
|:-----|----:|----:|:------------|[[;

	foreach Role in Contract.Roles do
		]]
| ((MarkdownEncode(Role.Name) )) | ((Role.MinCount)) | ((Role.MaxCount)) | ((OneRow(Role.ToMarkdown(Language, Contract) ) )) |[[;

	]]

Parts
----------

Parts Mode: **((Contract.PartsMode))**

| Legal ID | Role |
|:---------|:-----|[[;

	foreach Part in (Contract.Parts ?? []) do
		]]
| `((Part.LegalId))` | ((MarkdownEncode(Part.Role) )) |[[;

	]]

Parameters
-----------

| Name | Value | Description |
|:-----|:------|:------------|[[;

	foreach Parameter in (Contract.Parameters ?? []) do
		]]
| **((MarkdownEncode(Parameter.Name) ))** | ((Parameter.ObjectValue)) | ((OneRow(Parameter.ToMarkdown(Language, Contract) ) )) |[[;

	]]

Legal Text
-------------

((Contract.ToMarkdown(Language) ))

</fieldset>
[[
);

]]

Machine-Readable Information
-------------------------------

```xml

((Contract.ForMachines?.OuterXml))

```

Client Signatures
--------------------

| Legal ID | Bare JID | Role | Transferable | Date | Time | Signature |
|:---------|:---------|:-----|:------------:|:-----|:-----|:----------|[[;

foreach S in (Contract.ClientSignatures ?? []) do
	]]
| `((S.LegalId))` | `((S.BareJid))` | ((MarkdownEncode(S.Role) )) | ((S.Transferable ? "✔" : "✗")) | ((MarkdownEncode(S.Timestamp.ToShortDateString() ) )) | ((MarkdownEncode(S.Timestamp.ToLongTimeString() ) )) | ((SignatureStyle(S.DigitalSignature) )) |[[;

]]

Server Signature
------------------

| Date | Time | Signature |
|:-----|:-----|:----------|[[;

if exists(S:=Contract.ServerSignature) then
	]]
| ((MarkdownEncode(S.Timestamp.ToShortDateString() ) )) | ((MarkdownEncode(S.Timestamp.ToLongTimeString() ) )) | ((SignatureStyle(S.DigitalSignature) )) |[[
}}

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Signature
==========

{{if !exists(Page.Request.Signed) then
(
	]]
<form id="SignatureForm">

If you want to sign the contract as **((MarkdownEncode(Page.Request.Role) ))**, click the *Sign* button below.
You can choose to reject the contract, by pressing the *Reject* button below.[[;

	if (Protect:=LegalIdentityConfiguration.Instance.ProtectWithPassword) then
		]] The legal identity used for signatures is protected by a password. You will also need to provide the password below.

Password:  
<input id="Password" name="Password" type="password" style="width:20em" />
[[;
	]]

<button type="button" onclick="ContractAction('((RequestId))',((Protect?"true":"false")),true)">Sign</button>
<button type="button" onclick="ContractAction('((RequestId))',((Protect?"true":"false")),false)">Reject</button>

</form>[[
)
else
	]]The contract was signed ((Page.Request.Signed.ToShortDateString() )) at ((Page.Request.Signed.ToLongTimeString() )).[[
}}
