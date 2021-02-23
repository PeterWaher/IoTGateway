Title: Remote Endpoint {{EP}}
Description: Page containing information about a specific remote endpoint.
Date: 2020-05-26
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
Javascript: RemoteEndpoint.js
Javascript: /Events.js
UserVariable: User
Login: /Login.md
Privilege: Admin.Communication.Endpoints
Parameter: EP

================================================================================================================================

Endpoint details
===================

| {{Obj:=Waher.IoTGateway.Gateway.LoginAuditor.GetAnnotatedStateObject(EP).Result;
(Obj?.Endpoint)??(NotFound("Endpoint not found in database: "+EP))}}             ||
|:--------------------------------|:----------------------------------------------|
| Created:                        | {{MarkdownEncode(Obj.Created.ToString())}}    |
| Last Protocol:                  | {{MarkdownEncode(Obj.LastProtocol)}}          |
| Last Failed:                    | {{Obj.LastFailed}}                            |
| Blocked:                        | {{Obj.Blocked ? "✔" : "✗"}}                 |
| Reason:                         | {{MarkdownEncode(Obj.Reason)}}                |
| City:                           | {{MarkdownEncode(Obj.City)}}                  |
| Region:                         | {{MarkdownEncode(Obj.Region)}}                |
| Country:                        | {{MarkdownEncode(Obj.Country)}}               |
| Country Code:                   | {{MarkdownEncode(Obj.Code)}}                  |
| Flag:                           | {{Obj.Flag}}                                  |

<button type="button" onclick="ClearStatus('{{EP}}')" title="Clear block and state status from endpoint.">Clear Status</button>

<fieldset>
<legend>WHOIS Information</legend>

```
{{Obj.WhoIs}}
```

</fieldset>