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

{{Waher.IoTGateway.Gateway.CheckLocalLogin(Request)}}

<section id="LoginContainer" class="flex-centering">
	<h1>Login</h1>

	<p>You need to login to proceed.</p>
	<div class="native-carousel" id="login-carousel">
		<form id="LoginForm" action="/Login" method="post" data-login-method="user-password">
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
		<div data-login-method="quick-login">
			<p class="text-center">Neuro-Access Login</p>
			<div id="quickLoginCode" data-mode="image" data-serviceId="((QuickLoginServiceId(Request) ))" 
			data-purpose="To login on ((Domain)), for administrative purposes. This login request is valid for five (5) minutes."></div>
		</div>
		[[;
		)}}
	</div>


	<span>
		<button type="button" data-carousel="login-carousel" data-carousel-previous>&lt;</button>
		<button type="button" data-carousel="login-carousel" data-carousel-next>&gt;</button>

	</span>
	{{if exists(LoginError) then]]
<div class='error'>
	<p>((LoginError))</p>
</div>
[[;}}

</section>