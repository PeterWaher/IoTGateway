Title: favicon.ico
Description: Page where the favicon.ico file can be edited.
Date: 2024-05-23
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
UserVariable: User
Privilege: Admin.Security.Edit.FavIconIco
Login: /Login.md

================================================================================================================================

favicon.ico
==============

The contents of the [favicon.ico](https://en.wikipedia.org/wiki/Favicon) file can be customized on this page. This is a domain-specific page, so
if you edit the contents of this file using an alternative domain name, that configuration will be available only for that domain.
Editing the file using the main domain name (or `localhost`, or using an IP-address), will edit the default version. The contents
of the file in the root folder will be kept intact. The contents of the file will be stored in the database to avoid it being
updated when the software is updated.

<form action="EditFavIcon.md" method="post" enctype="multipart/form-data">

Contents of `favicon.ico`:

{{
FavIconIco:=System.IO.Path.Combine(Waher.IoTGateway.Gateway.RootFolder,"favicon.ico");

ErrorMsg:=null;
if exists(Posted) then
(
    if Posted matches
    {
        "UploadIcon":Required(PImage),
        "UploadIcon_ContentType":Required(Str(PContentType)),
        "UploadIcon_Binary":Required(PBinary),
        "UploadIcon_FileName":Required(Str(PFileName))
    } then
    (
        if !StartsWith(PContentType,"image/") then
            ErrorMsg:="Only images accepted."
        else if PContentType!="image/x-icon" then
            ErrorMsg:="Only icon images accepted."
        else
            SetDomainSetting(Request,FavIconIco,Base64Encode(PBinary))
    )
    else
        ErrorMsg:="Invalid payload."
);

Bin:=Waher.IoTGateway.DomainSettings.GetBinaryFileSettingAsync(Request,FavIconIco);
Decode(Bin,"image/x-icon")
}}

<p>
<label for="UploadIcon">Upload new icon:</label>  
<input id="UploadIcon" name="UploadIcon" type="file" accept="image/x-icon" multiple="false" />
</p>

{{
if !empty(ErrorMsg) then
    ]]<p class="error">((ErrorMsg))</p>[[
}}

<button type="submit" class="posButton">Apply</button>
</form>
