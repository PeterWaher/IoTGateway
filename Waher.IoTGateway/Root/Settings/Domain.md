﻿Title: Domain Settings
Description: Allows the user to configure domain settings.
Date: 2018-05-28
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Prompt.js
JavaScript: /Settings/Domain.js
JavaScript: /Settings/XMPP.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Privilege: Admin.Communication.Domain
Login: /Login.md

========================================================================

Domain Settings
===================

In the following sections you can configure the use of a domain name, as well as provide
human-readable descriptions of the service provided by the domain.

<form>
<fieldset>
<legend>Domain Name</legend>

By providing and using a domain name, it will be easier to reference and protect the 
application. This is especially important if you plan to make it available publicly on the 
Internet. A domain name must be registered with a *Domain Name Service* (DNS) or a domain 
name registrar. It must also point to the machine where the application is running. If you 
don't use a domain name, you can still access the application, either by using the 
predefined domain name `localhost` from the same machine, or by using the IP address of 
the machine.

<p>
<input type="checkbox" name="UseDomainName" id="UseDomainName" {{ConfigClass:=Waher.IoTGateway.Setup.DomainConfiguration;Config:=ConfigClass.Instance;Config.UseDomainName ? "checked" : ""}} onclick="ToggleDomainNameProperties()"/>
<label for="UseDomainName" title="If a domain name can be used to identify the machine.">Use domain name.</label>
</p>

<div id="DomainNameProperties" style="display:{{Config.UseDomainName ? "block" : "none"}}">

<p>
<label for="DomainName">Domain Name:</label>  
<input id="DomainName" name="DomainName" type="text" style="max-width:20em" title="Domain name used to identify the machine." oninput="DomainNameInput(this)"
	value="{{Config.Domain}}" {{Config.Step=0 ? "autofocus" : ""}}/>
<span id="DomainName2" style="display:none">{{Config.Domain}}</span>
</p>

{{if Config.AlternativeDomains!=null then (Index:=0;foreach AlternativeDomain in Config.AlternativeDomains do (]]
<p>
<label for="AltDomainName((Index))">Alternative Domain Name:</label>  
<input id="AltDomainName((Index))" name="AltDomainName((Index))" type="text" style="max-width:20em" title="Alternative domain name used to identify the machine."
	value="((AlternativeDomain))"/>
<button type="button" class="negButtonSm" onclick="RemoveAltDomainName('((Index))')">Remove</button>
</p>
[[;Index++))}}

<p>
<label for="AltDomainName">Alternative Domain Name:</label>  
<input id="AltDomainName" name="AltDomainName" type="text" style="max-width:20em" title="Alternative domain name used to identify the machine."/>
<button type="button" class="posButtonSm" onclick="AddAltDomainName()">Add</button>
</p>

If your network provider does not provide you with a stable public IP address, you need to dynamically update your IP address entries in the
DNS every time you get a new IP address. This can be automated if the network provider supports a Dynamic DNS service.

<p>
<input type="checkbox" name="DynamicDns" id="DynamicDns" {{Config.DynamicDns ? "checked" : ""}} onclick="ToggleDynamicDnsProperties()"/>
<label for="DynamicDns" title="If a Dynamic DNS service should be used.">Enable Dynamic DNS.</label>
</p>

<div id="DynDnsProperties" style="display:{{Config.DynamicDns ? "block" : "none"}}">

<p>
<label for="DynDnsTemplate">DynDNS Service Template:</label>  
<select id="DynDnsTemplate" name="DynDnsTemplate" style="width:auto" onchange="TemplateChanged(this)">
<option value=""></option>
<option value="DynDnsOrg"{{(Template:=Config.DynDnsTemplate)="DynDnsOrg"?" selected":""}}>DynDns.org</option>
<option value="LoopiaSe"{{Template="LoopiaSe"?" selected":""}}>Loopia.se</option>
</select>
</p>

