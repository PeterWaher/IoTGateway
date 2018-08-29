IoTGateway
======================

**IoTGateway** is a C# implementation of an IoT gateway. It is self contained, and includes all libraries and frameworks 
it needs to operate. You can install it by using the following [IoT Gateway Setup application](Executables/IoTGatewaySetup.exe?raw=true).
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
* [Themes](#themes)
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

The [source code](https://github.com/PeterWaher/IoTGateway) and libraries provided in this repository is provided open for the following uses:

* For **Personal evaluation**. Personal evaluation means evaluating the code, its libraries and underlying technologies, including learning 
	about underlying technologies.

* For **Academic use**. If you want to use the following code for academic use, all you need to do is to inform the author of who you are, 
	what academic institution you work for (or study for), and in what projects you intend to use the code. All that is asked in return is for 
	an acknowledgement and visible attribution to this repository, including a link, and that you do not redistribute the source code, or parts 
	thereof in the solutions you develop. If any solutions developed in an academic setting, become commercial, it will need a commercial license.

* For **Security analysis**. If you perform any security analysis on the code, to see what security aspects the code might have, all that is 
	asked of you, is that you inform the author of any findings at least forty-five days before publication of the findings, so that any vulnerabilities 
	might be addressed. Such contributions are much appreciated and will be acknowledged.

Commercial use of the code, in part or in full, in compiled binary form, or its source code, requires
a **Commercial License**. Contact the author for details.

All rights to the source code are reserved and exclusively owned by [Waher Data AB](http://waher.se/). 
Any contributions made to the **IoT Gateway** repository become the intellectual property of [Waher Data AB](http://waher.se/).
If you're interested in using the source code, as a whole, or in part, you need a license agreement 
with the author. You can contact him through [LinkedIn](http://waher.se/).

This software is provided by the copyright holder and contributors "as is" and any express or implied warranties, including, but not limited to, 
the implied warranties of merchantability and fitness for a particular purpose are disclaimed. In no event shall the copyright owner or contributors 
be liable for any direct, indirect, incidental, special, exemplary, or consequential damages (including, but not limited to, procurement of substitute 
goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict 
liability, or tort (including negligence or otherwise) arising in any way out of the use of this software, even if advised of the possibility of such 
damage.

The **IoT Gateway** is &copy; [Waher Data AB](http://waher.se/) 2016-2018. All rights reserved.
 
[![](/Images/logo-WaherDataAB-300x58.png)](http://waher.se/)

IoT Gateway
----------------------

The IoT Gateway is represented by the following set of projects. They are back-end server applications and perform 
communiction with devices, as well as host online content.
You can install it by using the following [IoT Gateway Setup application](Executables/IoTGatewaySetup.exe?raw=true).

| Project                         | Type          | Link                                                                                   | Project description |
|---------------------------------|---------------|----------------------------------------------------------------------------------------|---------------------|
| **Waher.IoTClient.Setup**       | Wix           | [Installer](Executables/IoTClientSetup.exe?raw=true)                                   | The [Waher.IoTClient.Setup](Waher.IoTClient.Setup) project creates a Windows setup application that bootstraps several bundles into one setup application. Apart from installing the IoT Client, it also installs any prerequisites, such as the correct .NET framework. It is based on in [Wix framework](https://www.firegiant.com/wix/). |
| **Waher.IoTClient.Win32**       | Wix           | [Installer](Executables/Waher.IoTClient.Win32.msi?raw=true)                            | The [Waher.IoTClient.Win32](Waher.IoTClient.Win32) project creates a Windows MSI package that installs the Windows 32-bit files for the IoT Client. Project is based on in [Wix framework](https://www.firegiant.com/wix/). |
| **Waher.IoTGateway**            | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.IoTGateway/)                              | The [Waher.IoTGateway](Waher.IoTGateway) project is a class library that defines the IoT Gateway. The gateway can host any web content. It converts markdown to HTML in real-time. It can be administrated over XMPP using the [Waher.Client.WPF](Clients/Waher.Client.WPF) application. |
| **Waher.IoTGateway.App**        | UWP           | [App Bundle Package](Executables/Waher.IoTGateway.App_x86_x64_arm.appxbundle?raw=true) | The [Waher.IoTGateway.App](Waher.IoTGateway.App) project is a Universal Windows Platform application version of the IoT Gateway. It can be installed on IoT devices running Windows 10 IoT. |
| **Waher.IoTGateway.Console**    | .NET Core 2.0 | [Installer](Executables/IoTGatewaySetup.exe?raw=true)                                  | The [Waher.IoTGateway.Console](Waher.IoTGateway.Console) project is a console application version of the IoT Gateway. It's easy to use and experiment with. |
| **Waher.IoTGateway.Installers** | .NET 4.6.2    |                                                                                        | The [Waher.IoTGateway.Installers](Waher.IoTGateway.Installers) project defines custom actions used by the setup application to install the IoT Gateway and dependencies propertly. |
| **Waher.IoTGateway.Resources**  | .NET Std 1.0  |                                                                                        | The [Waher.IoTGateway.Resources](Waher.IoTGateway.Resources) project contains resource files that are common to all IoT Gateway embodiments. |
| **Waher.IoTGateway.Setup**      | Wix           | [Installer](Executables/IoTGatewaySetup.exe?raw=true)                                  | The [Waher.IoTGateway.Setup](Waher.IoTGateway.Setup) project creates a Windows setup application that bootstraps several bundles into one setup application. Apart from installing the IoT Gateway, it also installs any prerequisites, such as the correct .NET framework. It is based on in [Wix framework](https://www.firegiant.com/wix/). |
| **Waher.IoTGateway.Svc**        | .NET Core 2.0 | [Installer](Executables/IoTGatewaySetup.exe?raw=true)                                  | The [Waher.IoTGateway.Svc](Waher.IoTGateway.Svc) project is a Windows Service version version of the IoT Gateway. |
| **Waher.IoTGateway.Win32**      | Wix           | [Installer](Executables/Waher.IoTGateway.Win32.msi?raw=true)                           | The [Waher.IoTGateway.Win32](Waher.IoTGateway.Win32) project creates a Windows MSI package that installs the Windows 32-bit files for the IoT Gateway. Project is based on in [Wix framework](https://www.firegiant.com/wix/). |

Clients
----------------------

The [Clients](Clients) folder contains projects starting with **Waher.Client.** and denote client projects. Clients are front-end applications that 
can be run by users to perform different types of interaction with things or the network.

| Project                          | Type       | Link                                                            | Project description |
|----------------------------------|------------|-----------------------------------------------------------------|---------------------|
| **Waher.Client.WPF**             | .NET 4.6.2 | [Installer](Executables/IoTClientSetup.exe?raw=true)            | The [Waher.Client.WPF](Clients/Waher.Client.WPF) project is a simple IoT client that allows you to interact with things and users. If you connect to the network, you can chat with users and things. The client GUI is built using Windows Presentation Foundation (WPF). Chat sessions support normal plain text content, and rich content based on markdown. |
| **Waher.Client.MqttEventViewer** | .NET 4.6.2 | [Executable](Executables/Waher.Client.MqttEventViewer?raw=true) | The [Waher.Client.MqttEventViewer](Clients/Client.MqttEventViewer) project defines a simple WPF client application that subscribes to an MQTT topic and displays any events it receivs. Events are parsed as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |

Content
----------------------

The [Content](Content) folder contains libraries that Internet Content including parsing and rendering, and their
corresponding Internet Content Type encodings and decodings.

| Project                            | Type         | Link                                                                    | Project description |
|------------------------------------|--------------|-------------------------------------------------------------------------|---------------------|
| **Waher.Content**                  | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content/)                  | The [Waher.Content](Content/Waher.Content) project is a class library that provides basic abstraction for Internet Content Type, and basic encodings and decodings. This includes handling and parsing of common data types. |
| **Waher.Content.Emoji**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Emoji/)            | The [Waher.Content.Emoji](Content/Waher.Content.Emoji) project contains utilities for working with emojis. |
| **Waher.Content.Emoji.Emoji1**     | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Emoji.Emoji1/)     | The [Waher.Content.Emoji.Emoji1](Content/Waher.Content.Emoji.Emoji1) project provide free emojis from [Emoji One](http://emojione.com/) to content applications. |
| **Waher.Content.Html**             | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Html/)             | The [Waher.Content.Html](Content/Waher.Content.Html) project provides a simple HTML document parser that can be used to extract information from web pages. Social Meta-data can be easily extracted from page. Information is taken from Open Graph meta data or Twitter Card meta data, as well as standard HTML meta data. |
| **Waher.Content.Images**           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Images/)           | The [Waher.Content.Images](Content/Waher.Content.Images) project contains encoders and decoders for images. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Content.Markdown**         | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown/)         | The [Waher.Content.Markdown](Content/Waher.Content.Markdown) project parses markdown documents and transforms them to other formats, such as HTML, Plain text and XAML. For a description of the markdown flavour supported by the parser, see [Markdown documentation](Content/Waher.Content.Markdown/README.md). |
| **Waher.Content.Markdown.Web**     | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Web/)     | The [Waher.Content.Markdown.Web](Content/Waher.Content.Markdown.Web) project allows the publishing of web content using Markdown. The library converts Markdown documents in real-time to HTML when hosted using the web server defined in [Waher.Content.HTTP](Content/Waher.Content.HTTP). |
| **Waher.Content.Markdown.Web.UWP** | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Web.UWP/) | The [Waher.Content.Markdown.Web.UWP](Content/Waher.Content.Markdown.Web.UWP) project allows the publishing of web content using Markdown. The library converts Markdown documents in real-time to HTML when hosted using the web server defined in [Waher.Content.HTTP.UWP](Content/Waher.Content.HTTP.UWP). |
| **Waher.Content.Xml**              | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Content.Xml/)              | The [Waher.Content.Xml](Content/Waher.Content.Xml) project helps with encoding and decoding of XML. It integrates with the architecture defined in [Waher.Content](Content/Waher.Content). |
| **Waher.Content.Xsl**              | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Xsl/)              | The [Waher.Content.Xsl](Content/Waher.Content.Xsl) project helps with validating and transforming XML documents. It integrates with the architecture defined in [Waher.Content](Content/Waher.Content). |

The folder also contains the following unit test projects:

| Project                            | Type         | Project description |
|------------------------------------|--------------|---------------------|
| **Waher.Content.Html.Test**        | .NET 4.6.2   | The [Waher.Content.Html.Test](Content/Waher.Content.Html.Test) project contains unit tests for the **Waher.Content.Html** project. |
| **Waher.Content.Markdown.Test**    | .NET 4.6.2   | The [Waher.Content.Markdown.Test](Content/Waher.Content.Markdown.Test) project contains unit tests for the **Waher.Content.Markdown** project. |
| **Waher.Content.Test**             | .NET 4.6.2   | The [Waher.Content.Test](Content/Waher.Content.Test) project contains unit tests for the **Waher.Content** project. |


Events
----------------------

The [Events](Events) folder contains libraries that manage different aspects of event logging in networks.

| Project                          | Type         | Link                                                                  | Project description |
|----------------------------------|--------------|-----------------------------------------------------------------------|---------------------|
| **Waher.Events**                 | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events/)                 | The [Waher.Events](Events/Waher.Events) project provides the basic architecture and framework for event logging in applications. It uses the static class **Log** as a hub for all type of event logging in applications. To this hub you can register any number of **Event Sinks** that receive events and distribute them according to implementation details in each one. By logging all events to **Log** you have a configurable environment where you can change logging according to specific needs of the project. |
| **Waher.Events.Console**         | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.Console/)         | The [Waher.Events.Console](Events/Waher.Events.Console) project provides a simple event sink, that outputs events to the console standard output. Useful, if creating simple console applications. |
| **Waher.Events.Documentation**   | XML          |                                                                       | The [Waher.Events.Documentation](Events/Waher.Events.Documentation) project contains documentation of specific important events. This documentation includes Event IDs and any parameters they are supposed to include. |
| **Waher.Events.Files**           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.Files/)           | The [Waher.Events.Files](Events/Waher.Events.Files) project defines event sinks that outputs events to files. Supported formats are plain text and XML. XML files can be transformed using XSLT to other formats, such as HTML. |
| **Waher.Events.MQTT**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.MQTT/)            | The [Waher.Events.MQTT](Events/Waher.Events.MQTT) project defines an event sink that sends events to an MQTT topic. Events are sent as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.MQTT.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Events.MQTT.UWP/)        | The [Waher.Events.MQTT.UWP](Events/Waher.Events.MQTT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Events.MQTT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Events.Persistence**     | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.Persistence/)     | The [Waher.Events.Persistence](Events/Waher.Events.Persistence) project creates an even sink that stores incoming (logged) events in the local object database, as defined by [Waher.Persistence](Persistence/Waher.Persistence). Event life time in the database is defined in the constructor. Searches can be made for historical events. |
| **Waher.Events.Statistics**      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.Statistics/)      | The [Waher.Events.Statistics](Events/Waher.Events.Statistics) project defines an event sink that computes statistics of events being logged. |
| **Waher.Events.WindowsEventLog** | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.WindowsEventLog/) | The [Waher.Events.WindowsEventLog](Events/Waher.Events.WindowsEventLog) project defines an event sink that logs events to a Windows Event Log. |
| **Waher.Events.XMPP**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Events.XMPP/)            | The [Waher.Events.XMPP](Events/Waher.Events.XMPP) project defines an event sink that distributes events over XMPP, according to [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.XMPP.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Events.XMPP.UWP/)        | The [Waher.Events.XMPP.UWP](Events/Waher.Events.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Events.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

Mocks
----------------------

The [Mocks](Mocks) folder contains projects that implement different mock devices. These can be used as development tools to test technologies, 
implementation, networks and tools.

| Project                        | Type         | Link                                                    | Project description |
|--------------------------------|--------------|---------------------------------------------------------|---------------------|
| **Waher.Mock**                 | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Mock/)     | The [Waher.Mock](Mocks/Waher.Mock) project is a class library that provides support for simple mock applications. This includes simple network configuration. |
| **Waher.Mock.Lamp**            | .NET 4.6.2   |                                                         | The [Waher.Mock.Lamp](Mocks/Waher.Mock.Lamp) project simulates a simple lamp switch with an XMPP interface. |
| **Waher.Mock.Lamp.UWP**        | UWP          |                                                         | The [Waher.Mock.Lamp.UWP](Mocks/Waher.Mock.Lamp.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Lamp** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.Temperature**     | .NET 4.6.2   |                                                         | The [Waher.Mock.Temperature](Mocks/Waher.Mock.Temperature) project simulates a simple temperature sensor with an XMPP interface. |
| **Waher.Mock.Temperature.UWP** | UWP          |                                                         | The [Waher.Mock.Temperature.UWP](Mocks/Waher.Mock.Temperature.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Temperature** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.UWP**             | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Mock.UWP/) | The [Waher.Mock.UWP](Mocks/Waher.Mock.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Mock** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. It is limited in that it does not provide a console dialog for editing connection parameters if none exist. It does not include schema validation of XML configuration files either. |

Networking
----------------------

The [Networking](Networking) folder contains libraries that manage different aspects of network communication.

| Project                                        | Type         | Link                                                                               | Project description |
|------------------------------------------------|--------------|------------------------------------------------------------------------------------|---------------------|
| **Waher.Networking**                           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking/)                          | The [Waher.Networking](Networking/Waher.Networking) project provides the basic architecture and tools for all networking libraries.  This includes sniffers, etc. |
| **Waher.Networking.CoAP**                      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.CoAP/)                     | The [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) project provides a simple CoAP endpoint client with DTLS support. |
| **Waher.Networking.CoAP.UWP**                  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.CoAP.UWP/)                 | The [Waher.Networking.CoAP.UWP](Networking/Waher.Networking.CoAP.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.CoAP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.HTTP**                      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.HTTP/)                     | The [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) project provides a simple HTTP server for publishing dynamic content and managing user authentication based on a customizable set of users and privileges. Supports the WebSocket protocol. |
| **Waher.Networking.HTTP.UWP**                  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.HTTP.UWP/)                 | The [Waher.Networking.HTTP.UWP](Networking/Waher.Networking.HTTP.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.HTTP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.LWM2M**                     | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.LWM2M/)                    | The [Waher.Networking.LWM2M](Networking/Waher.Networking.LWM2M) project provides LWM2M interfaces for your application, using the CoAP library defined in [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP). |
| **Waher.Networking.LWM2M.UWP**                 | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.LWM2M.UWP/)                | The [Waher.Networking.LWM2M.UWP](Networking/Waher.Networking.LWM2M.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.LWM2M** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.MQTT**                      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.MQTT/)                     | The [Waher.Networking.MQTT](Networking/Waher.Networking.MQTT) project provides a simple MQTT client. |
| **Waher.Networking.MQTT.UWP**                  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.MQTT.UWP/)                 | The [Waher.Networking.MQTT.UWP](Networking/Waher.Networking.MQTT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.MQTT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.PeerToPeer**                | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.PeerToPeer/)               | The [Waher.Networking.PeerToPeer](Networking/Waher.Networking.PeerToPeer) project provides tools for peer-to-peer and multi-player communication. |
| **Waher.Networking.UPnP**                      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.UPnP/)                     | The [Waher.Networking.UPnP](Networking/Waher.Networking.UPnP) project provides tools for searching and interacting with devices in the local area network using the UPnP protocol. |
| **Waher.Networking.XMPP**                      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP/)                     | The [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) project provides a simple XMPP client. |
| **Waher.Networking.XMPP.BOSH**                 | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.BOSH/)                | The [Waher.Networking.XMPP.BOSH](Networking/Waher.Networking.XMPP.BOSH) project provides support for the HTTP altenative binding based on BOSH (defined in [XEP-0124](http://xmpp.org/extensions/xep-0124.html) and [XEP-0206](http://xmpp.org/extensions/xep-0206.html)) to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.BOSH.UWP**             | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.BOSH.UWP/)            | The [Waher.Networking.XMPP.BOSH.UWP](Networking/Waher.Networking.XMPP.BOSH.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.BOSH** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Chat**                 | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Chat/)                | The [Waher.Networking.XMPP.Chat](Networking/Waher.Networking.XMPP.Chat) project provides a simple XMPP chat server bot for things, that is added to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). It supports markdown, and follows the chat semantics outlined in this proto-XEP: [Chat Interface for Internet of Things Devices](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html) |
| **Waher.Networking.XMPP.Chat.UWP**             | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Chat.UWP/)            | The [Waher.Networking.XMPP.Chat.UWP](Networking/Waher.Networking.XMPP.Chat.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Chat** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Concentrator**         | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Concentrator/)        | The [Waher.Networking.XMPP.Concentrator](Networking/Waher.Networking.XMPP.Concentrator) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server concentrator capabilities, as defined in [XEP-0326](http://xmpp.org/extensions/xep-0326.html). The concentrator interface allows a device to manage a set of internal virtual devices, all sharing the same XMPP connection. |
| **Waher.Networking.XMPP.Concentrator.UWP**     | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Concentrator.UWP/)    | The [Waher.Networking.XMPP.Concentrator.UWP](Networking/Waher.Networking.XMPP.Concentrator.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Concentrator** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Control**              | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Control/)             | The [Waher.Networking.XMPP.Control](Networking/Waher.Networking.XMPP.Control) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server control capabilities, as defined in the [IEEE XMPP IoT extensions](https://gitlab.com/IEEE-SA/XMPPI/IoT). |
| **Waher.Networking.XMPP.Control.UWP**          | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Control.UWP/)         | The [Waher.Networking.XMPP.Control.UWP](Networking/Waher.Networking.XMPP.Control.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Control** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.HTTPX**                | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.HTTPX/)               | The [Waher.Networking.XMPP.HTTPX](Networking/Waher.Networking.XMPP.HTTPX) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server HTTPX support, as defined in [XEP-0332](http://xmpp.org/extensions/xep-0332.html). It also provides an HTTP proxy for tunneling HTTPX content through an HTTP(S)-based web server hosted by [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP). |
| **Waher.Networking.XMPP.Interoperability**     | .NET Std 1.3 |                                                                                    | The [Waher.Networking.XMPP.Interoperability](Networking/Waher.Networking.XMPP.Interoperability) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server interoperability capabilities, as defined in this proto-XEP: [Internet of Things - Interoperability](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html) |
| **Waher.Networking.XMPP.Interoperability.UWP** | UWP          |                                                                                    | The [Waher.Networking.XMPP.Interoperability.UWP](Networking/Waher.Networking.XMPP.Interoperability.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Interoperability** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.P2P**                  | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.P2P/)                 | The [Waher.Networking.XMPP.P2P](Networking/Waher.Networking.XMPP.P2P) project provides classes that help the application do servless XMPP (peer-to-peer) communication, as defined in [XEP-0174](http://xmpp.org/extensions/xep-0174.html). |
| **Waher.Networking.XMPP.PEP**                  | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PEP/)                 | The [Waher.Networking.XMPP.PEP](Networking/Waher.Networking.XMPP.PEP) project adds support for the Personal Eventing Protocol extension (XEP-0163) to the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.PEP.UWP**              | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PEP.UWP/)             | The [Waher.Networking.XMPP.PEP.UWP](Networking/Waher.Networking.XMPP.PEP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.PEP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Provisioning**         | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Provisioning/)        | The [Waher.Networking.XMPP.Provisioning](Networking/Waher.Networking.XMPP.Provisioning) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client support for provisioning and delegation of trust, as defined in the [IEEE XMPP IoT extensions](https://gitlab.com/IEEE-SA/XMPPI/IoT). |
| **Waher.Networking.XMPP.Provisioning.UWP**     | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Provisioning.UWP/)    | The [Waher.Networking.XMPP.Provisioning.UWP](Networking/Waher.Networking.XMPP.Provisioning.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Provisioning** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.PubSub**               | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PubSub/)              | The [Waher.Networking.XMPP.PubSub](Networking/Waher.Networking.XMPP.PubSub) project adds support for the Publish/Subscribe extension (XEP-0060) to the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.PubSub.UWP**           | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PubSub.UWP/)          | The [Waher.Networking.XMPP.PubSub.UWP](Networking/Waher.Networking.XMPP.PubSub.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.PubSub** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Sensor**               | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Sensor/)              | The [Waher.Networking.XMPP.Sensor](Networking/Waher.Networking.XMPP.Sensor) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server sensor capabilities, as defined in the [IEEE XMPP IoT extensions](https://gitlab.com/IEEE-SA/XMPPI/IoT). |
| **Waher.Networking.XMPP.Sensor.UWP**           | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Sensor.UWP/)          | The [Waher.Networking.XMPP.Sensor.UWP](Networking/Waher.Networking.XMPP.Sensor.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Sensor** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Synchronization**      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Synchronization/)     | The [Waher.Networking.XMPP.Synchronization](Networking/Waher.Networking.XMPP.Synchronization) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides clock synchronization capabilities, as defined in the [IEEE XMPP IoT extensions](https://gitlab.com/IEEE-SA/XMPPI/IoT). |
| **Waher.Networking.XMPP.Synchronization.UWP**  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Synchronization.UWP/) | The [Waher.Networking.XMPP.Synchronization.UWP](Networking/Waher.Networking.XMPP.Synchronization.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Synchronization** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.UWP**                  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.UWP/)                 | The [Waher.Networking.XMPP.UWP](Networking/Waher.Networking.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.WebSocket**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.WebSocket/)           | The [Waher.Networking.XMPP.WebSocket](Networking/Waher.Networking.XMPP.WebSocket) project provides support for the websocket altenative binding based on BOSH (defined in [RFC-7395](https://tools.ietf.org/html/rfc7395)) to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.WebSocket.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.WebSocket.UWP/)       | The [Waher.Networking.XMPP.WebSocket.UWP](Networking/Waher.Networking.XMPP.WebSocket.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.WebSocket** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

The folder also contains the following unit test projects:

| Project                            | Type         | Project description |
|------------------------------------|--------------|---------------------|
| **Waher.Networking.CoAP.Test**     | .NET 4.6.2   | The [Waher.Networking.CoAP.Test](Networking/Waher.Networking.CoAP.Test) project contains unit-tests for the [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) library. |
| **Waher.Networking.HTTP.Test**     | .NET 4.6.2   | The [Waher.Networking.HTTP.Test](Networking/Waher.Networking.HTTP.Test) project contains unit-tests for the [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) library. |
| **Waher.Networking.MQTT.Test**     | .NET 4.6.2   | The [Waher.Networking.MQTT.Test](Networking/Waher.Networking.MQTT.Test) project contains unit-tests for the [Waher.Networking.MQTT](Networking/Waher.Networking.MQTT) library. |
| **Waher.Networking.XMPP.Test**     | .NET 4.6.2   | The [Waher.Networking.XMPP.Test](Networking/Waher.Networking.XMPP.Test) project contains unit-tests for the [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) library and add-ons. |

Persistence
----------------------

The [Persistence](Persistence) folder contains libraries that create an infrastructure for persistence of objects in applications. 
This includes a simple embedded encrypted local object database, as well as integration with external databases. Objects are persisted based on 
their annotated class definitions.

| Project                            | Type         | Link                                                               | Project description |
|------------------------------------|--------------|--------------------------------------------------------------------|---------------------|
| **Waher.Persistence**              | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Persistence/)         | The [Waher.Persistence](Persistence/Waher.Persistence) project provides the central interfaces for interaction with object databases. All modules can use the static **Database** class to persist and find objects in the preconfigured object database. |
| **Waher.Persistence.Files**        | .NET Std 1.5 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.Files/)   | The [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project defines a library that provides an object database that stores objects in local AES-256 encrypted files. Storage, indices, searching and retrieval is based solely on meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Dynamic code is compiled. Access is provided through the [Waher.Persistence](Persistence/Waher.Persistence) library. |
| **Waher.Persistence.FilesLW**      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.FilesLW/) | The [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) project defines a library that provides an object database that stores objects in local files. Storage, indices, searching and retrieval is based solely on meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Access is provided through the [Waher.Persistence](Persistence/Waher.Persistence) library. |
| **Waher.Persistence.MongoDB**      | .NET 4.6.2   |                                                                    | The [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project provides a [MongoDB](https://www.mongodb.org/) database provider that can be used for object persistence through the **Waher.Persistence** library. |

The folder also contains the following unit test projects:

| Project                            | Type         | Project description |
|------------------------------------|--------------|---------------------|
| **Waher.Persistence.Files.Test**   | .NET 4.6.2   | The [Waher.Persistence.Files.Test](Persistence/Waher.Persistence.Files.Test) project contains unit tests for the [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project. |
| **Waher.Persistence.FilesLW.Test** | .NET 4.6.2   | The [Waher.Persistence.FilesLW.Test](Persistence/Waher.Persistence.FilesLW.Test) project contains unit tests for the [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) project. |
| **Waher.Persistence.MongoDB.Test** | .NET 4.6.2   | The [Waher.Persistence.MongoDB.Test](Persistence/Waher.Persistence.MongoDB.Test) project contains unit tests for the [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project. |

Runtime
----------------------

The [Runtime](Runtime) folder contains libraries that help applications with common runtime tasks, such as caching, maintaining a type inventory, 
language localization, runtime settings, timing and scheduling.

| Project                               | Type          | Link                                                                       | Project description |
|---------------------------------------|---------------|----------------------------------------------------------------------------|---------------------|
| **Waher.Runtime.Cache**               | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Cache/)               | The [Waher.Runtime.Cache](Runtime/Waher.Runtime.Cache) project provides tools for in-memory caching. |
| **Waher.Runtime.Inventory**           | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Inventory/)           | The [Waher.Runtime.Inventory](Runtime/Waher.Runtime.Inventory) project keeps an inventory of types and interfaces available in your code. It also provides a means to access available types given an interface. |
| **Waher.Runtime.Inventory.Loader**    | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Inventory.Loader/)    | The [Waher.Runtime.Inventory.Loader](Runtime/Waher.Runtime.Inventory.Loader) project dynamically loads modules from a folder, and initiates the inventory defined in [Waher.Runtime.Inventory](Runtime/Waher.Runtime.Inventory) with all loaded and referenced assemblies. |
| **Waher.Runtime.Language**            | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Language/)            | The [Waher.Runtime.Language](Runtime/Waher.Runtime.Language) project helps applications with language localization. |
| **Waher.Runtime.ServiceRegistration** | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.ServiceRegistration/) | The [Waher.Runtime.ServiceRegistration](Runtime/Waher.Runtime.ServiceRegistration) library allows applications to register themselves with an XMPP-based Service Registry, such as the [IoT Broker](https://waher.se/Broker.md). |
| **Waher.Runtime.Settings**            | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Settings/)            | The [Waher.Runtime.Settings](Runtime/Waher.Runtime.Settings) project helps applications maintain a set of persistent settings. |
| **Waher.Runtime.Timing**              | .NET Std 1.3  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Timing/)              | The [Waher.Runtime.Timing](Runtime/Waher.Runtime.Timing) project provides tools for timing and scheduling. |

