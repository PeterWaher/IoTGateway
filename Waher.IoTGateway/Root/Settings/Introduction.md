Title: Introduction
Description: Provides the user with a short introduction.
Date: 2018-05-16
Author: Peter Waher
Master: {{(Configuring:=Waher.IoTGateway.Gateway.Configuring) ? "Master.md" : "/Master.md"}}
JavaScript: /Events.js
JavaScript: /Settings/Next.js
UserVariable: User
Login: /Login.md


========================================================================

Welcome
=============================

<form>

Before **{{Waher.IoTGateway.Gateway.ApplicationName}}** can run properly, you need to provide some basic configurations. The following pages
allow you to setup the basic functionality of the application.{{if Configuring then ]] Press the **Detailed** button to go through all
individual configuration steps now. You can also press **Simplified** to only go through the required configurations steps now. The other
steps can be configured at a later time.

<button type='button' onclick='Next()'>Detailed</button>
<button type='button' onclick='ConfigComplete("Simplified")'>Simplified</button>
[[ else ]]

<button type='button' onclick='Ok()'>OK</button>
[[;}}

</form>

