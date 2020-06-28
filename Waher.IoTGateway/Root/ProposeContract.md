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

{{if exists(CC:=Waher.IoTGateway.Gateway.ContractsClient) then ]]
You can propose a new smart contract by uploading it to `((CC.ComponentAddress))`. Select the contract file below, and click `Upload`.

<fieldset>
<legend>Upload Smart Contract</legend>
<form action="ProposeContract" method="POST">

<p>
<label for="ContractFile">Contract File:</label>  
<input id="ContractFile" name="ContractFile" type="file" title="Contract File to upload." accept="application/xml"/>
</p>

<br/>
<button type="button" onclick='UploadContract()' title="Upload Contract File">Upload</button>

<div id="Result"/>

</form>
</fieldset>
[[ else ]]
You are not connected to a broker that supports smart contracts. For this reason, proposing new smart contracts has been disabled.
[[}}
