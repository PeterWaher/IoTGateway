Title: Legal Identity Settings
Description: Allows the user to configure a legal identity.
Date: 2020-02-10
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Settings/LegalIdentity.js
JavaScript: /Settings/XMPP.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Login: /Login.md

========================================================================

Legal Identity
===================

By assigning a legal identity to **{{Waher.IoTGateway.Gateway.ApplicationName}}**, it can cryptographically sign smart contracts and other
types of information. Third parties with access to such signatures have a secure manner to validate them, and the legal identities behind the 
signatures.

You can apply for a legal identity below. The information is sent to your broker operator (i.e. *Trust Provider*) for approval. Depending
on which approval procedure used, you might have to take additional steps before the identity can be approved. Contact your Trust Provider for
more information about their approval procedure.

<form>
<fieldset>
<legend>Legal Identity Application</legend>

<p>
<input type="checkbox" name="UseLegalIdentity" id="UseLegalIdentity" {{ConfigClass:=Waher.IoTGateway.Setup.LegalIdentityConfiguration;Config:=ConfigClass.Instance;Config.UseLegalIdentity ? "checked" : ""}} onclick="ToggleLegalIdentityProperties()"/>
<label for="UseLegalIdentity" title="If a legal identity should be assigned to the system.">Use legal identity.</label>
</p>

<div id="LegalIdentityProperties" style="display:{{Config.UseLegalIdentity ? "block" : "none"}}">

<p>
<label for="FirstName">First Name:</label>  
<input id="FirstName" name="FirstName" type="text" style="width:20em" title="First name of person." value="{{Config.FirstName}}" {{Config.Step=0 ? "autofocus" : ""}}/>
</p>

<p>
<label for="MiddleName">Middle Name:</label>  
<input id="MiddleName" name="MiddleName" type="text" style="width:20em" title="Middle name of person." value="{{Config.MiddleName}}"/>
</p>

<p>
<label for="LastName">Last Name:</label>  
<input id="LastName" name="LastName" type="text" style="width:20em" title="Last name of person." value="{{Config.LastName}}"/>
</p>

<p>
<label for="PNr">Personal Number (organizational number):</label>  
<input id="PNr" name="PNr" type="text" style="width:20em" title="Number of person or organization." value="{{Config.PersonalNumber}}"/>
</p>

<p>
<label for="Address">Address:</label>  
<input id="Address" name="Address" type="text" style="width:20em" title="Address." value="{{Config.Address}}"/>  
<input id="Address2" name="Address2" type="text" style="width:20em" title="Address." value="{{Config.Address2}}"/>
</p>

<p>
<label for="PostalCode">Postal Code (ZIP):</label>  
<input id="PostalCode" name="PostalCode" type="text" style="width:20em" title="Postal code, or zip code." value="{{Config.PostalCode}}"/>
</p>

<p>
<label for="Area">Area:</label>  
<input id="Area" name="Area" type="text" style="width:20em" title="Name of area." value="{{Config.Area}}"/>
</p>

<p>
<label for="City">City:</label>  
<input id="City" name="City" type="text" style="width:20em" title="Name of city." value="{{Config.City}}"/>
</p>

<p>
<label for="Region">Region:</label>  
<input id="Region" name="Region" type="text" style="width:20em" title="Name of region." value="{{Config.Region}}"/>
</p>

<p>
<label for="Country">Country:</label>  
<input id="Country" name="Country" type="text" style="width:20em" title="Name of country." value="{{Config.Country}}"/>
</p>

{{if Config.AlternativeFields!=null then (Index:=0;foreach AlternativeField in Config.AlternativeFields do (]]
<p>
<label for="AltFieldName((Index))">Alternative Field:</label>  
<input id="AltFieldName((Index))" name="NameAltField((Index))" type="text" style="width:20em" title="Alternative field name."
	value="((AlternativeField.Key))"/>
<input id="AltFieldValue((Index))" name="AltFieldValue((Index))" type="text" style="width:20em" title="Alternative field value."
	value="((AlternativeField.Value))"/>
<button type="button" class="negButtonSm" onclick="RemoveAltField('((Index))')">Remove</button>
</p>
[[;Index++))}}

<p>
<label for="AltFieldName">Alternative Field:</label>  
<input id="AltFieldName" name="AltFieldName" type="text" style="width:20em" title="Alternative field name."/>
<input id="AltFieldValue" name="AltFieldValue" type="text" style="width:20em" title="Alternative field value."/>
<button type="button" class="posButtonSm" onclick="AddAltField()">Add</button>
</p>

<p>
<input type="checkbox" name="ProtectWithPassword" id="ProtectWithPassword" {{Config.ProtectWithPassword ? "checked" : ""}} onclick="TogglePasswordProperties()"/>
<label for="ProtectWithPassword" title="If the legal identity should be protected with a password. If so, contracts signed with the identity must be manually approved before they can be signed.">Protect with password.</label>
</p>

<div id="PasswordProperties" style="display:{{Config.ProtectWithPassword ? "block" : "none"}}">

<p>
<label for="Password">Password:</label>  
<input id="Password" name="Password" type="password" style="width:20em" title="Password used to protect the legal identity."/>
</p>

<p>
<label for="Password2">Retype Password:</label>  
<input id="Password2" name="Password2" type="password" style="width:20em" title="Retype password, to make sure you've typed it correctly.."/>
</p>

</div>

<p>Press the Apply button to apply for the legal identity above to be approved.</p>
<p id="ApplyError" class="error" style="display:none">Unable to apply for a legal identity. Error reported: <span id='Error'></span></p>
<p id="NextMessage" class="message"{{Config.Step=0 ? ' style="display:none"':''}}>Application successfully sent. You can wait here until the identity becomes approved, or choose to continue, by clicking the Next button.</p>

<button type='button' onclick='ApplyIdentity()'>Apply</button>
{{if Configuring then 
]]<button id='NextButton' type='button' onclick='Next()' style='display:((Config.Step>=1 ? "inline-block" : "none"))'>Next</button>[[ else 
]]<button id='NextButton' type='button' onclick='Ok()'>OK</button>[[;
}}

</div>

<div id="NotLegalIdentityProperties" style="display:{{Config.UseLegalIdentity ? "none" : "block"}}">
{{if Configuring then ]]
<button type='button' onclick='Next()'>Next</button>
[[ else ]]
<button type='button' onclick='Ok()'>OK</button>
[[;}}
</div>

</fieldset>
</form>


<fieldset>
<legend>Legal Identities</legend>

The following table lists legal identities associated with this application.

<div id="Identities">

![Identities](LegalIdentities.md)

</div>
</fieldset>