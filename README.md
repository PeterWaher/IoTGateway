IoTGateway
======================

**IoTGateway** is a C# implementation of an IoT gateway. It is self contained, and includes all libraries and frameworks 
it needs to operate. You can install it by using the following [IoT Gateway Setup application](Executables/IoTGatewaySetup.exe).
Example applications also include binary downloads.

Apart from the [IoT Gateway](#iot-gateway) projects, the solution is divided into different groups of projects and modules:

* [Clients](#clients)
* [Content](#content)
* [Events](#events)
* [Mocks](#mocks)
* [Networking](#networking)
* [Persistence](#persistence)
* [Runtime](#runtime)
* [Script](#script)
* [Security](#security)
* [Services](#services)
* [Things](#things)
* [Utilities](#utilities)
* [Web Services](#webServices)

License
----------------------

You should carefully read the following terms and conditions before using this software. Your use of this software indicates
your acceptance of this license agreement and warranty. If you do not agree with the terms of this license, or if the terms of this
license contradict with your local laws, you must remove any files from the **IoT Gateway** from your storage devices and cease to use it. 
The terms of this license are subjects of changes in future versions of the **IoT Gateway**.

You may not use, copy, emulate, clone, rent, lease, sell, modify, decompile, disassemble, otherwise reverse engineer, or transfer the
licensed program, or any subset of the licensed program, except as provided for in this agreement.  Any such unauthorised use shall
result in immediate and automatic termination of this license and may result in criminal and/or civil prosecution.

The [source code](https://github.com/PeterWaher/IoTGateway) provided in this project is provided open for the following uses:

* For **Personal evaluation**. Personal evaluation means evaluating the code, its libraries and underlying technologies, including learning 
	about underlying technologies.

* For **Academic use**. If you want to use the following code for academic use, all you need to do is to inform the author of who you are, what 
	academic institution you work for (or study for), and in what projects you intend to use the code. All I ask in return is for an 
	acknowledgement and visible attribution to this project, inluding a link, and that you do not redistribute the source code, or parts thereof 
	in the solutions you develop. If any solutions developed in an academic setting, become commercial, it will need a commercial license.

* For **Security analysis**. If you perform any security analysis on the code, to see what security aspects the code might have,
	all I ask is that you inform me of any findings so that any vulnerabilities might be addressed. I am thankful for any such contributions,
	and will acknowledge them.

All rights to the source code are reserved and exclusively owned by [Waher Data AB](http://waher.se/). If you're interested in using the 
source code, as a whole, or partially, you need a license agreement with the author. You can contact him through [LinkedIn](http://waher.se/).

This software is provided by the copyright holder and contributors "as is" and any express or implied warranties, including, but not limited to, 
the implied warranties of merchantability and fitness for a particular purpose are disclaimed. In no event shall the copyright owner or contributors 
be liable for any direct, indirect, incidental, special, exemplary, or consequential damages (including, but not limited to, procurement of substitute 
goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict 
liability, or tort (including negligence or otherwise) arising in any way out of the use of this software, even if advised of the possibility of such 
damage.

The **IoT Gateway** is &copy; [Waher Data AB](http://waher.se/) 2016.
 
[![](Images/logo-Futura-300x58.png)](http://waher.se/)

IoT Gateway
----------------------

The IoT Gateway is represented by the following set of projects. They are back-end server applications and perform 
communiction with devices, as well as host online content.
You can install it by using the following [IoT Gateway Setup application](Executables/IoTGatewaySetup.exe).

| Project | Project description |
|-------------|---------------------|
| **Waher.IoTGateway** | The [Waher.IoTGateway](Waher.IoTGateway) project is a class library that defines the IoT Gateway. The gateway can host any web content. It converts markdown to HTML in real-time. It can be administrated over XMPP using the [Waher.Client.WPF](Clients/Waher.Client.WPF) application. |
| **Waher.IoTGateway.Console** | The [Waher.IoTGateway.Console](Waher.IoTGateway.Console) project is a console application version of the IoT Gateway. It's easy to use and experiment with. |
| **Waher.IoTGateway.Installers** | The [Waher.IoTGateway.Installers](Waher.IoTGateway.Installers) project defines custom actions used by the setup application to install the IoT Gateway and dependencies propertly. |
| **Waher.IoTGateway.Resources** | The [Waher.IoTGateway.Resources](Waher.IoTGateway.Resources) project contains resource files that are common to all IoT Gateway embodiments. |
| **Waher.IoTGateway.Setup** | The [Waher.IoTGateway.Setup](Waher.IoTGateway.Setup) project creates a Windows setup application that bootstraps several bundles into one setup application. Apart from installing the IoT Gateway, it also installs any prerequisites, such as the correct .NET framework and database needed. It is based on in [Wix framework](https://www.firegiant.com/wix/). |
| **Waher.IoTGateway.Svc** | The [Waher.IoTGateway.Svc](Waher.IoTGateway.Svc) project is a Windows Service version version of the IoT Gateway. |
| **Waher.IoTGateway.Wix** | The [Waher.IoTGateway.Wix](Waher.IoTGateway.Wix) project creates a Windows MSI package that installs the IoT Gateway, based on in [Wix framework](https://www.firegiant.com/wix/). |

Clients
----------------------

The [Clients](Clients) folder contains projects starting with **Waher.Client.** and denote client projects. Clients are front-end applications that 
can be run by users to perform different types of interaction with things or the network.

| Project | Project description |
|-------------|---------------------|
| **Waher.Client.WPF** | The [Waher.Client.WPF](Clients/Waher.Client.WPF) project is a simple IoT client that allows you to interact with things and users. If you connect to the network, you can chat with users and things. The client GUI is built using Windows Presentation Foundation (WPF). Chat sessions support normal plain text content, and rich content based on markdown. |

Content
----------------------

The [Content](Content) folder contains libraries that manage Internet Content, and Internet Content Type encodings and decodings.

| Project | Project description |
|-------------|---------------------|
| **Waher.Content** | The [Waher.Content](Content/Waher.Content) project is a class library that provides basic abstraction for Internet Content Type, and basic encodings and decodings. This includes handling and parsing of common data types. |
| **Waher.Content.Drawing** | The [Waher.Content.Drawing](Content/Waher.Content.Drawing) project contains encoders and decoders for images, as defined in the **System.Drawing** library. |
| **Waher.Content.Emoji** | The [Waher.Content.Emoji](Content/Waher.Content.Emoji) project contains utilities for working with emojis. The library is a portable class library, and supports .NET 4.6 as well as UWP. |
| **Waher.Content.Emoji.Emoji1** | The [Waher.Content.Emoji.Emoji1](Content/Waher.Content.Emoji.Emoji1) project provide free emojis from [Emoji One](http://emojione.com/) to content applications. The library is a portable class library, and supports .NET 4.6 as well as UWP. |
| **Waher.Content.Markdown** | The [Waher.Content.Markdown](Content/Waher.Content.Markdown) project parses markdown documents and transforms them to other formats, such as HTML, Plain text and XAML. For a description of the markdown flavour supported by the parser, see [Markdown documentation](Content/Waher.Content.Markdown/README.md). |
| **Waher.Content.Markdown.UWP** | The [Waher.Content.Markdown.UWP](Content/Waher.Content.Markdown.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Content.Markdown** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. It is limited compared to the normal **Waher.Content.Markdown** library, in that it does not support graphing in embedded script. |
| **Waher.Content.Markdown.Test** | The [Waher.Content.Markdown.Test](Content/Waher.Content.Markdown.Test) project contains unit tests for the **Waher.Content.Markdown** project. |
| **Waher.Content.UWP** | The [Waher.Content.UWP](Content/Waher.Content.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Content** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Events
----------------------

The [Events](Events) folder contains libraries that manage different aspects of event logging in networks.

| Project | Project description |
|-------------|---------------------|
| **Waher.Events** | The [Waher.Events](Events/Waher.Events) project provides the basic architecture and framework for event logging in applications. It uses the static class **Log** as a hub for all type of event logging in applications. To this hub you can register any number of **Event Sinks** that receive events and distribute them according to implementation details in each one. By logging all events to **Log** you have a configurable environment where you can change logging according to specific needs of the project. |
| **Waher.Events.Console** | The [Waher.Events.Console](Events/Waher.Events.Console) project provides a simple event sink, that outputs events to the console standard output. Useful, if creating simple console applications. |
| **Waher.Events.Documentation** | The [Waher.Events.Documentation](Events/Waher.Events.Documentation) project contains documentation of specific important events. This documentation includes Event IDs and any parameters they are supposed to include. |
| **Waher.Events.Files** | The [Waher.Events.Files](Events/Waher.Events.Files) project defines event sinks that outputs events to files. Supported formats are plain text and XML. XML files can be transformed using XSLT to other formats, such as HTML. |
| **Waher.Events.MQTT** | The [Waher.Events.MQTT](Events/Waher.Events.MQTT) project defines an event sink that sends events to an MQTT topic. Events are sent as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.MQTT.WPFClient** | The [Waher.Events.MQTT.WPFClient](Events/Waher.Events.MQTT.WPFClient) project defines a simple WPF client application that subscribes to an MQTT topic and displays any events it receivs. Events are parsed as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.UWP** | The [Waher.Events.UWP](Events/Waher.Events.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Events** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Events.WindowsEventLog** | The [Waher.Events.WindowsEventLog](Events/Waher.Events.WindowsEventLog) project defines an event sink that logs events to a Windows Event Log. |
| **Waher.Events.XMPP** | The [Waher.Events.XMPP](Events/Waher.Events.XMPP) project defines an event sink that distributes events over XMPP, according to [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.XMPP.UWP** | The [Waher.Events.XMPP.UWP](Events/Waher.Events.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Events.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Mocks
----------------------

The [Mocks](Mocks) folder contains projects that implement different mock devices. These can be used as development tools to test technologies, 
implementation, networks and tools.

| Project | Project description |
|-------------|---------------------|
| **Waher.Mock** | The [Waher.Mock](Mocks/Waher.Mock) project is a class library that provides support for simple mock applications. This includes simple network configuration. |
| **Waher.Mock.Lamp** | The [Waher.Mock.Lamp](Mocks/Waher.Mock.Lamp) project simulates a simple lamp switch with an XMPP interface. |
| **Waher.Mock.Lamp.UWP** | The [Waher.Mock.Lamp.UWP](Mocks/Waher.Mock.Lamp.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Lamp** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.Temperature** | The [Waher.Mock.Temperature](Mocks/Waher.Mock.Temperature) project simulates a simple temperature sensor with an XMPP interface. |
| **Waher.Mock.Temperature.UWP** | The [Waher.Mock.Temperature.UWP](Mocks/Waher.Mock.Temperature.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Temperature** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.UWP** | The [Waher.Mock.UWP](Mocks/Waher.Mock.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Mock** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. It is limited in that it does not provide a console dialog for editing connection parameters if none exist. It does not include schema validation of XML configuration files either. |

Networking
----------------------

The [Networking](Networking) folder contains libraries that manage different aspects of network communication.

| Project | Project description |
|-------------|---------------------|
| **Waher.Networking** | The [Waher.Networking](Networking/Waher.Networking) project provides the basic architecture and tools for all networking libraries.  This includes sniffers, etc. |
| **Waher.Networking.CoAP** | The [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) project provides a simple CoAP client. |
| **Waher.Networking.CoAP.Test** | The [Waher.Networking.CoAP.Test](Networking/Waher.Networking.CoAP.Test) project contains unit-tests for the [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) library. |
| **Waher.Networking.HTTP** | The [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) project provides a simple HTTP server for publishing dynamic content and managing user authentication based on a customizable set of users and privileges. |
| **Waher.Networking.HTTP.Test** | The [Waher.Networking.HTTP.Test](Networking/Waher.Networking.HTTP.Test) project contains unit-tests for the [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) library. |
| **Waher.Networking.MQTT** | The [Waher.Networking.MQTT](Networking/Waher.Networking.MQTT) project provides a simple MQTT client. |
| **Waher.Networking.PeerToPeer** | The [Waher.Networking.PeerToPeer](Networking/Waher.Networking.PeerToPeer) project provides tools for peer-to-peer communication. |
| **Waher.Networking.UPnP** | The [Waher.Networking.UPnP](Networking/Waher.Networking.UPnP) project provides a library for interacting with UPnP-enabled devices in the network. |
| **Waher.Networking.UWP** | The [Waher.Networking.UWP](Networking/Waher.Networking.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP** | The [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) project provides a simple XMPP client. |
| **Waher.Networking.XMPP.Chat** | The [Waher.Networking.XMPP.Chat](Networking/Waher.Networking.XMPP.Chat) project provides a simple XMPP chat server bot for things, that is added to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). It supports markdown, and follows the chat semantics outlined in this proto-XEP: [Chat Interface for Internet of Things Devices](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html) |
| **Waher.Networking.XMPP.Chat.UWP** | The [Waher.Networking.XMPP.Chat.UWP](Networking/Waher.Networking.XMPP.Chat.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Chat** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Concentrator** | The [Waher.Networking.XMPP.Concentrator](Networking/Waher.Networking.XMPP.Concentrator) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server concentrator capabilities, as defined in [XEP-0326](http://xmpp.org/extensions/xep-0326.html). |
| **Waher.Networking.XMPP.Control** | The [Waher.Networking.XMPP.Control](Networking/Waher.Networking.XMPP.Control) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server control capabilities, as defined in [XEP-0325](http://xmpp.org/extensions/xep-0325.html). |
| **Waher.Networking.XMPP.Control.UWP** | The [Waher.Networking.XMPP.Control.UWP](Networking/Waher.Networking.XMPP.Control.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Control** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.HTTPX** | The [Waher.Networking.XMPP.HTTPX](Networking/Waher.Networking.XMPP.HTTPX) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server HTTPX support, as defined in [XEP-0332](http://xmpp.org/extensions/xep-0332.html). It also provides an HTTP proxy for tunneling HTTPX content through an HTTP(S)-based web server hosted by [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP). |
| **Waher.Networking.XMPP.Interoperability** | The [Waher.Networking.XMPP.Interoperability](Networking/Waher.Networking.XMPP.Interoperability) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server interoperability capabilities, as defined in this proto-XEP: [Internet of Things - Interoperability](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html) |
| **Waher.Networking.XMPP.Interoperability.UWP** | The [Waher.Networking.XMPP.Interoperability.UWP](Networking/Waher.Networking.XMPP.Interoperability.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Interoperability** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.P2P** | The [Waher.Networking.XMPP.P2P](Networking/Waher.Networking.XMPP.P2P) project provides classes that help the application do servless XMPP (peer-to-peer) communication, as defined in [XEP-0174](http://xmpp.org/extensions/xep-0174.html). |
| **Waher.Networking.XMPP.Provisioning** | The [Waher.Networking.XMPP.Provisioning](Networking/Waher.Networking.XMPP.Provisioning) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client support for provisioning and delegation of trust, as defined in [XEP-0324](http://xmpp.org/extensions/xep-0324.html). |
| **Waher.Networking.XMPP.Provisioning.UWP** | The [Waher.Networking.XMPP.Provisioning.UWP](Networking/Waher.Networking.XMPP.Provisioning.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Provisioning** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Sensor** | The [Waher.Networking.XMPP.Sensor](Networking/Waher.Networking.XMPP.Sensor) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server sensor capabilities, as defined in [XEP-0323](http://xmpp.org/extensions/xep-0323.html). |
| **Waher.Networking.XMPP.Sensor.UWP** | The [Waher.Networking.XMPP.Sensor.UWP](Networking/Waher.Networking.XMPP.Sensor.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Sensor** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Test** | The [Waher.Networking.XMPP.Test](Networking/Waher.Networking.XMPP.Test) project contains unit-tests for the [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) library and add-ons. |
| **Waher.Networking.XMPP.UWP** | The [Waher.Networking.XMPP.UWP](Networking/Waher.Networking.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Persistence
----------------------

The [Persistence](Persistence) folder contains libraries that manage data persistence in object databases.

| Project | Project description |
|-------------|---------------------|
| **Waher.Persistence** | The [Waher.Persistence](Persistence/Waher.Persistence) project provides the central interfaces for interaction with object databases. All modules can use the static **Database** class to persist and find objects in the preconfigured object database. |
| **Waher.Persistence.Files** | The [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project allows applications to persist data in files in a simple and efficient manner, through the **Waher.Persistence** library. |
| **Waher.Persistence.Files.Test** | The [Waher.Persistence.Files.Test](Persistence/Waher.Persistence.Files.Test) project contains unit tests for the [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project. |
| **Waher.Persistence.MongoDB** | The [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project provides a [MongoDB](https://www.mongodb.org/) database provider that can be used for object persistence through the **Waher.Persistence** library. |
| **Waher.Persistence.MongoDB.Test** | The [Waher.Persistence.MongoDB.Test](Persistence/Waher.Persistence.MongoDB.Test) project contains unit tests for the [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project. |

Runtime
----------------------

The [Runtime](Runtime) folder contains libraries that manage different aspects of the runtime environment.

| Project | Project description |
|-------------|---------------------|
| **Waher.Runtime.Cache** | The [Waher.Runtime.Cache](Runtime/Waher.Runtime.Cache) project provides tools for in-memory caching. |
| **Waher.Runtime.Cache.UWP** | The [Waher.Runtime.Cache.UWP](Runtime/Runtime.Cache.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Runtime.Cache** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Runtime.Language** | The [Waher.Runtime.Language](Runtime/Waher.Runtime.Language) project is a portable class library that helps applications with language localization. |
| **Waher.Runtime.Settings** | The [Waher.Runtime.Settings](Runtime/Waher.Runtime.Settings) project is a portable class library that helps applications maintain a set of persistent settings. |
| **Waher.Runtime.Timing** | The [Waher.Runtime.Timing](Runtime/Waher.Runtime.Timing) project provides tools for timing and scheduling. |
| **Waher.Runtime.Timing.UWP** | The [Waher.Runtime.Timing.UWP](Runtime/Runtime.Timing.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Runtime.Timing** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Script
----------------------

The [Script](Script) folder contains libraries that manage scripting.

| Project | Project description |
|-------------|---------------------|
| **Waher.Script** | The [Waher.Script](Script/Waher.Script) project is a class library that provides basic abstraction and execution model for symbolic math and scripting. It also manages pluggable modules and easy dynamic access to runtime namespaces and types. |
| **Waher.Script.Graphs** | The [Waher.Script.Graphs](Script/Waher.Script.Graphs) project is a class library that adds graphing functions to the script engine. |
| **Waher.Script.Persistence** | The [Waher.Script.Persistence](Script/Waher.Script.Persistence) project is a class library that allows access to the object database defined in the [Waher.Persistence](Persistence/Waher.Persistence) library in script. |
| **Waher.Script.Statisics** | The [Waher.Script.Statisics](Script/Waher.Script.Statisics) project is a class library that adds statistical functions to the script engine. |
| **Waher.Script.Test** | The [Waher.Script.Test](Script/Waher.ScriptTest) project contains unit tests for the script-related projects in this section. |
| **Waher.Script.UWP** | The [Waher.Script.UWP](Script/Waher.Script.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Script** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Security
----------------------

The [Security](Security) folder contains libraries that relate to security and encryption.

| Project | Project description |
|-------------|---------------------|
| **Waher.Security** | The [Waher.Security](Security/Waher.Security) project is a portable class library that provides a basic security model based on users, roles and privileges. It's not based on operating system features, to allow code to be platform independent. |

Services
----------------------

The [Services](Services) folder contains standalone service applications.

| Project | Project description |
|-------------|---------------------|
| **Waher.Service.PcSensor** | The [Waher.Service.PcSensor](Services/Waher.Service.PcSensor) project defines an application that converts your PC into an IoT sensor, by publishing performace counters as sensor values. [Full Screen Shot 1.](Images/Waher.Service.PcSensor.1.png) [Full Screen Shot 2.](Images/Waher.Service.PcSensor.2.png) [Full Screen Shot 3.](Images/Waher.Service.PcSensor.3.png) [Executable.](Executables/Waher.Service.PcSensor.zip) |
| **Waher.Service.GPIO** | The [Waher.Service.GPIO](Services/Waher.Service.GPIO) project defines a Universal Windows Platform application that can be installed on Windows 10 IoT devices. It will publish available GPIO inputs/outputs over XMPP sensor, control and chat interfaces. It will also publish Digital and Analog Arduino interfaces, if an Arduino using the Firmata protocol is connected to an USB port of the device. The application can be used to elaborate with GPIO peripherals using a simple chat client. |

Things
----------------------

The [Things](Things) folder contains libraries that manage data abstraction for things.

| Project | Project description |
|-------------|---------------------|
| **Waher.Things** | The [Waher.Things](Things/Waher.Things) project is a class library that provides basic abstraction of things, errors, sensor data and control operations. |
| **Waher.Things.Metering** | The [Waher.Things.Metering](Things/Waher.Things.Metering) project is a class library that defines a basic metering infrastructure. |
| **Waher.Things.UWP** | The [Waher.Things.UWP](Things/Waher.Things.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Things** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Utilities
----------------------

The [Utilities](Utilities) folder contains applications that help the developer or administrator with different tasks.

| Project | Project description |
|-------------|---------------------|
| **Waher.Utility.GetEmojiCatalog** | The [Waher.Utility.GetEmojiCatalog](Utilities/Waher.Utility.GetEmojiCatalog) project downloads an [emoji catalog](http://unicodey.com/emoji-data/table.htm) and extracts the information and generates code for handling emojis. |

Web Services
----------------------

The [WebServices](WebServices) folder contains modules that add web service capabilities to projects they are used in.

| Project | Project description |
|-------------|---------------------|
| **Waher.WebService.Script** | The [Waher.WebService.Script](WebServices/Waher.WebService.Script) project provides a web service that can be used to execute script on the server, from the client. |

## Unit Tests

All unit tests are run using [NUnit v2.6.4](http://nunit.org/?p=download).