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
<select id="Country" name="Country" style="width:20em" title="Name of country.">
{{
	Countries:=[
	   {"code":"AF", "name":"AFGHANISTAN"},
	   {"code":"AX", "name":"ÅLAND ISLANDS"},
	   {"code":"AL", "name":"ALBANIA"},
	   {"code":"DZ", "name":"ALGERIA"},
	   {"code":"AS", "name":"AMERICAN SAMOA"},
	   {"code":"AD", "name":"ANDORRA"},
	   {"code":"AO", "name":"ANGOLA"},
	   {"code":"AI", "name":"ANGUILLA"},
	   {"code":"AQ", "name":"ANTARCTICA"},
	   {"code":"AG", "name":"ANTIGUA AND BARBUDA"},
	   {"code":"AR", "name":"ARGENTINA"},
	   {"code":"AM", "name":"ARMENIA"},
	   {"code":"AW", "name":"ARUBA"},
	   {"code":"AU", "name":"AUSTRALIA"},
	   {"code":"AT", "name":"AUSTRIA"},
	   {"code":"AZ", "name":"AZERBAIJAN"},
	   {"code":"BS", "name":"BAHAMAS"},
	   {"code":"BH", "name":"BAHRAIN"},
	   {"code":"BD", "name":"BANGLADESH"},
	   {"code":"BB", "name":"BARBADOS"},
	   {"code":"BY", "name":"BELARUS"},
	   {"code":"BE", "name":"BELGIUM"},
	   {"code":"BZ", "name":"BELIZE"},
	   {"code":"BJ", "name":"BENIN"},
	   {"code":"BM", "name":"BERMUDA"},
	   {"code":"BT", "name":"BHUTAN"},
	   {"code":"BO", "name":"BOLIVIA"},
	   {"code":"BA", "name":"BOSNIA AND HERZEGOVINA"},
	   {"code":"BW", "name":"BOTSWANA"},
	   {"code":"BV", "name":"BOUVET ISLAND"},
	   {"code":"BR", "name":"BRAZIL"},
	   {"code":"IO", "name":"BRITISH INDIAN OCEAN TERRITORY"},
	   {"code":"BN", "name":"BRUNEI DARUSSALAM"},
	   {"code":"BG", "name":"BULGARIA"},
	   {"code":"BF", "name":"BURKINA FASO"},
	   {"code":"BI", "name":"BURUNDI"},
	   {"code":"KH", "name":"CAMBODIA"},
	   {"code":"CM", "name":"CAMEROON"},
	   {"code":"CA", "name":"CANADA"},
	   {"code":"CV", "name":"CAPE VERDE"},
	   {"code":"KY", "name":"CAYMAN ISLANDS"},
	   {"code":"CF", "name":"CENTRAL AFRICAN REPUBLIC"},
	   {"code":"TD", "name":"CHAD"},
	   {"code":"CL", "name":"CHILE"},
	   {"code":"CN", "name":"CHINA"},
	   {"code":"CX", "name":"CHRISTMAS ISLAND"},
	   {"code":"CC", "name":"COCOS (KEELING) ISLANDS"},
	   {"code":"CO", "name":"COLOMBIA"},
	   {"code":"KM", "name":"COMOROS"},
	   {"code":"CG", "name":"CONGO"},
	   {"code":"CD", "name":"CONGO, THE DEMOCRATIC REPUBLIC OF THE"},
	   {"code":"CK", "name":"COOK ISLANDS"},
	   {"code":"CR", "name":"COSTA RICA"},
	   {"code":"CI", "name":"COTE D'IVOIRE"},
	   {"code":"HR", "name":"CROATIA"},
	   {"code":"CU", "name":"CUBA"},
	   {"code":"CY", "name":"CYPRUS"},
	   {"code":"CZ", "name":"CZECH REPUBLIC"},
	   {"code":"DK", "name":"DENMARK"},
	   {"code":"DJ", "name":"DJIBOUTI"},
	   {"code":"DM", "name":"DOMINICA"},
	   {"code":"DO", "name":"DOMINICAN REPUBLIC"},
	   {"code":"EC", "name":"ECUADOR"},
	   {"code":"EG", "name":"EGYPT"},
	   {"code":"SV", "name":"EL SALVADOR"},
	   {"code":"GQ", "name":"EQUATORIAL GUINEA"},
	   {"code":"ER", "name":"ERITREA"},
	   {"code":"EE", "name":"ESTONIA"},
	   {"code":"ET", "name":"ETHIOPIA"},
	   {"code":"FK", "name":"FALKLAND ISLANDS (MALVINAS)"},
	   {"code":"FO", "name":"FAROE ISLANDS"},
	   {"code":"FJ", "name":"FIJI"},
	   {"code":"FI", "name":"FINLAND"},
	   {"code":"FR", "name":"FRANCE"},
	   {"code":"GF", "name":"FRENCH GUIANA"},
	   {"code":"PF", "name":"FRENCH POLYNESIA"},
	   {"code":"TF", "name":"FRENCH SOUTHERN TERRITORIES"},
	   {"code":"GA", "name":"GABON"},
	   {"code":"GM", "name":"GAMBIA"},
	   {"code":"GE", "name":"GEORGIA"},
	   {"code":"DE", "name":"GERMANY"},
	   {"code":"GH", "name":"GHANA"},
	   {"code":"GI", "name":"GIBRALTAR"},
	   {"code":"GR", "name":"GREECE"},
	   {"code":"GL", "name":"GREENLAND"},
	   {"code":"GD", "name":"GRENADA"},
	   {"code":"GP", "name":"GUADELOUPE"},
	   {"code":"GU", "name":"GUAM"},
	   {"code":"GT", "name":"GUATEMALA"},
	   {"code":"GG", "name":"GUERNSEY"},
	   {"code":"GN", "name":"GUINEA"},
	   {"code":"GW", "name":"GUINEA-BISSAU"},
	   {"code":"GY", "name":"GUYANA"},
	   {"code":"HT", "name":"HAITI"},
	   {"code":"HM", "name":"HEARD ISLAND AND MCDONALD ISLANDS"},
	   {"code":"VA", "name":"HOLY SEE (VATICAN CITY STATE)"},
	   {"code":"HN", "name":"HONDURAS"},
	   {"code":"HK", "name":"HONG KONG"},
	   {"code":"HU", "name":"HUNGARY"},
	   {"code":"IS", "name":"ICELAND"},
	   {"code":"IN", "name":"INDIA"},
	   {"code":"ID", "name":"INDONESIA"},
	   {"code":"IR", "name":"IRAN, ISLAMIC REPUBLIC OF"},
	   {"code":"IQ", "name":"IRAQ"},
	   {"code":"IE", "name":"IRELAND"},
	   {"code":"IM", "name":"ISLE OF MAN"},
	   {"code":"IL", "name":"ISRAEL"},
	   {"code":"IT", "name":"ITALY"},
	   {"code":"JM", "name":"JAMAICA"},
	   {"code":"JP", "name":"JAPAN"},
	   {"code":"JE", "name":"JERSEY"},
	   {"code":"JO", "name":"JORDAN"},
	   {"code":"KZ", "name":"KAZAKHSTAN"},
	   {"code":"KE", "name":"KENYA"},
	   {"code":"KI", "name":"KIRIBATI"},
	   {"code":"KP", "name":"KOREA, DEMOCRATIC PEOPLE'S REPUBLIC OF"},
	   {"code":"KR", "name":"KOREA, REPUBLIC OF"},
	   {"code":"KW", "name":"KUWAIT"},
	   {"code":"KG", "name":"KYRGYZSTAN"},
	   {"code":"LA", "name":"LAO PEOPLE'S DEMOCRATIC REPUBLIC"},
	   {"code":"LV", "name":"LATVIA"},
	   {"code":"LB", "name":"LEBANON"},
	   {"code":"LS", "name":"LESOTHO"},
	   {"code":"LR", "name":"LIBERIA"},
	   {"code":"LY", "name":"LIBYAN ARAB JAMAHIRIYA"},
	   {"code":"LI", "name":"LIECHTENSTEIN"},
	   {"code":"LT", "name":"LITHUANIA"},
	   {"code":"LU", "name":"LUXEMBOURG"},
	   {"code":"MO", "name":"MACAO"},
	   {"code":"MK", "name":"MACEDONIA, THE FORMER YUGOSLAV REPUBLIC OF"},
	   {"code":"MG", "name":"MADAGASCAR"},
	   {"code":"MW", "name":"MALAWI"},
	   {"code":"MY", "name":"MALAYSIA"},
	   {"code":"MV", "name":"MALDIVES"},
	   {"code":"ML", "name":"MALI"},
	   {"code":"MT", "name":"MALTA"},
	   {"code":"MH", "name":"MARSHALL ISLANDS"},
	   {"code":"MQ", "name":"MARTINIQUE"},
	   {"code":"MR", "name":"MAURITANIA"},
	   {"code":"MU", "name":"MAURITIUS"},
	   {"code":"YT", "name":"MAYOTTE"},
	   {"code":"MX", "name":"MEXICO"},
	   {"code":"FM", "name":"MICRONESIA, FEDERATED STATES OF"},
	   {"code":"MD", "name":"MOLDOVA, REPUBLIC OF"},
	   {"code":"MC", "name":"MONACO"},
	   {"code":"MN", "name":"MONGOLIA"},
	   {"code":"MS", "name":"MONTSERRAT"},
	   {"code":"MA", "name":"MOROCCO"},
	   {"code":"MZ", "name":"MOZAMBIQUE"},
	   {"code":"MM", "name":"MYANMAR"},
	   {"code":"NA", "name":"NAMIBIA"},
	   {"code":"NR", "name":"NAURU"},
	   {"code":"NP", "name":"NEPAL"},
	   {"code":"NL", "name":"NETHERLANDS"},
	   {"code":"AN", "name":"NETHERLANDS ANTILLES"},
	   {"code":"NC", "name":"NEW CALEDONIA"},
	   {"code":"NZ", "name":"NEW ZEALAND"},
	   {"code":"NI", "name":"NICARAGUA"},
	   {"code":"NE", "name":"NIGER"},
	   {"code":"NG", "name":"NIGERIA"},
	   {"code":"NU", "name":"NIUE"},
	   {"code":"NF", "name":"NORFOLK ISLAND"},
	   {"code":"MP", "name":"NORTHERN MARIANA ISLANDS"},
	   {"code":"NO", "name":"NORWAY"},
	   {"code":"OM", "name":"OMAN"},
	   {"code":"PK", "name":"PAKISTAN"},
	   {"code":"PW", "name":"PALAU"},
	   {"code":"PS", "name":"PALESTINIAN TERRITORY, OCCUPIED"},
	   {"code":"PA", "name":"PANAMA"},
	   {"code":"PG", "name":"PAPUA NEW GUINEA"},
	   {"code":"PY", "name":"PARAGUAY"},
	   {"code":"PE", "name":"PERU"},
	   {"code":"PH", "name":"PHILIPPINES"},
	   {"code":"PN", "name":"PITCAIRN"},
	   {"code":"PL", "name":"POLAND"},
	   {"code":"PT", "name":"PORTUGAL"},
	   {"code":"PR", "name":"PUERTO RICO"},
	   {"code":"QA", "name":"QATAR"},
	   {"code":"RE", "name":"REUNION"},
	   {"code":"RO", "name":"ROMANIA"},
	   {"code":"RU", "name":"RUSSIAN FEDERATION"},
	   {"code":"RW", "name":"RWANDA"},
	   {"code":"SH", "name":"SAINT HELENA"},
	   {"code":"KN", "name":"SAINT KITTS AND NEVIS"},
	   {"code":"LC", "name":"SAINT LUCIA"},
	   {"code":"PM", "name":"SAINT PIERRE AND MIQUELON"},
	   {"code":"VC", "name":"SAINT VINCENT AND THE GRENADINES"},
	   {"code":"WS", "name":"SAMOA"},
	   {"code":"SM", "name":"SAN MARINO"},
	   {"code":"ST", "name":"SAO TOME AND PRINCIPE"},
	   {"code":"SA", "name":"SAUDI ARABIA"},
	   {"code":"SN", "name":"SENEGAL"},
	   {"code":"CS", "name":"SERBIA AND MONTENEGRO"},
	   {"code":"SC", "name":"SEYCHELLES"},
	   {"code":"SL", "name":"SIERRA LEONE"},
	   {"code":"SG", "name":"SINGAPORE"},
	   {"code":"SK", "name":"SLOVAKIA"},
	   {"code":"SI", "name":"SLOVENIA"},
	   {"code":"SB", "name":"SOLOMON ISLANDS"},
	   {"code":"SO", "name":"SOMALIA"},
	   {"code":"ZA", "name":"SOUTH AFRICA"},
	   {"code":"GS", "name":"SOUTH GEORGIA AND THE SOUTH SANDWICH ISLANDS"},
	   {"code":"ES", "name":"SPAIN"},
	   {"code":"LK", "name":"SRI LANKA"},
	   {"code":"SD", "name":"SUDAN"},
	   {"code":"SR", "name":"SURINAME"},
	   {"code":"SJ", "name":"SVALBARD AND JAN MAYEN"},
	   {"code":"SZ", "name":"SWAZILAND"},
	   {"code":"SE", "name":"SWEDEN"},
	   {"code":"CH", "name":"SWITZERLAND"},
	   {"code":"SY", "name":"SYRIAN ARAB REPUBLIC"},
	   {"code":"TW", "name":"TAIWAN, PROVINCE OF CHINA"},
	   {"code":"TJ", "name":"TAJIKISTAN"},
	   {"code":"TZ", "name":"TANZANIA, UNITED REPUBLIC OF"},
	   {"code":"TH", "name":"THAILAND"},
	   {"code":"TL", "name":"TIMOR-LESTE"},
	   {"code":"TG", "name":"TOGO"},
	   {"code":"TK", "name":"TOKELAU"},
	   {"code":"TO", "name":"TONGA"},
	   {"code":"TT", "name":"TRINIDAD AND TOBAGO"},
	   {"code":"TN", "name":"TUNISIA"},
	   {"code":"TR", "name":"TURKEY"},
	   {"code":"TM", "name":"TURKMENISTAN"},
	   {"code":"TC", "name":"TURKS AND CAICOS ISLANDS"},
	   {"code":"TV", "name":"TUVALU"},
	   {"code":"UG", "name":"UGANDA"},
	   {"code":"UA", "name":"UKRAINE"},
	   {"code":"AE", "name":"UNITED ARAB EMIRATES"},
	   {"code":"GB", "name":"UNITED KINGDOM"},
	   {"code":"US", "name":"UNITED STATES"},
	   {"code":"UM", "name":"UNITED STATES MINOR OUTLYING ISLANDS"},
	   {"code":"UY", "name":"URUGUAY"},
	   {"code":"UZ", "name":"UZBEKISTAN"},
	   {"code":"VU", "name":"VANUATU"},
	   {"code":"VE", "name":"VENEZUELA"},
	   {"code":"VN", "name":"VIET NAM"},
	   {"code":"VG", "name":"VIRGIN ISLANDS, BRITISH"},
	   {"code":"VI", "name":"VIRGIN ISLANDS, U.S."},
	   {"code":"WF", "name":"WALLIS AND FUTUNA"},
	   {"code":"EH", "name":"WESTERN SAHARA"},
	   {"code":"YE", "name":"YEMEN"},
	   {"code":"ZM", "name":"ZAMBIA"},
	   {"code":"ZW", "name":"ZIMBABWE"}
	];
	foreach Country in Countries do
		]]<option value="((Country.code))"((Config.Country=Country.code?" selected":""))>((Country.name))</option>[[
}}
</select>
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

<button type='button' onclick='RandomizePassword()'>Create Random Password</button>
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