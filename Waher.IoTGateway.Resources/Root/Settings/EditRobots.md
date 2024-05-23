Title: robots.txt
Description: Page where the robots.txt file can be edited.
Date: 2024-05-22
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
UserVariable: User
Privilege: Admin.Security.Edit.RobotsTxt
Login: /Login.md

================================================================================================================================

robots.txt
=============

The contents of the [robots.txt](https://www.robotstxt.org) file can be customized on this page. This is a domain-specific page, so
if you edit the contents of this file using an alternative domain name, that configuration will be available only for that domain.
Editing the file using the main domain name (or `localhost`, or using an IP-address), will edit the default version. The contents
of the file in the root foled will be kept intact. The contents of the file will be stored in the database, to avoid it being
updated when the software is updated.

<form action="EditRobots.md" method="post">

Contents of `robots.txt`:  
<textarea name="TextContents" style="min-height:40vh">{{
RobotsTxt:=System.IO.Path.Combine(Waher.IoTGateway.Gateway.RootFolder,"robots.txt");

if exists(Posted) then 
(
    if !(Posted matches {"TextContents":Required(Str(PTextContents))}) then BadRequest("Payload invalid.");
    SetDomainSetting(Request,RobotsTxt,PTextContents);
);

s:=Waher.IoTGateway.DomainSettings.GetFileSettingAsync(Request,RobotsTxt);
s.Replace("\r\n","\n").Replace("\r","\n").Replace("\n","\r\n")}}</textarea>

<button type="submit" class="posButton">Apply</button>
</form>
