Title: Introduction
Description: Provides the user with a short introduction.
Date: 2018-05-16
Author: Peter Waher
Copyright: /Copyright.md
Master: Master.md
JavaScript: /Events.js
JavaScript: /Settings/Next.js
UserVariable: User
Login: /Login.md


========================================================================

Welcome
=============================

<form>

Before **{{Waher.IoTGateway.Gateway.ApplicationName}}** can run properly, you need to provide some basic configurations. The following pages
allow you to setup the basic functionality of the application. Press the **Begin** button to start.

{{if Waher.IoTGateway.Gateway.Configuring then ]]
<button id='NextButton' type='button' onclick='Next()'>Begin</button>
[[ else ]]
<button id='OkButton' type='button' onclick='Ok()'>OK</button>
[[;}}

</form>

