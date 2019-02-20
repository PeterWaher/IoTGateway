Title: Personal Data
Description: Information about personal data processing.
Date: 2018-06-27
Author: Peter Waher
Copyright: /Copyright.md
Master: Master.md
JavaScript: /Events.js
JavaScript: /Settings/Next.js
JavaScript: /Settings/PersonalData.js
UserVariable: User
Login: /Login.md


========================================================================

Personal Data
=============================

<form>

**{{ApplicationName:=Waher.IoTGateway.Gateway.ApplicationName}}** does everything it can to protect your personal information. For this reason,
it is important for you to know what personal data is being processed and how. 
{{if Waher.IoTGateway.Gateway.Configuring then ]]If you consent to this processing, check the box below, and press **Next** to continue.[[}}

{{foreach ProcessingActivity in (ConfigClass:=Waher.IoTGateway.Setup.PersonalDataConfiguration;ConfigClass.ProcessingActivities) do ]]

![PII](PersonalData/((ProcessingActivity.TransparentInformationMarkdownFileName)))

[[}}

<br/>

{{if Waher.IoTGateway.Gateway.Configuring then ]]
<p>
<input type="checkbox" name="Consent" id="Consent" ((Config:=ConfigClass.Instance;Config.Consented ? "checked" : "")) onclick="ConsentClicked()"/>
<label for="Consent" title="Check this checkbox if you consent to the personal data processing described above.">I consent to the personal data processing described above.</label>
</p>
<button id='NextButton' type='button' onclick='Next()' style='display:((Config.Consented ? "inline-block" : "none"))'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}

</form>