The folder also contains the following unit test projects:

| Project                            | Type          | Project description |
|------------------------------------|---------------|---------------------|
| **Waher.Runtime.Language.Test**    | .NET Core 2.0 | The [Waher.Runtime.Language.Test](Runtime/Waher.Runtime.Language.Test) project contains unit tests for the [Waher.Runtime.Language](Runtime/Waher.Runtime.Language) project. |

Script
----------------------

The [Script](Script) folder contains libraries that define an extensible execution envionment for script supporting canonical extensions, .NET integration, 
graphs, physical units and unit conversions, etc. For more information about the script engine supported by these libraries, see the 
[script reference](https://waher.se/Script.md).


| Project                      | Type         | Link                                                              | Project description |
|------------------------------|--------------|-------------------------------------------------------------------|---------------------|
| **Waher.Script**             | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Script/)             | The [Waher.Script](Script/Waher.Script) project is a class library that provides basic abstraction and execution model for symbolic math and scripting. It also manages pluggable modules and easy dynamic access to runtime namespaces and types. |
| **Waher.Script.Content**     | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Script.Content/)     | The [Waher.Script.Content](Script/Waher.Script.Content) project is a class library that adds content functions to the script engine, suitable for loading, fetching or processing content from files or online resources. |
| **Waher.Script.Fractals**    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Fractals/)    | The [Waher.Script.Fractals](Script/Waher.Script.Fractals) project is a class library that adds fractal image functions to the script engine, suitable for generating backgound images. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.Graphs**      | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Script.Graphs/)      | The [Waher.Script.Graphs](Script/Waher.Script.Graphs) project is a class library that adds graphing functions to the script engine. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.Graphs3D**    | .NET Std 1.3 |                                                                   | The [Waher.Script.Graphs3D](Script/Waher.Script.Graphs3D) project is a class library that adds 3D-graphing functions to the script engine. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.Lab**         | .NET 4.6.2   | [Executable (x86)](Executables/Waher.Script.Lab.x86.zip?raw=true) | The [Waher.Script.Lab](Script/Waher.Script.Lab) project is a WPF application that allows you to experiment and work with script. |
| **Waher.Script.Persistence** | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Script.Persistence/) | The [Waher.Script.Persistence](Script/Waher.Script.Persistence) project is a class library that allows access to the object database defined in the [Waher.Persistence](Persistence/Waher.Persistence) library in script. |
| **Waher.Script.Statisics**   | .NET Std 1.5 | [NuGet](https://www.nuget.org/packages/Waher.Script.Statistics/)  | The [Waher.Script.Statisics](Script/Waher.Script.Statisics) project is a class library that adds statistical functions to the script engine. |

The folder also contains the following unit test projects:

| Project                            | Type         | Project description |
|------------------------------------|--------------|---------------------|
| **Waher.Script.Test**              | .NET 4.6.2   | The [Waher.Script.Test](Script/Waher.Script.Test) project contains unit tests for the script-related projects in this section. |

Security
----------------------

The [Security](Security) folder contains libraries that are dedicated at solving particular security or data protection such as authentication, 
authorization and encryption.

| Project                           | Type         | Link                                                                   | Project description |
|-----------------------------------|--------------|------------------------------------------------------------------------|---------------------|
| **Waher.Security**                | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security/)                | The [Waher.Security](Security/Waher.Security) project provides a basic security model based on users, roles and privileges. It's not based on operating system features, to allow code to be platform independent. |
| **Waher.Security.ACME**           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.ACME/)           | The [Waher.Security.ACME](https://github.com/PeterWaher/IoTGateway/tree/master/Security/Waher.Security.ACME) project contains a class library implementing the ACME v2 protocol for the generation of certificates using ACME-compliant certificate servers, as defined in the [ACME draft](https://tools.ietf.org/html/draft-ietf-acme-acme-13). |
| **Waher.Security.DTLS**           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.DTLS/)           | The [Waher.Security.DTLS](https://github.com/PeterWaher/IoTGateway/tree/master/Security/Waher.Security.DTLS) project contains a class library implementing the Datagram Transport Layer Security (DTLS) Version 1.2, as defined in [RFC 6347](https://tools.ietf.org/html/rfc6347). |
| **Waher.Security.EllipticCurves** | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.EllipticCurves/) | The [Waher.Security.EllipticCurves](https://github.com/PeterWaher/IoTGateway/tree/master/Security/Waher.Security.EllipticCurves) project contains a class library implementing algorithms for Elliptic Curve Cryptography. |
| **Waher.Security.JWS**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.JWS/)            | The [Waher.Security.JWS](Security/Waher.Security.JWS) project implements a framework for JSON Web Signatures (JWS), as defined in [RFC 7515](https://tools.ietf.org/html/rfc7515). |
| **Waher.Security.JWT**            | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.JWT/)            | The [Waher.Security.JWT](Security/Waher.Security.JWT) project helps applications with the creation and validation of Java Web Tokens (JWT), as defined in [RFC 7519](https://tools.ietf.org/html/rfc7519). |
| **Waher.Security.JWT.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Security.JWT.UWP/)        | The [Waher.Security.JWT.UWP](Security/Waher.Security.JWT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Security.JWT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Security.PKCS**           | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Security.PKCS/)           | The [Waher.Security.PKCS](https://github.com/PeterWaher/IoTGateway/tree/master/Security/Waher.Security.PKCS) project contains classes and tools for working with Public Key Cryptography Standards (PKCS). |

The folder also contains the following unit test projects:

| Project                                | Type          | Project description |
|----------------------------------------|---------------|---------------------|
| **Waher.Security.ACME.Test**           | .NET 4.6.2    | The [Waher.Security.ACME.Test](Security/Waher.Security.ACME.Test) project contains unit tests for the  [Waher.Security.ACME](Security/Waher.Security.ACME) project.                                         |
| **Waher.Security.DTLS.Test**           | .NET 4.6.2    | The [Waher.Security.DTLS.Test](Security/Waher.Security.DTLS.Test) project contains unit tests for the  [Waher.Security.DTLS](Security/Waher.Security.DTLS) project.                                         |
| **Waher.Security.EllipticCurves.Test** | .NET Core 2.0 | The [Waher.Security.EllipticCurves.Test](Security/Waher.Security.EllipticCurves.Test) project contains unit tests for the  [Waher.Security.EllipticCurves](Security/Waher.Security.EllipticCurves) project. |
| **Waher.Security.JWT.Test**            | .NET 4.6.2    | The [Waher.Security.JWT.Test](Security/Waher.Security.JWT.Test) project contains unit tests for the  [Waher.Security.JWT](Security/Waher.Security.JWT) project.                                             |
| **Waher.Security.PKCS.Test**           | .NET 4.6.2    | The [Waher.Security.PKCS.Test](Security/Waher.Security.PKCS.Test) project contains unit tests for the  [Waher.Security.PKCS](Security/Waher.Security.PKCS) project.                                         |

Services
----------------------

The [Services](Services) folder contains standalone service applications.

| Project                    | Type       | Link                                                          | Project description |
|----------------------------|------------|---------------------------------------------------------------|---------------------|
| **Waher.Service.GPIO**     | UWP        |                                                               | The [Waher.Service.GPIO](Services/Waher.Service.GPIO) project defines a Universal Windows Platform application that can be installed on Windows 10 IoT devices. It will publish available GPIO inputs/outputs over XMPP sensor, control and chat interfaces. It will also publish Digital and Analog Arduino interfaces, if an Arduino using the Firmata protocol is connected to an USB port of the device. The application can be used to elaborate with GPIO peripherals using a simple chat client. |
| **Waher.Service.PcSensor** | .NET 4.6.2 | [Executable](Executables/Waher.Service.PcSensor.zip?raw=true) | The [Waher.Service.PcSensor](Services/Waher.Service.PcSensor) project defines an application that converts your PC into an IoT sensor, by publishing performace counters as sensor values. [Full Screen Shot 1.](Images/Waher.Service.PcSensor.1.png) [Full Screen Shot 2.](Images/Waher.Service.PcSensor.2.png) [Full Screen Shot 3.](Images/Waher.Service.PcSensor.3.png) |

Themes
----------------------

The [Themes](Themes) folder contains libraries that contain content files for different visual themes.

| Project                       | Type         | Link                                                               | Project description |
|-------------------------------|--------------|--------------------------------------------------------------------|---------------------|
| **Waher.Theme.CactusRose**    | .NET Std 1.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.CactusRose/)    | The [Waher.Theme.CactusRose](Themes/Waher.Theme.CactusRose) project contains content files for the Cactus Rose theme. |
| **Waher.Theme.GothicPeacock** | .NET Std 1.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.GothicPeacock/) | The [Waher.Theme.GothicPeacock](Themes/Waher.Theme.GothicPeacock) project contains content files for the Gothic Peacock theme. |

