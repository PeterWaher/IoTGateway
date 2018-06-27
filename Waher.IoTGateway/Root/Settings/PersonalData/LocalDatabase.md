Application data
------------------

Most of the information processed by {{ApplicationName}} ends up in the local object database. This database is encrypted, for your integrity. 
Keys are stored in the operating system and are protected by your system user account. Some data is stored outside of the local object database. 
This includes temporary application and communication logs, cached files, etc.

You encrypted local database is stored in this folder: `{{System.IO.Path.Combine(Waher.IoTGateway.Gateway.AppDataFolder,"Data")}}`

Application data in general, is stored in this folder: `{{Waher.IoTGateway.Gateway.AppDataFolder}}`