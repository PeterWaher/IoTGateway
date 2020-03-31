Title: Notification Settings
Description: Allows the user to configure notification settings.
Date: 2019-01-12
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Settings/Notification.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Login: /Login.md

========================================================================

Notifications
===================

When important events occur in the application, notifications can be sent to one or more addresses. If you want to enable this feature,
enter one or more addresses below to which notifications will be sent.

<form>
<fieldset>
<legend>Notification Settings</legend>

<p>
<label for="NotificationAddresses">Send notifications to:</label>  
<input id="NotificationAddresses" name="NotificationAddresses" type="text" style="width:20em" title="Notifications will be sent to these addresses."
	value="{{ConfigClass:=Waher.IoTGateway.Setup.NotificationConfiguration;Config:=ConfigClass.Instance;Config.AddressesString}}" autofocus/>
</p>

**Note**: Separate addresses using semi-colon `;`

{{if Waher.IoTGateway.Setup.XmppConfiguration.Instance.Mail then ]]
**Note 2**: Notification addresses can be both XMPP addresses and normal mail addresses.
[[}}

<p>Press the Test button to send a test-notification to the notification addresses.</p>
<p id="TestError" class="error" style="display:none">Unable to send a notification. Please verify the addresses, and try again.</p>
<p id="NextMessage" class="message" style="display:none">Notification sent. Please check that they are received properly.
{{if Waher.IoTGateway.Setup.XmppConfiguration.Instance.Mail then ]]
(You might need to check the spam folder.)
[[}}
</p>

<button type='button' onclick='TestAddresses(true,false)'>Test</button>
{{if Configuring then ]]
<button id='NextButton' type='button' onclick='TestAddresses(false,true)'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='TestAddresses(false,false)'>OK</button>
[[;}}

</fieldset>
</form>
