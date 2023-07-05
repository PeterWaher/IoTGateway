AuthenticateSession(Request,"User");
Authorize(User,"Admin.Communication.Endpoints");

Waher.IoTGateway.Gateway.LoginAuditor.UnblockAndReset(Posted.endpoint);

ClientEvents.PushEvent([Posted.tabId],"Reload",null);