<label for="CheckIpScript">[Script](https://waher.se/Script.md) returning the current IP Address:</label>  
<textarea id="CheckIpScript" autofocus="autofocus" wrap="hard" onkeydown="return DynamicDnsScriptUpdated(this,event);">{{Config.CheckIpScript}}</textarea>

<label for="UpdateIpScript">[Script](https://waher.se/Script.md) updating an IP Address in the DNS:</label>  
<textarea id="UpdateIpScript" autofocus="autofocus" wrap="hard" onkeydown="return DynamicDnsScriptUpdated(this,event);">{{Config.UpdateIpScript}}</textarea>

**Note**: During the execution of the update script, the following variables are available: `Account` and `Password` contain the account
name and password parameters provided below. The `IP` variable contains the current IP address, as a string. The `Domain` variable
contains the domain name that is to be updated.

<p>
<label for="DynDnsAccount">Dynamic DNS Account:</label>  
<input id="DynDnsAccount" name="DynDnsAccount" type="text" style="max-width:20em" title="Account Name in the Dynamic DNS service." 
	value="{{Config.DynDnsAccount}}"/>
</p>

<p>
<label for="DynDnsPassword">Dynamic DNS Password:</label>  
<input id="DynDnsPassword" name="DynDnsPassword" type="password" style="max-width:20em" title="Password for the Dynamic DNS account." 
	value="{{Config.DynDnsPassword}}"/>
</p>

<p>
<label for="DynDnsInterval">Dynamic DNS Interval (seconds):</label>  
<input id="DynDnsInterval" name="DynDnsInterval" type="number" style="width:10em" title="Interval (in seconds) for checking if the IP address has changed. Make sure to keep the interval within the span recommended by the provider." 
	min="60" max="86400" step="1" value="{{Config.DynDnsInterval}}"/>
</p>

</div>

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
<input id="ContactEMail" name="ContactEMail" type="email" style="max-width:20em" value="{{Config.ContactEMail}}"
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
{{if Configuring then ]]
<button id='NextButton' type='button' onclick='Next()' style='display:((Config.Step>1 ? "inline-block" : "none"))'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}

</div>

<div id="NotEncryptionProperties" style="display:{{Config.UseEncryption ? "none" : "block"}}">
{{if Configuring then ]]
<button type='button' onclick='Next()'>Next</button>
[[ else ]]
<button type='button' onclick='Ok()'>OK</button>
[[;}}
</div>

</div>
</div>

<div id="NotDomainNameProperties" style="display:{{Config.UseDomainName ? "none" : "block"}}">
{{if Configuring then ]]
<button type='button' onclick='Next()'>Next</button>
[[ else ]]
<button type='button' onclick='Ok()'>OK</button>
[[;}}
</div>

<fieldset id="ConnectionStatus" style="display:none">
<legend>Status</legend>
<div id='Status'></div>
</fieldset>
</fieldset>

<fieldset>
<legend>Human-readable Name</legend>

When clients connect to the gateway, they have an option to present a human-readable name
representing the domain, instead of the domain name configured above. Below you can provide
a human-readable name to the server. You can provide different localized names for different
languages.

<p>
<label for="HumanReadableName">Human-readable Name:</label>  
<input id="HumanReadableName" name="HumanReadableName" type="text" title="Human-readable name used to identify the machine."
	value="{{Config.HumanReadableName}}"/>
</p>

<p>
<label for="HumanReadableNameLanguage">Human-readable Name Language:</label>  
<input id="HumanReadableNameLanguage" name="HumanReadableNameLanguage" type="text" style="max-width:20em" title="Language of human-readable name."
	value="{{Config.HumanReadableNameLanguage}}"/>
</p>

{{Index:=0;
if Config.LocalizedNames!=null then 
(
	foreach LocalizedName in Config.LocalizedNames do ]]
<p>
<table>
<tr>
<td>
<label for="Language((++Index))">Language ((Index)):</label>  
<input type="text" id="Language((Index))" value="((LocalizedName.Key))"/>
</td>
<td style="width:65%">
<label for="LocalizedName((Index))">Localized Name ((Index)):</label>  
<input type="text" id="LocalizedName((Index))" value="((LocalizedName.Value))"/>
</td>
<td>
<button type="button" class="negButtonSm" onclick="RemoveLocalizedName( ((Index)) )">Remove</button>
</td>
</tr>
</table>
</p>
[[
)
}}

<button id="AddNameLocalizationButton" type="button" onclick="AddHumanReadableNameLanguage(this)" data-nrNames="{{Index}}">Add Language</button>
<button type="button" onclick="SaveHumanReadableNames()">Save</button>
</fieldset>

<fieldset>
<legend>Human-readable Description</legend>

When clients connect to the gateway, they have an option to present a human-readable description
representing the domain, describing the purpose and role of the server and the service provider
hosting it. Below you can provide a human-readable description for the server. You can provide 
different localized descriptions for different languages.

<p>
<label for="HumanReadableDescription">Human-readable Description:</label>  
<input id="HumanReadableDescription" name="HumanReadableDescription" type="text" title="Human-readable description used to describe the purpose of the domain."
	value="{{Config.HumanReadableDescription}}"/>
</p>

<p>
<label for="HumanReadableDescriptionLanguage">Human-readable Description Language:</label>  
<input id="HumanReadableDescriptionLanguage" name="HumanReadableDescriptionLanguage" type="text" style="max-width:20em" title="Language of human-readable description."
	value="{{Config.HumanReadableDescriptionLanguage}}"/>
</p>

{{Index:=0;
if Config.LocalizedDescriptions!=null then 
(
	foreach LocalizedDescription in Config.LocalizedDescriptions do ]]
<p>
<table>
<tr>
<td>
<label for="LanguageDescription((++Index))">Language ((Index)):</label>  
<input type="text" id="LanguageDescription((Index))" value="((LocalizedDescription.Key))"/>
</td>
<td style="width:65%">
<label for="LocalizedDescription((Index))">Localized Description ((Index)):</label>  
<input type="text" id="LocalizedDescription((Index))" value="((LocalizedDescription.Value))"/>
</td>
<td>
<button type="button" class="negButtonSm" onclick="RemoveLocalizedDescription( ((Index)) )">Remove</button>
</td>
</tr>
</table>
</p>
[[
)
}}

<button id="AddDescriptionLocalizationButton" type="button" onclick="AddHumanReadableDescriptionLanguage(this)" data-nrDescriptions="{{Index}}">Add Language</button>
<button type="button" onclick="SaveHumanReadableDescriptions()">Save</button>
</fieldset>

</form>