Things
----------------------

The [Things](Things) folder contains libraries that define a hardware and data abstraction layer for interacting with things. This includes describing 
sensor data, control parameters, attributes, displayable parameters, commands, queries and data sources. It also includes embedding things dynamically,
to form more complex devices, such as concentrators or bridges.

| Project                   | Type         | Link                                                           | Project description |
|---------------------------|--------------|----------------------------------------------------------------|---------------------|
| **Waher.Things**          | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Things/)          | The [Waher.Things](Things/Waher.Things) project is a class library that provides basic abstraction of things, errors, sensor data and control operations. |
| **Waher.Things.Arduino**  | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Things.Arduino/)  | The [Waher.Things.Arduino](Things/Waher.Things.Arduino) project is a class library that publishes nodes for interaction with Arduinos and connected modules via Firmata. |
| **Waher.Things.Gpio**     | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Things.Gpio/)     | The [Waher.Things.Gpio](Things/Waher.Things.Gpio) project is a class library that publishes nodes for interaction with onboard General Purpose Input/Output (GPIO) modules. |
| **Waher.Things.Ip**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Things.Ip/)       | The [Waher.Things.Ip](Things/Waher.Things.Ip) project is a class library that publishes nodes representing nodes on an IP network. |
| **Waher.Things.Metering** | .NET Std 1.3 | [NuGet](https://www.nuget.org/packages/Waher.Things.Metering/) | The [Waher.Things.Metering](Things/Waher.Things.Metering) project is a class library that defines a basic metering infrastructure. |

Utilities
----------------------

The [Utilities](Utilities) folder contains applications that help the developer or administrator with different tasks.

| Project                            | Type          | Link  | Project description |
|------------------------------------|---------------|-------|---------------------|
| **Waher.Utility.Acme**             | .NET 4.6.2    |       | The [Waher.Utility.Acme](Utilities/Waher.Utility.Acme) is a command-line tool that helps you create certificates using the Automatic Certificate Management Environment (ACME) v2 protocol. |
| **Waher.Utility.AnalyzeClock**     | .NET Core 2.0 |       | The [Waher.Utility.AnalyzeClock](Utilities/Waher.Utility.AnalyzeClock) is a command-line tool that helps you analyze the difference in clocks between machines compatible with the [IEEE XMPP IoT extensions](https://gitlab.com/IEEE-SA/XMPPI/IoT). |
| **Waher.Utility.AnalyzeDB**        | .NET Core 2.0 |       | The [Waher.Utility.AnalyzeDB](Utilities/Waher.Utility.AnalyzeDB) is a command-line tool that helps you analyze an object database created by the [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) or [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) libraries, such as the IoT Gateway database. |
| **Waher.Utility.GetEmojiCatalog**  | .NET 4.6.2    |       | The [Waher.Utility.GetEmojiCatalog](Utilities/Waher.Utility.GetEmojiCatalog) project downloads an [emoji catalog](http://unicodey.com/emoji-data/table.htm) and extracts the information and generates code for handling emojis. |
| **Waher.Utility.Install**          | .NET Core 2.0 |       | The [Waher.Utility.Install](Utilities/Waher.Utility.Install) is a command-line tool that helps you install pluggable modules into the IoT Gateway. |
| **Waher.Utility.Transform**        | .NET Core 2.0 |       | The [Waher.Utility.Transform](Utilities/Waher.Utility.Transform) is a command-line tool that transforms an XML file utilizing an XSL Transform (XSLT). |

Web Services
----------------------

The [WebServices](WebServices) folder contains modules that add web service capabilities to projects they are used in.

| Project                     | Type         | Link  | Project description |
|-----------------------------|--------------|-------|---------------------|
| **Waher.WebService.Script** | .NET Std 1.5 |       | The [Waher.WebService.Script](WebServices/Waher.WebService.Script) project provides a web service that can be used to execute script on the server, from the client. |

## Unit Tests

Unit tests either use MSTest or [NUnit v2.6.4](http://nunit.org/?p=download).