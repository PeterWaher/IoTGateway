Title: Login
Description: IoT Gateway login page.
Date: 2016-12-25
Author: Peter Waher
Master: /Master.md
Parameter: from

============================================================================================================================================

Login
=============

{{Waher.IoTGateway.Gateway.CheckLocalLogin(Request)}}

<form id="LoginForm" action="/Login" method="post">

You need to login to proceed.

User Name:  
<input id="UserName" name="UserName" type="text" autofocus="autofocus" style="width:20em" />

Password:  
<input id="Password" name="Password" type="password" style="width:20em" />

{{if exists(LoginError) then]]
<div class='error'>
((LoginError))
</div>
[[;}}

<button id="LoginButton" type="submit">Login</button>

</form>
