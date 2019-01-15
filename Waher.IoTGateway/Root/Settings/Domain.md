Title: Domain Settings
Description: Allows the user to configure domain settings.
Date: 2018-05-28
Author: Peter Waher
Copyright: /Copyright.md
Master: Master.md
JavaScript: /Events.js
JavaScript: /Settings/Domain.js
JavaScript: /Settings/XMPP.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Login: /Login.md

========================================================================

Domain Name
===================

By providing and using a domain name, it will be easier to reference and protect the application. This is especially important if you plan to make it
available publicly on the Internet. A domain name must be registered with a *Domain Name Service* (DNS) or a domain name registrar. It must also point to the
machine where the application is running. If you don't use a domain name, you can still access the application, either by using the predefined domain name 
**localhost** from the same machine, or by using the IP address of the machine.

<form>
<fieldset>
<legend>Domain Settings</legend>

<p>
<input type="checkbox" name="UseDomainName" id="UseDomainName" {{ConfigClass:=Waher.IoTGateway.Setup.DomainConfiguration;Config:=ConfigClass.Instance;Config.UseDomainName ? "checked" : ""}} onclick="ToggleDomainNameProperties()"/>
<label for="UseDomainName" title="If a domain name can be used to identify the machine.">Use domain name.</label>
</p>

<div id="DomainNameProperties" style="display:{{Config.UseDomainName ? "block" : "none"}}">

<p>
<label for="DomainName">Domain Name:</label>  
<input id="DomainName" name="DomainName" type="text" style="width:20em" title="Domain name used to identify the machine." oninput="DomainNameInput(this)"
	value="{{ConfigClass:=Waher.IoTGateway.Setup.DomainConfiguration;Config:=ConfigClass.Instance;Config.Domain}}" {{Config.Step=0 ? "autofocus" : ""}}/>
<span id="DomainName2" style="display:none">{{Config.Domain}}</span>
</p>

{{if Config.AlternativeDomains!=null then (Index:=0;foreach AlternativeDomain in Config.AlternativeDomains do (]]
<p>
<label for="AltDomainName((Index))">Alternative Domain Name:</label>  
<input id="AltDomainName((Index))" name="AltDomainName((Index))" type="text" style="width:20em" title="Alternative domain name used to identify the machine."
	value="((AlternativeDomain))"/>
<button type="button" class="negButtonSm" onclick="RemoveAltDomainName('((Index))')">Remove</button>
</p>
[[;Index++))}}

<p>
<label for="AltDomainName">Alternative Domain Name:</label>  
<input id="AltDomainName" name="AltDomainName" type="text" style="width:20em" title="Alternative domain name used to identify the machine."/>
<button type="button" class="posButtonSm" onclick="AddAltDomainName()">Add</button>
</p>

<p>Press the Test button to test the domain name.</p>
<p id="TestError" class="error" style="display:none">Unable to connect to and validate domain name <b id="InvalidDomainName"></b>. Please verify it is correct, and try again.</p>
<p id="NextMessage" class="message" style="display:none">Domain names successfully verified.</p>

<button type='button' onclick='TestNames()'>Test</button>

<div id="Encryption" style="display:{{Config.Step>0?"block":"none"}}">

With valid domain names, you can choose to activate web server-side encryption for added security. This allows web clients to validate they are indeed communicating
with this server. To disable server-side encryption, uncheck the following checkbox[^This does not affect database encryption or end-to-end encryption. 
It only affects client-server communication.].

<p>
<input type="checkbox" name="UseEncryption" id="UseEncryption" {{Config.UseEncryption ? "checked" : ""}} onclick="ToggleEncryptionProperties()"/>
<label for="UseEncryption" title="If server-side encyption should be used.">Enable server-side encryption.</label>
</p>

<div id="EncryptionProperties" style="display:{{Config.UseEncryption ? "block" : "none"}}">

Server-side encryption requires the use of a valid certificate that encodes the above domain names. Such a certificate will be created automatically for you
by a Certificate Authority (CA). Such an authority might require a contact e-mail address in case they need to contact you. You can provide such an address
below. By default, [Let's Encrypt](https://letsencrypt.org/) will be used to generate valid certificates. You can choose another Certificate Authority if you
want. You do this by checking the box below.

<p>
<label for="ContactEMail">Contact e-mail address:</label>  
<input id="ContactEMail" name="ContactEMail" type="email" style="width:20em" value="{{Config.ContactEMail}}"
	title="Contact e-mail address to be used in communication with the Certificate Authority."/>
</p>

<p id="ToSParagraph" style="display:{{Config.HasToS?"block":"none"}}">
<input type="checkbox" name="AcceptToS" id="AcceptToS" {{Config.AcceptToS ? "checked" : ""}}/>
<label for="AcceptToS" title="If the CA requirers the acceptance of a Terms of Service agreement.">Accept <a id="ToS" target="_blank" href="{{Config.UrlToS}}">CA Terms of Service</a>.</label>
</p>

<p>
<input type="checkbox" name="CustomCA" id="CustomCA" {{Config.CustomCA ? "checked" : ""}} onclick="ToggleCustomCAProperties()"/>
<label for="CustomCA" title="If a custom Certificate Authority is to be used.">Use a custom Certificate Authority (CA).</label>
</p>

<div id="CustomCAProperties" style="display:{{Config.CustomCA ? "block" : "none"}}">

You can use any Certificate Authority that publishes an *Automatic Certificate Management Environment* (ACME). Check with your CA that they do. All you need
to provide is the URL to their ACME Directory below.

<p>
<label for="AcmeDirectory">URL to ACME Directory:</label>  
<input id="AcmeDirectory" name="AcmeDirectory" type="text" title="URL to the ACME directory of the Certificate Authority you wish to use."
	value="{{Config.AcmeDirectory}}"/>
</p>

</div>

<p>Press the Test button to test access to the Certificate Authority. An attempt to create a certificate will be made.</p>
<p id="PleaseWait" style="display:none" class="message">Please wait while the machine attempts to create a certificate.</p>
<p id="CertificateError" class="error" style="display:none"></p>
<p id="NextMessage2" class="message" style="display:none">Certificate for the server successfully created.</p>

<button id='TestAcmeButton' type='button' onclick='TestAcme()'>Test</button>
{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button id='NextButton' type='button' onclick='Next()' style='display:{{Config.Step>1 ? "inline-block" : "none"}}'>Next</button>
[[ else ]]
<button id='OkButton' type='button' onclick='Ok()'>OK</button>
[[;}}

</div>

<div id="NotEncryptionProperties" style="display:{{Config.UseEncryption ? "none" : "block"}}">
{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button type='button' onclick='Next()'>Next</button>
[[ else ]]
<button id='OkButton' type='button' onclick='Ok()'>OK</button>
[[;}}
</div>

</div>
</div>

<div id="NotDomainNameProperties" style="display:{{Config.UseDomainName ? "none" : "block"}}">
{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button type='button' onclick='Next()'>Next</button>
[[ else ]]
<button id='OkButton' type='button' onclick='Ok()'>OK</button>
[[;}}
</div>

</fieldset>

<fieldset id="ConnectionStatus" style="display:none">
<legend>Status</legend>
<div id='Status'></div>
</fieldset>

</form>

