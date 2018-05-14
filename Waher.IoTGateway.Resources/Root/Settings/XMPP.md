Title: XMPP Settings
Description: Allows the user to configure XMPP settings.
Date: 2018-05-10
Author: Peter Waher
Copyright: /Copyright.md
Master: ..\Master.md
JavaScript: /Events.js
JavaScript: /Settings/XMPP.js
CSS: /Settings/XMPP.css


========================================================================

IoT Gateway Setup
=============================

<form>
<fieldset>
<legend>XMPP settings</legend>

The IoT Gateway requires a connection to an XMPP network to work properly. The XMPP connection allows you to configure, monitor and administer your
IoT Gateway in a secure manner, from anywhere where you have access to the same XMPP network. If the server has provisioning support, it will allow 
for additional security for embedded devices. Enter the domain name of the XMPP server you wish to use. Or click on any of the featured servers below.
You can also use any of the free public servers listed at [xmpp.net](https://xmpp.net/directory.php). If you're an expert user, you can also setup your
own XMPP server. [xmpp.org](https://xmpp.org/software/servers.html) publishes an abridged list of XMPP server software you can use.

<p>
<label for="XmppServer">Server:</label>  
<input id="XmppServer" name="XmppServer" type="text" style="width:20em" title="Name of server that hosts the XMPP server."
	value="{{ConfigClass:=Waher.IoTGateway.Setup.XmppConfiguration;Config:=ConfigClass.Instance;Config.Host}}" {{Config.Step=0 ? "autofocus" : ""}}/>
</p>

<p id="XmppServerError" class="error" style="display:none">You must select an XMPP server.</p>

Featured servers:

<div class="featuredServers">
{{foreach Server in ConfigClass.FeaturedServers do
(
	]]<button type='button' class='featured' onclick='SelectServer("((Server))")'>((Server))</button>
[[)}}
</div>

<p>
<input type="checkbox" name="Custom" id="Custom" title="If custom binding properties are required." {{Config.CustomBinding ? "checked" : ""}} onclick="ToggleCustomProperties()"/>
<label for="Custom">Custom connection properties.</label>
</p>

<div id="CustomProperties" style="display:{{Config.CustomBinding ? "block" : "none"}}">

<p>
<input type="checkbox" name="TrustServer" id="TrustServer" title="If invalid server sertificates is acceptable." {{Config.TrustServer ? "checked" : ""}} />
<label for="TrustServer">Trust invalid server certificates.</label>
</p>

<p>
<input type="checkbox" name="Sniffer" id="Sniffer" title="If XMPP communication is to be logged." {{Config.Sniffer ? "checked" : ""}} />
<label for="Sniffer">Log XMPP communication to the application data folder.</label>
</p>

<p>
<label for="Transport">Transport:</label>  
<select id="Transport" name="Transport" style="width:auto" onchange="ToggleTransport()">
<option value="C2S"{{(TransportMethod:=Config.TransportMethod.ToString())="C2S" ? " selected" : ""}}>TCP</option>
<option value="BOSH"{{TransportMethod="BOSH" ? " selected" : ""}}>HTTP</option>
</select>
</p>

<div id="C2S" style="display:{{TransportMethod="C2S" ? "block" : "none"}}">
<p>
<label for="Port">Port:</label>  
<input id="Port" name="Port" type="number" min="1" max="65535" style="width:20em" value="{{Config.Port}}" />
</p>
<p id="PortError" class="error" style="display:none">Invalid port number.</p>
</div>

<div id="BOSH" style="display:{{TransportMethod="BOSH" ? "block" : "none"}}">
<p>
<label for="BoshUrl">URL:</label>  
<input id="BoshUrl" name="BoshUrl" type="url" style="width:40em" value="{{Config.BoshUrl}}" />
</p>
<p id="BoshUrlError" class="error" style="display:none">You must provide a URL to connect to.</p>
</div>

</div>

<div id="Credentials" style="display:{{Config.Step>0 ? "block" : "none"}}">

<p id="Success0" class="message" style="display:none">
Good. Successfully connected to server. Now, please provide user credentials.
</p>

<p>
<label for="Account">Account:</label>  
<input id="Account" name="Account" type="text" style="width:20em" value="{{Config.Account}}" {{Config.Step=1 ? "autofocus" : ""}}/>
</p>

<p>
<label for="Password">Password:</label>  
<input id="Password" name="Password" type="password" style="width:20em" value="{{Config.Password}}" />
</p>

<p id="Fail1" class="error" style="display:none">
Account does not exist or password is incorrect. If the account does not exist, you can try to create it by checking the box below.
</p>
<p id="Fail2" class="error" style="display:none">
Password is incorrect, or a new account was not permitted to be created.
</p>

<p>
<input type="checkbox" name="CreateAccount" id="CreateAccount" title="If an account should be created on the server." {{Config.CreateAccount ? "checked" : ""}} onclick="ToggleCreateAccount()"/>
<label for="CreateAccount">Create account on server.</label>
</p>

<div id="Create" style="display:{{Config.CreateAccount ? "block" : "none"}}">

<p>
<label for="Password2">Repeat password:</label>  
<input id="Password2" name="Password2" type="password" style="width:20em" value="{{Config.Password}}" />
<button type='button' onclick='RandomizePassword()'>Randomize password</button>
</p>
<p id="Password2Error" class="error" style="display:none">Passwords do not match.</p>

<p>
<label for="AccountName">Readable name:</label>  
<input id="AccountName" name="AccountName" type="text" style="width:20em" value="{{Config.AccountHumanReadableName}}" />
</p>

</div>
</div>

<p id="ConnectMessage">Press the Connect button to try the connection.</p>
<p id="ConnectError" class="error" style="display:none">Unable to connect to the server. Please verify your connection details and try again.</p>
<p id="NextMessage" class="message" style="display:none">Connection successful. Press the Next button to save settings and continue.</p>

<button type='button' onclick='ConnectToHost()'>Connect</button>
<button id='NextButton' type='button' onclick='Next()' style='display:{{Config.Step>1 ? "inline" : "none"}}'>Next</button>

</fieldset>

<fieldset id="ServerFeatures" style="display:{{Config.Step>1 ? "block" : "none"}}">
<legend>Server features</legend>

| Feature | Support | JID |
|:--------|:-------:|:---:|
| Offline messages | <span id="OfflineMessages">{{Config.OfflineMessages ? "✓" : ""}}</span> | N/A |
| Blocking | <span id="Blocking">{{Config.Blocking ? "✓" : ""}}</span> | N/A |
| Reporting | <span id="Reporting">{{Config.Reporting ? "✓" : ""}}</span> | N/A |
| Abuse reporting | <span id="AbuseReporting">{{Config.Abuse ? "✓" : ""}}</span> | N/A |
| Spam reporting | <span id="SpamReporting">{{Config.Spam? "✓" : ""}}</span> | N/A |
| Thing Registry | <span id="ThingRegistry">{{empty(Config.ThingRegistry) ? "" : "✓"}}</span> | <span id="ThingRegistryJID">{{Config.ThingRegistry}}</span> |
| Provisioning | <span id="Provisioning">{{empty(Config.Provisioning) ? "" : "✓"}}</span> | <span id="ProvisioningJID">{{Config.Provisioning}}</span> |
| Publish/Subscribe | <span id="PubSub">{{empty(Config.PubSub) ? "" : "✓"}}</span> | <span id="PubSubJID">{{Config.PubSub}}</span> |

</fieldset>

<fieldset id="ConnectionStatus" style="display:none">
<legend>Connection Status</legend>
<div id='Status'></div>
</fieldset>

</form>

