UserVariable: User
Privilege: Admin.Data.Database
Login: /Login.md

<fieldset>
<legend>MongoDB Settings</legend>

<p>
<label for="HostName">Host Name:</label> (Leave empty if on local machine)  
<input id="HostName" name="HostName" type="text" value="{{Settings:=Config.DatabasePluginSettings;Settings.Host}}"/>
</p>

<p>
<label for="PortNumber">Port number:</label>  
<input id="PortNumber" name="PortNumber" type="number" min="1" max="65535" value="{{Settings.Port}}" style="width:20em"/>
</p>

<p>
<label for="DatabaseName">Database Name:</label>  
<input id="DatabaseName" name="DatabaseName" type="text" required value="{{Settings.Database}}"/>
</p>

<p>
<label for="DefaultCollection">Default Collection:</label>  
<input id="DefaultCollection" name="DefaultCollection" type="text" required value="{{Settings.DefaultCollection}}"/>
</p>

<p>
<label for="UserName">User Name:</label>  
<input id="UserName" name="UserName" type="text" value="{{Settings.UserName}}"/>
</p>

<p>
<label for="Password">Password:</label>  
<input id="Password" name="Password" type="password" value="{{Settings.Password}}"/>
</p>

</fieldset>