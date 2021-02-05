Title: Event Log
Description: Allows the user to view events in real-time.
Date: 2021-02-05
Author: Peter Waher
Master: /Master.md
JavaScript: /Events.js
JavaScript: EventLog.js
CSS: EventLog.cssx
UserVariable: User
Privilege: Admin.Data.Events
Login: /Login.md

========================================================================

Event Log
===========================

On this page, you can follow events as they are logged, in real-time. The view will automatically be terminated after some time 
to avoid performance degradation and leaks. Event-logs should only be used as a tool for troubleshooting.

{{GW:=Waher.IoTGateway.Gateway;GW.AddWebEventSink("WebAdmin",Request,"User",["Admin.Data.Events"])}}

<table cellspacing="0" cellpadding="2" border="0" style="word-break:break-all">
<thead>
<tr>
<th>Date</th>
<th>Time</th>
<th>Type</th>
<th>Level</th>
<th>EventId</th>
<th>Object</th>
<th>Actor</th>
<th>Module</th>
<th>Facility</th>
<th>Message</th>
</tr>
</thead>
<tbody id="EventLogBody">
</tbody>
</table>
