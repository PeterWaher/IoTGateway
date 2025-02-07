Title: Login
Description: {{Waher.IoTGateway.Gateway.ApplicationName}} login page.
Date: 2016-12-25
Author: Peter Waher
Master: /Master.md
Javascript: Login.js
Javascript: /Events.js
Parameter: from
Neuron: {{GW:=Waher.IoTGateway.Gateway;Domain:=empty(GW.Domain) ? (x:=Before(After(GW.GetUrl("/"),"://"),"/");if contains(x,":") and exists(number(after(x,":"))) then "localhost:"+after(x,":") else "localhost") : GW.Domain}}

============================================================================================================================================

Login
=============

{{Waher.IoTGateway.Gateway.CheckLocalLogin(Request)}}

<form id="LoginForm" action="/Login" method="post">

You need to login to proceed.

User Name:  
<input id="UserName" name="UserName" type="text" autofocus="autofocus" style="max-width:20em" />

Password:  
<input id="Password" name="Password" type="password" style="max-width:20em" />

{{if exists(LoginError) then]]
<div class='error'>
((LoginError))
</div>
[[;}}

<button id="LoginButton" type="submit">Login</button>

{{if exists(QuickLoginServiceId) and Waher.IoTGateway.Setup.LegalIdentityConfiguration.Instance.HasApprovedLegalIdentities then
(
	]]
Neuro-Access Login
======================

<div id="quickLoginCode" data-mode="image" data-serviceId="((QuickLoginServiceId(Request) ))" 
data-purpose="To login on ((Domain)), for administrative purposes. This login request is valid for five (5) minutes."></div>

[[;
)}}

</form>
