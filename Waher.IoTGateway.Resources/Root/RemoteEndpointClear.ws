if !exists(User) then
	Forbidden("Access to resource is forbidden.");

Waher.IoTGateway.Gateway.LoginAuditor.UnblockAndReset(Posted.endpoint);

ClientEvents.PushEvent([Posted.tabId],"Reload",null);