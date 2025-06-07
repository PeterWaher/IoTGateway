Title: Login
Description: {{Waher.IoTGateway.Gateway.ApplicationName}} login page.
Date: 2016-12-25
Author: Peter Waher
Master: /Master.md
Javascript: Login.js
Javascript: /Events.js
Parameter: from
Neuron:
{{GW:=Waher.IoTGateway.Gateway;Domain:=empty(GW.Domain) ? (x:=Before(After(GW.GetUrl("/"),"://"),"/");if contains(x,":") and exists(number(after(x,":"))) then "localhost:"+after(x,":") else "localhost") : GW.Domain}}

{{
	LoginMethod := Request.Header.Cookie["login-method"] ?? "";
	Waher.IoTGateway.Gateway.CheckLocalLogin(Request);
}}

<section id="LoginContainer" class="flex-centering">
	<h1>Login</h1>

	<p>You need to login to proceed.</p>
	<div class="native-carousel" id="login-carousel">
		<form id="LoginForm" action="/Login" method="post" data-login-method="user-password" {{ LoginMethod = "user-password" ? "data-carousel-active" : "" }}>
			<div>
				<p>User Name:</p>
				<input id="UserName" name="UserName" type="text" autofocus="autofocus" style="max-width:20em" />
			</div>
			<div>
				<p>Password:</p>
				<input id="Password" name="Password" type="password" style="max-width:20em" />
			</div>
			<button id="LoginButton" type="submit">Login</button>

		</form>

		{{if exists(QuickLoginServiceId) and Waher.IoTGateway.Setup.LegalIdentityConfiguration.Instance.HasApprovedLegalIdentities then
		(
		]]
		<div data-login-method="quick-login" ((LoginMethod = "quick-login" ? "data-carousel-active" : ""))>
			<h2 class="text-center">Neuro-Access Login</h2>
			<div id="quickLoginCode" style="margin-block: 2rem" data-mode="image" data-serviceId="((QuickLoginServiceId(Request) ))" 
			data-purpose="To login on ((Domain)), for administrative purposes. This login request is valid for five (5) minutes."><img id="quickLoginImg" height="400" width="400"/></div>
		</div>
		[[;
		)}}
	</div>


	<span>
		{{if exists(QuickLoginServiceId) and Waher.IoTGateway.Setup.LegalIdentityConfiguration.Instance.HasApprovedLegalIdentities then
		(
		]]
		<button type="button" data-carousel="login-carousel" data-carousel-button=0><img draggable="false" (dragstart)="false;" class="unselectable" src="/Images/user-password-login-icon.svg"></button>
		<button type="button" data-carousel="login-carousel" data-carousel-button=1><img draggable="false" (dragstart)="false;" class="unselectable" src="/Images/quick-login-login-icon.svg"></button>
		[[
		)
		}}
	</span>
	{{if exists(LoginError) then]]
<div class='error'>
	<p>((LoginError))</p>
</div>
[[;}}

</section>