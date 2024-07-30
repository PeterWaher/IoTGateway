Event Logs
-------------

Events are logged in the local internal encrypted database, where they are available for 90 days before being deleted. Events are also made 
available in text log files for security reasons in the following folder: `{{System.IO.Path.Combine(Waher.IoTGateway.Gateway.AppDataFolder,"Data")}}`.
Events are stored in this folder for 7 days. The reason for storing events locally, is for trouble-shooting, in case something goes wrong, or for 
analyzing performance.

If the broker to which *{{ApplicationName}}* is connected has a published event log, events of types `Error`, `Critical`, `Alert` and `Emergency`
are also forwarded to this broker event log. Events of type `Debug`, `Information`, `Notice` and `Warning` are not forwareded. The reason for
forwarding the selected events, is for detecting common errors, or for network security purposes.
