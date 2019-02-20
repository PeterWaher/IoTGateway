Title: Theme Settings
Description: Allows the user to configure theme settings.
Date: 2018-06-15
Author: Peter Waher
Copyright: /Copyright.md
Master: Master.md
JavaScript: /Events.js
JavaScript: /Settings/Theme.js
JavaScript: /Settings/Next.js
CSS: /Settings/Config.cssx
UserVariable: User
Login: /Login.md

========================================================================

Theme
============

A theme defines a general look and feel for web pages. Select the theme you like the most from the list below.

<form>
<fieldset>
<legend>Selected Theme</legend>

<div id="themes" class="themes">
{{ConfigClass:=Waher.IoTGateway.Setup.ThemeConfiguration;Config:=ConfigClass.Instance;foreach ThemeDefinition in ConfigClass.GetDefinitions() do ]]
<div data-theme-id="((ThemeDefinition.Id))" class="theme((Config.ThemeId=ThemeDefinition.Id?"Selected"))" onclick="SetTheme('((ThemeDefinition.Id))')">
<img class="themeImage" alt="((HtmlAttributeEncode(ThemeDefinition.Title);))" width="((Thumbnail:=ThemeDefinition.Thumbnail;Thumbnail.Width))" height="((Thumbnail.Height))" src="((HtmlAttributeEncode(Thumbnail.Resource);))"/>
<div class="themeTitle">

((MarkdownEncode(ThemeDefinition.Title);))

</div>
</div>
[[;}}
</div>

{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button id='NextButton' type='button' onclick='Next()' style='display:((Config.Step>0 ? "inline-block" : "none"))'>Next</button>
[[ else ]]
<button id='NextButton' type='button' onclick='Ok()'>OK</button>
[[;}}

</fieldset>

</form>

