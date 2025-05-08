IoTGateway
======================

**IoTGateway** is a C# implementation of an IoT gateway. It is self-contained, and includes all libraries and frameworks 
it needs to operate.

Apart from the [IoT Gateway](#iot-gateway) projects, the solution is divided into different groups of projects and modules:

* [Clients](#clients)
* [Content](#content)
* [Events](#events)
* [Layout](#layout)
* [Mocks](#mocks)
* [Networking](#networking)
* [Persistence](#persistence)
* [Reports](#reports)
* [Runtime](#runtime)
* [Script](#script)
* [Security](#security)
* [Services](#services)
* [Themes](#themes)
* [Things](#things)
* [Utilities](#utilities)
* [Web Services](#web-services)
* [Environment Variables](#environment-variables)
* [Compiling Solution](#compiling-solution)

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

**Note**: Distribution of code in source or compiled form, for purposes other than mentioned
above, is not considered personal use and requires a commercial license, even if distribution 
is made under an apparently free license. It facilitates the development of competing 
software, without the investment in actually performing the corresponding coding. It also 
can make the use of the original libraries obsolete, as free code apparently doing the same, 
based on the original libraries, would be available under an apparently free license. (Thus, 
making distribution free does not mitigate this effect.) Developers using the libraries to 
enhance their own projects (brands, offerings or businesses, even if the software itself is 
free), should therefore consider sponsoring the development of such software. It is the 
express intent of the developer of these libraries to create libraries that facilitate the 
development of great software for IoT. Also, the commercial license includes options to 
request customizations of the libraries.

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

The **IoT Gateway** is &copy; [Waher Data AB](http://waher.se/) 2016-2025. All rights reserved.
 
[![](/Images/logo-WaherDataAB-300x58.png)](http://waher.se/)

Mastering Internet of Things
-----------------------------------

Many of the libraries available in this repository contains are described and explained
in the book *Mastering Internet of Things* by Peter Waher. You can find the book
on [Packt](https://www.packtpub.com/networking-and-servers/mastering-internet-things),
[Amazon](https://www.amazon.com/Mastering-Internet-Things-Peter-Waher/dp/1788397487/),
[Bokus](https://www.bokus.com/bok/9781788397483/mastering-internet-of-things/)
and other stores.

![Mastering Internet of Things Book Cover](/Images/Cover.png)

The examples described in this book are available in a separate repository:
[MIoT](https://github.com/PeterWaher/MIoT)

Solution Files
------------------

The repository contains multiple solution files. Since the repo contains many different pojects, different solution files different subsets
of projects, making it easier to work with different aspects of the gateway, or on different platforms.

| Solution File | Description |
|:--------------|:------------|
| `IoTGateway.sln`     | Main repository. Contains references to all gateway projects, for most platforms.        |
| `IoTGatewayCore.sln` | Solution file that contains core repositories only (i.e. .NET Standard and .NET Core projects). Can be compiled using VS Code on multiple platforms. |
| `Cluster.sln`        | Contains repositories related to the cluster networking library, and their dependecies.  |
| `Content.sln`        | Contains Content-related repositories, and their dependecies.                            |
| `HTTP.sln`           | Contains repositories related to the HTTP networking library, and their dependecies.     |
| `Layout.sln`         | Contains Layout-related repositories, and their dependecies.                             |
| `Persistence.sln`    | Contains Persistence-related repositories, and their dependecies.                        |
| `Runtime.sln`        | Contains Runtime-related repositories, and their dependecies.                            |
| `Script.sln`         | Contains Script-related repositories, and their dependecies.                             |
| `XMPP.sln`           | Contains repositories related to the XMPP networking libraries, and their dependecies.   |


IoT Gateway
----------------------

The IoT Gateway is represented by the following set of projects. They are back-end server applications and perform 
communiction with devices, as well as host online content.

| Project                            | Type          | Link                                                      | Project description |
|------------------------------------|---------------|-----------------------------------------------------------|---------------------|
| **Waher.IoTGateway**               | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.IoTGateway/) | The [Waher.IoTGateway](Waher.IoTGateway) project is a class library that defines the IoT Gateway. The gateway can host any web content. It converts markdown to HTML in real-time. It can be administrated over XMPP using the [Waher.Client.WPF](Clients/Waher.Client.WPF) application. |
| **Waher.IoTGateway.Build**         | .NET 8.0      |                                                           | The [Waher.IoTGateway.Build](Waher.IoTGateway.Build) project contains MSBuild script for building setup files. Can be used in an auto-build environment. |
| **Waher.IoTGateway.Console**       | .NET 8.0      |                                                           | The [Waher.IoTGateway.Console](Waher.IoTGateway.Console) project is a console application version of the IoT Gateway. It's easy to use and experiment with. |
| **Waher.IoTGateway.Resources**     | .NET Std 2.0  |                                                           | The [Waher.IoTGateway.Resources](Waher.IoTGateway.Resources) project contains resource files that are common to all IoT Gateway embodiments. |
| **Waher.IoTGateway.Setup.Windows** | .NET 8.0      |                                                           | The [Waher.IoTGateway.Setup.Windows](Waher.IoTGateway.Setup.Windows) contains a Windows application that installs the IoT Gateway on a Windows machine. It can install multiple instances of the gateway. The project can be easily customized to install custom collections of packages. |
| **Waher.IoTGateway.Svc**           | .NET 8.0      |                                                           | The [Waher.IoTGateway.Svc](Waher.IoTGateway.Svc) project is a Windows Service version version of the IoT Gateway. |

Clients
----------------------

The [Clients](Clients) folder contains projects starting with **Waher.Client.** and denote client projects. Clients are front-end applications that 
can be run by users to perform different types of interaction with things or the network.

| Project                          | Type       | Project description |
|----------------------------------|------------|---------------------|
| **Waher.Client.WPF**             | .NET 8.0   | The [Waher.Client.WPF](Clients/Waher.Client.WPF) project is a simple IoT client that allows you to interact with things and users. If you connect to the network, you can chat with users and things. The client GUI is built using Windows Presentation Foundation (WPF). Chat sessions support normal plain text content, and rich content based on markdown. |
| **Waher.Client.MqttEventViewer** | .NET 8.0   | The [Waher.Client.MqttEventViewer](Clients/Client.MqttEventViewer) project defines a simple WPF client application that subscribes to an MQTT topic and displays any events it receivs. Events are parsed as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Script.Lab**             | .NET 8.0   | The [Waher.Script.Lab](Script/Waher.Script.Lab) project is a WPF application that allows you to experiment and work with script. |

Content
----------------------

The [Content](Content) folder contains libraries that handle Internet Content including parsing and rendering, using their
corresponding Internet Content Type encodings and decodings.

| Project                                  | Type         | Link                                                                          | Project description |
|------------------------------------------|--------------|-------------------------------------------------------------------------------|---------------------|
| **Waher.Content**                        | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content/)                        | The [Waher.Content](Content/Waher.Content) project is a class library that provides basic abstraction for Internet Content Type, and basic encodings and decodings. This includes handling and parsing of common data types. |
| **Waher.Content.Asn1**                   | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Asn1/)                   | The [Waher.Content.Asn1](Content/Waher.Content.Asn1) project implements a simple ASN.1 (Abstract Syntax Notation One) parser. The library supports generation of C# code from ASN.1 schemas. Encoding/Decoding schemes supported: BER, CER, DER. |
| **Waher.Content.Dsn**                    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Dsn/)                    | The [Waher.Content.Dsn](Content/Waher.Content.Dsn) project provides encoding and decoding of Delivery Status Notification (DSN) messages and message reports, as defined in RFC 3462 and 3464. |
| **Waher.Content.Emoji**                  | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Emoji/)                  | The [Waher.Content.Emoji](Content/Waher.Content.Emoji) project contains utilities for working with emojis. |
| **Waher.Content.Emoji.Emoji1**           | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Emoji.Emoji1/)           | The [Waher.Content.Emoji.Emoji1](Content/Waher.Content.Emoji.Emoji1) project provide free emojis from [Emoji One](http://emojione.com/) to content applications. |
| **Waher.Content.Html**                   | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Html/)                   | The [Waher.Content.Html](Content/Waher.Content.Html) project provides a simple HTML document parser that can be used to extract information from web pages. Social Meta-data can be easily extracted from page. Information is taken from Open Graph meta data or Twitter Card meta data, as well as standard HTML meta data. |
| **Waher.Content.Images**                 | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Images/)                 | The [Waher.Content.Images](Content/Waher.Content.Images) project contains encoders and decoders for images. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. Contains extraction of EXIF meta-data from images. |
| **Waher.Content.Markdown**               | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown/)               | The [Waher.Content.Markdown](Content/Waher.Content.Markdown) project can be used to parse Markdown documents and transforms them to other formats, such as HTML, Plain text and XAML. For a description of the markdown flavour supported by the parser, see [Markdown documentation](https://waher.se/Markdown.md). The library can also compare Markdown documents, and provide Markdown-based difference documents, showing how one version of a document is edited to produce a second version. [Reference](https://waher.se/Markdown.md) |
| **Waher.Content.Markdown.Consolidation** | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Consolidation/) | The [Waher.Content.Markdown.Consolidation](Content/Waher.Content.Markdown.Consolidation) project helps clients working with Markdown defined with the Markdown engine in [Waher.Content.Markdown](Content/Waher.Content.Markdown) to consolidate Markdown content originating from multiple sources, generating composite documents for more intuitive presentation. |
| **Waher.Content.Markdown.Contracts**     | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Contracts/)     | The [Waher.Content.Markdown.Contracts](Content/Waher.Content.Markdown.Contracts) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to Smart Contract XML, as defined by the Neuro-Foundation ([neuro-foundation.io](https://neuro-foundation.io)). |
| **Waher.Content.Markdown.GraphViz**      | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.GraphViz/)      | The [Waher.Content.Markdown.GraphViz](Content/Waher.Content.Markdown.GraphViz) project extends the capabilities of the Markdown engine defined in [Waher.Content.Markdown](Content/Waher.Content.Markdown). It allows for real-time inclusion and generation of [GraphViz](http://graphviz.org/) diagrams, if the software is installed on the system. [Markdown documentation](https://waher.se/Markdown.md#graphvizDiagrams). |
| **Waher.Content.Markdown.JavaScript**    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.JavaScript/)    | The [Waher.Content.Markdown.JavaScript](Content/Waher.Content.Markdown.JavaScript) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to JavaScript. Code generated can be used in browsers (for example) to dynamically generate DOM objects necessary to present the Markdown dynamically in a HTML page. |
| **Waher.Content.Markdown.Latex**         | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Latex/)         | The [Waher.Content.Markdown.Latex](Content/Waher.Content.Markdown.Latex) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to LaTeX. |
| **Waher.Content.Markdown.Layout2D**      | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Layout2D/)      | The [Waher.Content.Markdown.Layout2D](Content/Waher.Content.Markdown.Layout2D) project extends the capabilities of the Markdown engine defined in [Waher.Content.Markdown](Content/Waher.Content.Markdown). It allows for real-time inclusion and generation of [Waher.Layout.Layout2D](Layout/Waher.Layout.Layout2D) diagrams. [Markdown documentation](https://waher.se/Markdown.md#2dLayoutDiagrams). |
| **Waher.Content.Markdown.PlantUml**      | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.PlantUml/)      | The [Waher.Content.Markdown.PlantUml](Content/Waher.Content.Markdown.PlantUml) project extends the capabilities of the Markdown engine defined in [Waher.Content.Markdown](Content/Waher.Content.Markdown). It allows for real-time inclusion and generation of [PlantUML](https://plantuml.com/) diagrams, if the software is installed on the system. [Markdown documentation](https://waher.se/Markdown.md#umlWithPlantuml). |
| **Waher.Content.Markdown.SystemFiles**   | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.SystemFiles/)   | The [Waher.Content.Markdown.SystemFiles](Content/Waher.Content.Markdown.SystemFiles) project helps modules to find files and applications installed on the system, for integration purposes. |
| **Waher.Content.Markdown.Web**           | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Web/)           | The [Waher.Content.Markdown.Web](Content/Waher.Content.Markdown.Web) project allows the publishing of web content using Markdown. The library converts Markdown documents in real-time to HTML when hosted using the web server defined in [Waher.Networking.HTTP](Content/Waher.Networking.HTTP). |
| **Waher.Content.Markdown.Web.UWP**       | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Web.UWP/)       | The [Waher.Content.Markdown.Web.UWP](Content/Waher.Content.Markdown.Web.UWP) project allows the publishing of web content using Markdown. The library converts Markdown documents in real-time to HTML when hosted using the web server defined in [Waher.Networking.HTTP.UWP](Content/Waher.Networking.HTTP.UWP). |
| **Waher.Content.Markdown.Wpf**           | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Wpf/)           | The [Waher.Content.Markdown.Wpf](Content/Waher.Content.Markdown.Wpf) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to XAML (WPF flavour). |
| **Waher.Content.Markdown.Xamarin**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Xamarin/)       | The [Waher.Content.Markdown.Xamarin](Content/Waher.Content.Markdown.Xamarin) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to XAML (Xamarin Forms flavour). |
| **Waher.Content.Markdown.Xml**           | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Markdown.Xml/)           | The [Waher.Content.Markdown.Xml](Content/Waher.Content.Markdown.Xml) project extends the rendering capabilities of the Markdown library defined in Waher.Content.Markdown, by providing a renderer to XML. |
| **Waher.Content.QR**                     | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.QR/)                     | The [Waher.Content.QR](Content/Waher.Content.QR) contains a light-weight managed encoder of QR codes. It can generate both text-based output (using block characters) for display on text devices, as well as images and color-coded codes. |
| **Waher.Content.Rss**                    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Rss/)                    | The [Waher.Content.Rss](Content/Waher.Content.Rss) contains encoders and decoders of syndicalized information available in the RSS format. Architecture is extensible, allowing for customized encoding and decoding of content based on fully qualified names. The library supports RSS v0.91, v0.92, v2.0 and v2.0.1. |
| **Waher.Content.Semantic**               | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Semantic/)               | The [Waher.Content.Semantic](Content/Waher.Content.Semantic) contains encoders and decoders of semantic information, using well-known formats defined by the W3C for the semantic web, such as Turtle, RDF/XML and SPARQL results. Library also contains abstraction layer and extensiblea architecture for literals, as well as interfaces for semantic cubes for procedural access of semantic information. |
| **Waher.Content.SystemFiles**            | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.SystemFiles/)            | The [Waher.Content.SystemFiles](Content/Waher.Content.SystemFiles) helps modules to find files and applications installed on the system, for integration purposes. |
| **Waher.Content.Xml**                    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Xml/)                    | The [Waher.Content.Xml](Content/Waher.Content.Xml) project helps with encoding and decoding of XML. It integrates with the architecture defined in [Waher.Content](Content/Waher.Content). |
| **Waher.Content.Xsl**                    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Content.Xsl/)                    | The [Waher.Content.Xsl](Content/Waher.Content.Xsl) project helps with validating and transforming XML documents. It integrates with the architecture defined in [Waher.Content](Content/Waher.Content). |

The folder also contains the following unit test projects:

| Project                            | Type     | Project description |
|------------------------------------|----------|---------------------|
| **Waher.Content.Asn1.Test**        | .NET 8.0 | The [Waher.Content.Asn1.Test](Content/Waher.Content.Asn1.Test) project contains unit tests for the **Waher.Content.Asn1** project. |
| **Waher.Content.Html.Test**        | .NET 8.0 | The [Waher.Content.Html.Test](Content/Waher.Content.Html.Test) project contains unit tests for the **Waher.Content.Html** project. |
| **Waher.Content.Images.Test**      | .NET 8.0 | The [Waher.Content.Images.Test](Content/Waher.Content.Images.Test) project contains unit tests for the **Waher.Content.Images** project. |
| **Waher.Content.Markdown.Test**    | .NET 8.0 | The [Waher.Content.Markdown.Test](Content/Waher.Content.Markdown.Test) project contains unit tests for the **Waher.Content.Markdown** project. |
| **Waher.Content.QR.Test**          | .NET 8.0 | The [Waher.Content.QR.Test](Content/Waher.Content.QR.Test) project contains unit tests for the **Waher.Content.QR** project. |
| **Waher.Content.Rss.Test**         | .NET 8.0 | The [Waher.Content.Rss.Test](Content/Waher.Content.Rss.Test) project contains unit tests for the **Waher.Content.Rss** project. |
| **Waher.Content.Semantic.Test**    | .NET 8.0 | The [Waher.Content.Semantic.Test](Content/Waher.Content.Semantic.Test) project contains unit tests for the **Waher.Content.Semantic** project. |
| **Waher.Content.Test**             | .NET 8.0 | The [Waher.Content.Test](Content/Waher.Content.Test) project contains unit tests for the **Waher.Content** project. |


Events
----------------------

The [Events](Events) folder contains libraries that manage different aspects of event logging in networks.

| Project                          | Type         | Link                                                                  | Project description |
|----------------------------------|--------------|-----------------------------------------------------------------------|---------------------|
| **Waher.Events**                 | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events/)                 | The [Waher.Events](Events/Waher.Events) project provides the basic architecture and framework for event logging in applications. It uses the static class **Log** as a hub for all type of event logging in applications. To this hub you can register any number of **Event Sinks** that receive events and distribute them according to implementation details in each one. By logging all events to **Log** you have a configurable environment where you can change logging according to specific needs of the project. |
| **Waher.Events.Console**         | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.Console/)         | The [Waher.Events.Console](Events/Waher.Events.Console) project provides a simple event sink, that outputs events to the console standard output. Useful, if creating simple console applications. |
| **Waher.Events.Documentation**   | XML          |                                                                       | The [Waher.Events.Documentation](Events/Waher.Events.Documentation) project contains documentation of specific important events. This documentation includes Event IDs and any parameters they are supposed to include. |
| **Waher.Events.Files**           | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.Files/)           | The [Waher.Events.Files](Events/Waher.Events.Files) project defines event sinks that outputs events to files. Supported formats are plain text and XML. XML files can be transformed using XSLT to other formats, such as HTML. |
| **Waher.Events.Filter**          | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.Filter/)          | The [Waher.Events.Filter](Events/Waher.Events.Filter) project defines an event sink that filters incoming events before propagating them allowed events to another event sink. Use this in conjunction with another event sink to process only a subset of logged events using the second event sink. |
| **Waher.Events.MQTT**            | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Events.MQTT/)            | The [Waher.Events.MQTT](Events/Waher.Events.MQTT) project defines an event sink that sends events to an MQTT topic. Events are sent as XML fragments, according to the schema defined in [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.MQTT.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Events.MQTT.UWP/)        | The [Waher.Events.MQTT.UWP](Events/Waher.Events.MQTT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Events.MQTT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Events.Persistence**     | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.Persistence/)     | The [Waher.Events.Persistence](Events/Waher.Events.Persistence) project creates an even sink that stores incoming (logged) events in the local object database, as defined by [Waher.Persistence](Persistence/Waher.Persistence). Event life time in the database is defined in the constructor. Searches can be made for historical events. |
| **Waher.Events.Pipe**            | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Events.Pipe/)            | The [Waher.Events.Pipe](Events/Waher.Events.Pipe) project creates an even sink that sends incoming (logged) events to a pipe. An event reader is also available, making it easy to transport logged events from one process to another on the same machine. |
| **Waher.Events.Socket**          | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Events.Socket/)          | The [Waher.Events.Socket](Events/Waher.Events.Socket) project defines an event sink that sends events to a TCP listener socket. Events are sent as XML fragments. |
| **Waher.Events.Socket.UWP**      | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Events.Socket.UWP/)      | The [Waher.Events.Socket.UWP](Events/Waher.Events.Socket.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Events.Socket** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Events.Statistics**      | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.Statistics/)      | The [Waher.Events.Statistics](Events/Waher.Events.Statistics) project defines an event sink that computes statistics of events being logged. |
| **Waher.Events.WindowsEventLog** | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Events.WindowsEventLog/) | The [Waher.Events.WindowsEventLog](Events/Waher.Events.WindowsEventLog) project defines an event sink that logs events to a Windows Event Log. |
| **Waher.Events.XMPP**            | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Events.XMPP/)            | The [Waher.Events.XMPP](Events/Waher.Events.XMPP) project defines an event sink that distributes events over XMPP, according to [XEP-0337](http://xmpp.org/extensions/xep-0337.html). |
| **Waher.Events.XMPP.UWP**        | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Events.XMPP.UWP/)        | The [Waher.Events.XMPP.UWP](Events/Waher.Events.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Events.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

The folder also contains the following unit test projects:

| Project                      | Type     | Project description |
|------------------------------|----------|---------------------|
| **Waher.Events.Pipe.Test**   | .NET 8.0 | The [Waher.Events.Pipe.Test](Events/Waher.Events.Pipe.Test) project contains unit tests for the **Waher.Events.Pipe** project. |
| **Waher.Events.Socket.Test** | .NET 8.0 | The [Waher.Events.Socket.Test](Events/Waher.Events.Socket.Test) project contains unit tests for the **Waher.Events.Socket** project. |

Layout
----------------------

The [Layout](Layout) folder contains libraries for laying out objects visually.

| Project                            | Type         | Link                                                                    | Project description |
|------------------------------------|--------------|-------------------------------------------------------------------------|---------------------|
| **Waher.Layout.Layout2D**          | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Layout.Layout2D/)          | The [Waher.Layout.Layout2D](Layout/Waher.Layout.Layout2D) project provides an object model for laying out graphical objects in two dimensions. These models can then be used to generate images. The models can be represented in XML, and contains an XML schema that can be used to validate documents, as well as provide support when editing layout documents. |

The folder also contains the following unit test projects:

| Project                            | Type     | Project description |
|------------------------------------|----------|---------------------|
| **Waher.Layout.Layout2D.Test**     | .NET 8.0 | The [Waher.Layout.Layout2D.Test](Layout/Waher.Layout.Layout2D.Test) project contains unit tests for the **Waher.Layout.Layout2D** project. |

Mocks
----------------------

The [Mocks](Mocks) folder contains projects that implement different mock devices. These can be used as development tools to test technologies, 
implementation, networks and tools.

| Project                        | Type         | Link                                                    | Project description |
|--------------------------------|--------------|---------------------------------------------------------|---------------------|
| **Waher.Mock**                 | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Mock/)     | The [Waher.Mock](Mocks/Waher.Mock) project is a class library that provides support for simple mock applications. This includes simple network configuration. |
| **Waher.Mock.Lamp**            | .NET 8.0     |                                                         | The [Waher.Mock.Lamp](Mocks/Waher.Mock.Lamp) project simulates a simple lamp switch with an XMPP interface. |
| **Waher.Mock.Lamp.UWP**        | UWP          |                                                         | The [Waher.Mock.Lamp.UWP](Mocks/Waher.Mock.Lamp.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Lamp** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.Temperature**     | .NET 8.0     |                                                         | The [Waher.Mock.Temperature](Mocks/Waher.Mock.Temperature) project simulates a simple temperature sensor with an XMPP interface. |
| **Waher.Mock.Temperature.UWP** | UWP          |                                                         | The [Waher.Mock.Temperature.UWP](Mocks/Waher.Mock.Temperature.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Mock.Temperature** mock application. This application can be run on Windows 10, including on Rasperry Pi. |
| **Waher.Mock.UWP**             | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Mock.UWP/) | The [Waher.Mock.UWP](Mocks/Waher.Mock.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Mock** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. It is limited in that it does not provide a console dialog for editing connection parameters if none exist. It does not include schema validation of XML configuration files either. |

Networking
----------------------

The [Networking](Networking) folder contains libraries that manage different aspects of network communication.

| Project                                        | Type          | Link                                                                               | Project description |
|------------------------------------------------|---------------|------------------------------------------------------------------------------------|---------------------|
| **Waher.Networking**                           | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking/)                          | The [Waher.Networking](Networking/Waher.Networking) project provides the basic architecture and tools for all networking libraries. This includes sniffers, etc., as well as classes for building client and server applications. |
| **Waher.Networking.Cluster**                   | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.Cluster/)                  | The [Waher.Networking.Cluster](Networking/Waher.Networking.Cluster) project provides a framework for building applications that can cooperate and solve problems in clusters. Communication between endpoints in clusters is performed using AES-256 encrypted datagrams over a predefined UDP Multicast channel. Only participants with access to the shared key can participate in the cluster. Supports Unacknowledged, Acknowledged and Assured Message transfers in clusters, as well as Request/Response command executions, Locking of singleton resources, serialization of objects, etc. |
| **Waher.Networking.Cluster.ConsoleSandbox**    | .NET 8.0      |                                                                                    | The [Waher.Networking.Cluster.ConsoleSandbox](Networking/Waher.Networking.Cluster.ConsoleSandbox) project provides a simple console application that allows you to interactively test the cluster protocol in the network. |
| **Waher.Networking.CoAP**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.CoAP/)                     | The [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) project provides a simple CoAP endpoint client with DTLS support. |
| **Waher.Networking.CoAP.UWP**                  | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.CoAP.UWP/)                 | The [Waher.Networking.CoAP.UWP](Networking/Waher.Networking.CoAP.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.CoAP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.DNS**                       | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.DNS/)                      | The [Waher.Networking.DNS](Networking/Waher.Networking.DNS) project provides a class library for resolving DNS host, mailbox and service names on the network. It also supports reverse address lookups, International Domain Names (IDN), DNS Black Lists (DNSBL), text records, and maintains a local Resource Record cache. |
| **Waher.Networking.HTTP**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.HTTP/)                     | The [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) project provides a simple HTTP server for publishing dynamic content and managing user authentication based on a customizable set of users and privileges. Supports the WebSocket protocol. |
| **Waher.Networking.HTTP.Brotli**               | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.HTTP.Brotli/)              | The [Waher.Networking.HTTP.Brotli](Networking/Waher.Networking.HTTP.Brotli) project adds Brotli-compression capabilities to the HTTP server defined in the **Waher.Networking.HTTP** Library. |
| **Waher.Networking.HTTP.UWP**                  | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.HTTP.UWP/)                 | The [Waher.Networking.HTTP.UWP](Networking/Waher.Networking.HTTP.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.HTTP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.LWM2M**                     | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.LWM2M/)                    | The [Waher.Networking.LWM2M](Networking/Waher.Networking.LWM2M) project provides LWM2M interfaces for your application, using the CoAP library defined in [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP). |
| **Waher.Networking.LWM2M.UWP**                 | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.LWM2M.UWP/)                | The [Waher.Networking.LWM2M.UWP](Networking/Waher.Networking.LWM2M.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.LWM2M** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.Modbus**                    | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.Modbus/)                   | The [Waher.Networking.Modbus](Networking/Waher.Networking.Modbus) project provides a simple Modbus client. |
| **Waher.Networking.Modbus.UWP**                | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.Modbus.UWP/)               | The [Waher.Networking.Modbus.UWP](Networking/Waher.Networking.Modbus.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.Modbus** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.MQTT**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.MQTT/)                     | The [Waher.Networking.MQTT](Networking/Waher.Networking.MQTT) project provides a simple MQTT client. |
| **Waher.Networking.MQTT.UWP**                  | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.MQTT.UWP/)                 | The [Waher.Networking.MQTT.UWP](Networking/Waher.Networking.MQTT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.MQTT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.PeerToPeer**                | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.PeerToPeer/)               | The [Waher.Networking.PeerToPeer](Networking/Waher.Networking.PeerToPeer) project provides tools for peer-to-peer and multi-player communication. |
| **Waher.Networking.SASL**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.SASL/)                     | The [Waher.Networking.SASL](Networking/Waher.Networking.SASL) project implements Simple Authentication and Security Layer (SASL) mechanisms. |
| **Waher.Networking.SMTP**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.SMTP/)                     | The [Waher.Networking.SMTP](Networking/Waher.Networking.SMTP) project implements a simple e-mail client based on the SMTP protocol, that can send formatted e-mail messages with attachments. |
| **Waher.Networking.UPnP**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.UPnP/)                     | The [Waher.Networking.UPnP](Networking/Waher.Networking.UPnP) project provides tools for searching and interacting with devices in the local area network using the UPnP protocol. |
| **Waher.Networking.WHOIS**                     | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.WHOIS/)                    | The [Waher.Networking.WHOIS](Networking/Waher.Networking.WHOIS) project implements a [WHOIS](https://tools.ietf.org/html/rfc3912) client that can be used to query Regional Internet Registries for information relating to IP addresses, etc. |
| **Waher.Networking.WHOIS.UWP**                 | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.WHOIS.UWP/)                | The [Waher.Networking.WHOIS.UWP](Networking/Waher.Networking.WHOIS.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Networking.WHOIS** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP**                      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP/)                     | The [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) project provides a simple XMPP client. |
| **Waher.Networking.XMPP.Avatar**               | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Avatar/)              | The [Waher.Networking.XMPP.Avatar](Networking/Waher.Networking.XMPP.Avatar) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on helps the client manage avatars. |
| **Waher.Networking.XMPP.Avatar.UWP**           | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Avatar.UWP/)          | The [Waher.Networking.XMPP.Avatar.UWP](Networking/Waher.Networking.XMPP.Avatar.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Avatar** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.BOSH**                 | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.BOSH/)                | The [Waher.Networking.XMPP.BOSH](Networking/Waher.Networking.XMPP.BOSH) project provides support for the HTTP altenative binding based on BOSH (defined in [XEP-0124](http://xmpp.org/extensions/xep-0124.html) and [XEP-0206](http://xmpp.org/extensions/xep-0206.html)) to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.BOSH.UWP**             | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.BOSH.UWP/)            | The [Waher.Networking.XMPP.BOSH.UWP](Networking/Waher.Networking.XMPP.BOSH.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.BOSH** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Chat**                 | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Chat/)                | The [Waher.Networking.XMPP.Chat](Networking/Waher.Networking.XMPP.Chat) project provides a simple XMPP chat server bot for things, that is added to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). It supports markdown, and follows the chat semantics outlined in this proto-XEP: [Chat Interface for Internet of Things Devices](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Chat.html) |
| **Waher.Networking.XMPP.Chat.UWP**             | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Chat.UWP/)            | The [Waher.Networking.XMPP.Chat.UWP](Networking/Waher.Networking.XMPP.Chat.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Chat** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Concentrator**         | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Concentrator/)        | The [Waher.Networking.XMPP.Concentrator](Networking/Waher.Networking.XMPP.Concentrator) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server concentrator capabilities, as defined in [XEP-0326](http://xmpp.org/extensions/xep-0326.html). The concentrator interface allows a device to manage a set of internal virtual devices, all sharing the same XMPP connection. |
| **Waher.Networking.XMPP.Concentrator.UWP**     | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Concentrator.UWP/)    | The [Waher.Networking.XMPP.Concentrator.UWP](Networking/Waher.Networking.XMPP.Concentrator.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Concentrator** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Contracts**            | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Contracts/)           | The [Waher.Networking.XMPP.Contracts](Networking/Waher.Networking.XMPP.Contracts) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client interfaces for managing legal identities, smart contracts and signatures, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Control**              | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Control/)             | The [Waher.Networking.XMPP.Control](Networking/Waher.Networking.XMPP.Control) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server control capabilities, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Control.UWP**          | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Control.UWP/)         | The [Waher.Networking.XMPP.Control.UWP](Networking/Waher.Networking.XMPP.Control.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Control** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.HTTPX**                | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.HTTPX/)               | The [Waher.Networking.XMPP.HTTPX](Networking/Waher.Networking.XMPP.HTTPX) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server HTTPX support, as defined in [XEP-0332](http://xmpp.org/extensions/xep-0332.html). It also provides an HTTP proxy for tunneling HTTPX content through an HTTP(S)-based web server hosted by [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP). |
| **Waher.Networking.XMPP.Interoperability**     | .NET Std 2.1  |                                                                                    | The [Waher.Networking.XMPP.Interoperability](Networking/Waher.Networking.XMPP.Interoperability) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server interoperability capabilities, as defined in this proto-XEP: [Internet of Things - Interoperability](http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html) |
| **Waher.Networking.XMPP.Interoperability.UWP** | UWP           |                                                                                    | The [Waher.Networking.XMPP.Interoperability.UWP](Networking/Waher.Networking.XMPP.Interoperability.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Interoperability** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Mail**                 | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Mail/)                | The [Waher.Networking.XMPP.Mail](https://github.com/PeterWaher/IoTGateway/tree/master/Networking/Waher.Networking.XMPP.Mail) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](https://github.com/PeterWaher/IoTGateway/tree/master/Networking/Waher.Networking.XMPP). This add-on provides client support for mail extensions on XMPP servers. |
| **Waher.Networking.XMPP.MUC**                  | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.MUC/)                 | The [Waher.Networking.XMPP.MUC](Networking/Waher.Networking.XMPP.MUC) project adds support for the Multi-User-Chat (MUC) extension (XEP-0045) to the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). Direct invitations (XEP-0249), and Self-Ping (XEP-410) are also supported. |
| **Waher.Networking.XMPP.MUC.UWP**              | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.MUC.UWP/)             | The [Waher.Networking.XMPP.MUC.UWP](Networking/Waher.Networking.XMPP.MUC.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.MUC** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.P2P**                  | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.P2P/)                 | The [Waher.Networking.XMPP.P2P](Networking/Waher.Networking.XMPP.P2P) project provides classes that help the application do servless XMPP (peer-to-peer) communication, as defined in [XEP-0174](http://xmpp.org/extensions/xep-0174.html). |
| **Waher.Networking.XMPP.PEP**                  | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PEP/)                 | The [Waher.Networking.XMPP.PEP](Networking/Waher.Networking.XMPP.PEP) project adds support for the Personal Eventing Protocol extension (XEP-0163) to the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.PEP.UWP**              | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PEP.UWP/)             | The [Waher.Networking.XMPP.PEP.UWP](Networking/Waher.Networking.XMPP.PEP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.PEP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Provisioning**         | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Provisioning/)        | The [Waher.Networking.XMPP.Provisioning](Networking/Waher.Networking.XMPP.Provisioning) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client support for provisioning and delegation of trust, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Provisioning.UWP**     | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Provisioning.UWP/)    | The [Waher.Networking.XMPP.Provisioning.UWP](Networking/Waher.Networking.XMPP.Provisioning.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Provisioning** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.PubSub**               | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PubSub/)              | The [Waher.Networking.XMPP.PubSub](Networking/Waher.Networking.XMPP.PubSub) project adds support for the Publish/Subscribe extension (XEP-0060) to the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.PubSub.UWP**           | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.PubSub.UWP/)          | The [Waher.Networking.XMPP.PubSub.UWP](Networking/Waher.Networking.XMPP.PubSub.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.PubSub** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Push**                 | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Push/)                | The [Waher.Networking.XMPP.Push](Networking/Waher.Networking.XMPP.Push) project adds client-side support for Push Notification when the client is not connected over XMPP. Implemented for the XMPP Client library defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). Push Notification provided by the TAG Neuron. Allows the client to forward different types of messages, and contents, into different notification channels, as defined by mobile platforms, when client is offline. |
| **Waher.Networking.XMPP.RDP**                  | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.RDP/)                 | The [Waher.Networking.XMPP.RDP](Networking/Waher.Networking.XMPP.RDP) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client-side support for Remote Desktop over XMPP. |
| **Waher.Networking.XMPP.Sensor**               | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Sensor/)              | The [Waher.Networking.XMPP.Sensor](Networking/Waher.Networking.XMPP.Sensor) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides client and server sensor capabilities, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Sensor.UWP**           | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Sensor.UWP/)          | The [Waher.Networking.XMPP.Sensor.UWP](Networking/Waher.Networking.XMPP.Sensor.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Sensor** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.Software**             | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Software/)            | The [Waher.Networking.XMPP.Software](Networking/Waher.Networking.XMPP.Software) project provides a client for managing and downloading software packages and software updates, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Synchronization**      | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Synchronization/)     | The [Waher.Networking.XMPP.Synchronization](Networking/Waher.Networking.XMPP.Synchronization) project provides an add-on to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). This add-on provides clock synchronization capabilities, as defined in the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Networking.XMPP.Synchronization.UWP**  | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.Synchronization.UWP/) | The [Waher.Networking.XMPP.Synchronization.UWP](Networking/Waher.Networking.XMPP.Synchronization.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.Synchronization** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.UWP**                  | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.UWP/)                 | The [Waher.Networking.XMPP.UWP](Networking/Waher.Networking.XMPP.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Networking.XMPP.WebSocket**            | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.WebSocket/)           | The [Waher.Networking.XMPP.WebSocket](Networking/Waher.Networking.XMPP.WebSocket) project provides support for the websocket altenative binding based on BOSH (defined in [RFC-7395](https://tools.ietf.org/html/rfc7395)) to the XMPP client defined in [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP). |
| **Waher.Networking.XMPP.WebSocket.UWP**        | UWP           | [NuGet](https://www.nuget.org/packages/Waher.Networking.XMPP.WebSocket.UWP/)       | The [Waher.Networking.XMPP.WebSocket.UWP](Networking/Waher.Networking.XMPP.WebSocket.UWP) project provides a reduced Universal Windows Platform compatible version of the **Waher.Networking.XMPP.WebSocket** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |

The folder also contains the following unit test projects:

| Project                              | Type     | Project description |
|--------------------------------------|----------|---------------------|
| **Waher.Networking.Test**            | .NET 8.0 | The [Waher.Networking.Test](Networking/Waher.Networking.Test) project contains unit-tests for the [Waher.Networking](Networking/Waher.Networking) library. |
| **Waher.Networking.Cluster.Test**    | .NET 8.0 | The [Waher.Networking.Cluster.Test](Networking/Waher.Networking.Cluster.Test) project contains unit-tests for the [Waher.Networking.Cluster](Networking/Waher.Networking.Cluster) library. |
| **Waher.Networking.CoAP.Test**       | .NET 8.0 | The [Waher.Networking.CoAP.Test](Networking/Waher.Networking.CoAP.Test) project contains unit-tests for the [Waher.Networking.CoAP](Networking/Waher.Networking.CoAP) library. |
| **Waher.Networking.DNS.Test**        | .NET 8.0 | The [Waher.Networking.DNS.Test](Networking/Waher.Networking.DNS.Test) project contains unit-tests for the [Waher.Networking.DNS](Networking/Waher.Networking.DNS) library. |
| **Waher.Networking.HTTP.Test**       | .NET 8.0 | The [Waher.Networking.HTTP.Test](Networking/Waher.Networking.HTTP.Test) project contains unit-tests for the [Waher.Networking.HTTP](Networking/Waher.Networking.HTTP) library. |
| **Waher.Networking.HTTP.TestServer** | .NET 8.0 | The [Waher.Networking.HTTP.TestServer](Networking/Waher.Networking.HTTP.TestServer) project is a console application that hosts a simple web server using the [Waher.Networking.HTTP](Networking/Waher.Networking.CoAP) library, for testing purposes using external unit test tools, such as the [h2spec](https://github.com/summerwind/h2spec) tool and [similar](https://github.com/httpwg/wiki/wiki/HTTP-Testing-Resources). |
| **Waher.Networking.Modbus.Test**     | .NET 8.0 | The [Waher.Networking.Modbus.Test](Networking/Waher.Networking.Modbus.Test) project contains unit-tests for the [Waher.Networking.Modbus](Networking/Waher.Networking.Modbus) library. |
| **Waher.Networking.MQTT.Test**       | .NET 8.0 | The [Waher.Networking.MQTT.Test](Networking/Waher.Networking.MQTT.Test) project contains unit-tests for the [Waher.Networking.MQTT](Networking/Waher.Networking.MQTT) library. |
| **Waher.Networking.WHOIS.Test**      | .NET 8.0 | The [Waher.Networking.WHOIS.Test](Networking/Waher.Networking.WHOIS.Test) project contains unit-tests for the [Waher.Networking.WHOIS](Networking/Waher.Networking.WHOIS) library. |
| **Waher.Networking.XMPP.Test**       | .NET 8.0 | The [Waher.Networking.XMPP.Test](Networking/Waher.Networking.XMPP.Test) project contains unit-tests for the [Waher.Networking.XMPP](Networking/Waher.Networking.XMPP) library and add-ons. |

Persistence
----------------------

The [Persistence](Persistence) folder contains libraries that create an infrastructure for persistence of objects in applications. 
This includes a simple embedded encrypted local object database, as well as integration with external databases. Objects are persisted based on 
their annotated class definitions.

| Project                                      | Type         | Link                                                                              | Project description |
|----------------------------------------------|--------------|-----------------------------------------------------------------------------------|---------------------|
| **Waher.Persistence**                        | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence/)                        | The [Waher.Persistence](Persistence/Waher.Persistence) project provides the central interfaces for interaction with object databases. All modules can use the static **Database** class to persist and find objects in the preconfigured object database. |
| **Waher.Persistence.Files**                  | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.Files/)                  | The [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project defines a library that provides an object database that stores objects in local AES-256 encrypted files. Storage, indices, searching and retrieval is based solely on meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Dynamic code is compiled. Access is provided through the [Waher.Persistence](Persistence/Waher.Persistence) library. |
| **Waher.Persistence.FilesLW**                | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.FilesLW/)                | The [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) project defines a library that provides an object database that stores objects in local files. Storage, indices, searching and retrieval is based solely on meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Access is provided through the [Waher.Persistence](Persistence/Waher.Persistence) library. |
| **Waher.Persistence.FullTextSearch**         | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.FullTextSearch/)         | The [Waher.Persistence.FullTextSearch](Persistence/Waher.Persistence.FullTextSearch) project provides full-text-search capabilities to the data you persist using the [Waher.Persistence](Persistence/Waher.Persistence) library, regardless of what database provider you use. The full-text-search engine can both index objects, through their class definitions, objects generated ex-nihilo through the script engine, or content available in files. Architecture is pluggable, and you can extend the tokenization process by providing cusrtom tokenizers for different classes and file types. |
| **Waher.Persistence.MongoDB**                | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.MongoDB/)                | The [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project provides a [MongoDB](https://www.mongodb.org/) database provider that can be used for object persistence through the [Waher.Persistence](Persistence/Waher.Persistence) library. |
| **Waher.Persistence.Serialization**          | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.Serialization/)          | The [Waher.Persistence.Serialization](Persistence/Waher.Persistence.Serialization) project defines a library that serializes objects to binary form using meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Compatible with Waher.Persistence.Serialization.Compiled. |
| **Waher.Persistence.Serialization.Compiled** | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.Serialization.Compiled/) | The [Waher.Persistence.Serialization.Compiled](Persistence/Waher.Persistence.Serialization.Compiled) project defines a library that serializes objects to binary form using meta-data provided through the corresponding class definitions. Object serializers are created dynamically. Dynamic code is compiled. Compatible with Waher.Persistence.Serialization. |
| **Waher.Persistence.XmlLedger**              | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Persistence.XmlLedger/)              | The [Waher.Persistence.XmlLedger](Persistence/Waher.Persistence.XmlLedger) project provides a simple ledger that records anything that happens in the database to XML files in the program data folder. Files are kept of a configurable amount of time. |

The folder also contains the following unit test projects:

| Project                                   | Type     | Project description |
|-------------------------------------------|----------|---------------------|
| **Waher.Persistence.Files.Test**          | .NET 8.0 | The [Waher.Persistence.Files.Test](Persistence/Waher.Persistence.Files.Test) project contains unit tests for the [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) project. |
| **Waher.Persistence.FilesLW.Test**        | .NET 8.0 | The [Waher.Persistence.FilesLW.Test](Persistence/Waher.Persistence.FilesLW.Test) project contains unit tests for the [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) project. |
| **Waher.Persistence.FullTextSearch.Test** | .NET 8.0 | The [Waher.Persistence.FullTextSearch.Test](Persistence/Waher.Persistence.FullTextSearch.Test) project contains unit tests for the [Waher.Persistence.FullTextSearch](Persistence/Waher.Persistence.FullTextSearch) project. |
| **Waher.Persistence.MongoDB.Test**        | .NET 8.0 | The [Waher.Persistence.MongoDB.Test](Persistence/Waher.Persistence.MongoDB.Test) project contains unit tests for the [Waher.Persistence.MongoDB](Persistence/Waher.Persistence.MongoDB) project. |
| **Waher.Persistence.XmlLedger.Test**      | .NET 8.0 | The [Waher.Persistence.XmlLedger.Test](Persistence/Waher.Persistence.XmlLedger.Test) project contains unit tests for the [Waher.Persistence.XmlLedger](Persistence/Waher.Persistence.XmlLedger) project. |

Reports
----------------------

The [Reports](Reports) folder contains libraries that define an abstraction layer for administrative reports, as well as implementation of reports 
to administrators can use to get insight into the operation of the gateway. The abstraction layer ties into the harmonized abstraction layer 
defined by **Waher.Things**.

| Project                   | Type          | Link                                                           | Project description |
|---------------------------|---------------|----------------------------------------------------------------|---------------------|
| **Waher.Reports**         | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Reports/)         | The [Waher.Reports](Things/Waher.Reports) project is a class library that provides basic abstraction of reports. |
| **Waher.Reports.Files**   | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Reports.Files/)   | The [Waher.Reports.Files](Things/Waher.Reports.Files) project is a class library that publishes file-based reports stored in application data folders. Reports use XML to define structure and script to define logic. |

Runtime
----------------------

The [Runtime](Runtime) folder contains libraries that help applications with common runtime tasks, such as caching, maintaining a type inventory, 
language localization, runtime settings, timing and scheduling.

| Project                               | Type          | Link                                                                       | Project description |
|---------------------------------------|---------------|----------------------------------------------------------------------------|---------------------|
| **Waher.Runtime.Cache**               | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Cache/)               | The [Waher.Runtime.Cache](Runtime/Waher.Runtime.Cache) project provides tools for in-memory caching. |
| **Waher.Runtime.Collections**         | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Collections/)         | The [Waher.Runtime.Collections](Runtime/Waher.Runtime.Collections) class library contains specialized collections for increased performance. |
| **Waher.Runtime.Console**             | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Console/)             | The [Waher.Runtime.Console](Runtime/Waher.Runtime.Console) class library helps with queueing and serialization of input and output to and from the Console in a multi-threaded environment, avoiding dead-locks when console is locked. |
| **Waher.Runtime.Counters**            | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Counters/)            | The [Waher.Runtime.Counters](Runtime/Waher.Runtime.Counters) project helps with counting events in runtime environments in a persistent and efficient and platform-independent manner. |
| **Waher.Runtime.Geo**                 | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Geo/)                 | The [Waher.Runtime.Geo](Runtime/Waher.Runtime.Geo) project helps with working with geo-spatial information in real-time. |
| **Waher.Runtime.Inventory**           | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Inventory/)           | The [Waher.Runtime.Inventory](Runtime/Waher.Runtime.Inventory) project keeps an inventory of types and interfaces available in your code. It also provides a means to access available types given an interface, and can find the best implementation to process a task or item. It can be used to implement an Inversion of Control Pattern, and helps instantiate interfaces, abstract classes and normal classes, including recursively instantiating constructor arguments. Handles singleton types. |
| **Waher.Runtime.Inventory.Loader**    | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Inventory.Loader/)    | The [Waher.Runtime.Inventory.Loader](Runtime/Waher.Runtime.Inventory.Loader) project dynamically loads modules from a folder, and initiates the inventory defined in [Waher.Runtime.Inventory](Runtime/Waher.Runtime.Inventory) with all loaded and referenced assemblies. |
| **Waher.Runtime.IO**                  | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.IO/)                  | The [Waher.Runtime.IO](Runtime/Waher.Runtime.IO) project contains a small set of IO extensions and tools simplifying stream, file, string and certificate management. |
| **Waher.Runtime.Language**            | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Language/)            | The [Waher.Runtime.Language](Runtime/Waher.Runtime.Language) project helps applications with language localization. |
| **Waher.Runtime.Profiling**           | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Profiling/)           | The [Waher.Runtime.Profiling](Runtime/Waher.Runtime.Profiling) project contains tools for profiling sequences of actions in multiple threads, as well as benchmarking. Results are accumulated, and can be exported to XML or as PlantUML Diagrams. |
| **Waher.Runtime.Queue**               | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Queue/)               | The [Waher.Runtime.Queue](Runtime/Waher.Runtime.Queue) project contains a specialised FIFO Queue for asynchronous transport of items between tasks. You can have multiple working tasks adding items to the queue, as well as multiple working tasks subscribing to items from the queue. |
| **Waher.Runtime.ServiceRegistration** | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.ServiceRegistration/) | The [Waher.Runtime.ServiceRegistration](Runtime/Waher.Runtime.ServiceRegistration) library allows applications to register themselves with an XMPP-based Service Registry, such as the [IoT Broker](https://waher.se/Broker.md). |
| **Waher.Runtime.Settings**            | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Settings/)            | The [Waher.Runtime.Settings](Runtime/Waher.Runtime.Settings) project helps applications maintain a set of persistent settings. Server-side runtime settings, as well as host-specific and user-specific settings are supported. |
| **Waher.Runtime.Text**                | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Text/)                | The [Waher.Runtime.Text](Runtime/Waher.Runtime.Text) project provides classes working with text and text documents, particularly find differences between texts, or sequences of symbols, or mapping strings to a harmonized set of strings (or tokens). |
| **Waher.Runtime.Temporary**           | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Temporary/)           | The [Waher.Runtime.Temporary](Runtime/Waher.Runtime.Temporary) project contains classes simplifying working with temporary in-memory and file streams. |
| **Waher.Runtime.Threading**           | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Threading/)           | The [Waher.Runtime.Threading](Runtime/Waher.Runtime.Threading) project provides classes for usage in multi-threaded asynchronous environments providing multiple-read/single-write capabilities. |
| **Waher.Runtime.Threading.Sync**      | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Threading.Sync/)      | The [Waher.Runtime.Threading.Sync](Runtime/Waher.Runtime.Threading.Sync) project provides classes for executing tasks that need to be synchronized from the same thread, asynchronously, in an asynchronous environment. Includes an asynchronous Mutex class. |
| **Waher.Runtime.Timing**              | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Timing/)              | The [Waher.Runtime.Timing](Runtime/Waher.Runtime.Timing) project provides tools for timing and scheduling. |
| **Waher.Runtime.Transactions**        | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Runtime.Transactions/)        | The [Waher.Runtime.Transactions](Runtime/Waher.Runtime.Transactions) project defines an architecture for processing transactions to help protect the integrity of data in asynchronous or distributed environments. |

The folder also contains the following unit test projects:

| Project                            | Type     | Project description |
|------------------------------------|----------|---------------------|
| **Waher.Runtime.Collections.Test** | .NET 8.0 | The [Waher.Runtime.Collections.Test](Runtime/Waher.Runtime.Collections.Test) project contains unit tests for the [Waher.Runtime.Collections](Runtime/Waher.Runtime.Collections) project. |
| **Waher.Runtime.Geo.Test**         | .NET 8.0 | The [Waher.Runtime.Geo.Test](Runtime/Waher.Runtime.Geo.Test) project contains unit tests for the [Waher.Runtime.Geo](Runtime/Waher.Runtime.Geo) project. |
| **Waher.Runtime.Inventory.Test**   | .NET 8.0 | The [Waher.Runtime.Inventory.Test](Runtime/Waher.Runtime.Inventory.Test) project contains unit tests for the [Waher.Runtime.Inventory](Runtime/Waher.Runtime.Inventory) project. |
| **Waher.Runtime.Language.Test**    | .NET 8.0 | The [Waher.Runtime.Language.Test](Runtime/Waher.Runtime.Language.Test) project contains unit tests for the [Waher.Runtime.Language](Runtime/Waher.Runtime.Language) project. |
| **Waher.Runtime.Profiling.Test**   | .NET 8.0 | The [Waher.Runtime.Profiling.Test](Runtime/Waher.Runtime.Profiling.Test) project contains unit tests for the [Waher.Runtime.Profiling](Runtime/Waher.Runtime.Profiling) project. |
| **Waher.Runtime.Settings.Test**    | .NET 8.0 | The [Waher.Runtime.Settings.Test](Runtime/Waher.Runtime.Settings.Test) project contains unit tests for the [Waher.Runtime.Settings](Runtime/Waher.Runtime.Settings) project. |
| **Waher.Runtime.Text.Test**        | .NET 8.0 | The [Waher.Runtime.Text.Test](Runtime/Waher.Runtime.Text.Test) project contains unit tests for the [Waher.Runtime.Text](Runtime/Waher.Runtime.Text) project. |
| **Waher.Runtime.Threading.Test**   | .NET 8.0 | The [Waher.Runtime.Threading.Test](Runtime/Waher.Runtime.Threading.Test) project contains unit tests for the [Waher.Runtime.Threading](Runtime/Waher.Runtime.Threading) project. |

Script
----------------------

The [Script](Script) folder contains libraries that define an extensible execution envionment for script supporting canonical extensions, .NET integration, 
graphs, physical units and unit conversions, etc. For more information about the script engine supported by these libraries, see the 
[script reference](https://waher.se/Script.md).


| Project                          | Type         | Link                                                                  | Project description |
|----------------------------------|--------------|-----------------------------------------------------------------------|---------------------|
| **Waher.Script**                 | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script/)                 | The [Waher.Script](Script/Waher.Script) project is a class library that provides basic abstraction and execution model for symbolic math and scripting. It also manages pluggable modules and easy dynamic access to runtime namespaces and types. [Reference](https://waher.se/Script.md) |
| **Waher.Script.Content**         | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Content/)         | The [Waher.Script.Content](Script/Waher.Script.Content) project is a class library that adds content functions to the script engine, suitable for loading, fetching or processing content from files or online resources. |
| **Waher.Script.Cryptography**    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Cryptography/)    | The [Waher.Script.Cryptography](Script/Waher.Script.Cryptography) project is a class library that adds cryptography functions to the script engine. |
| **Waher.Script.Data**            | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Data/)            | The [Waher.Script.Data](Script/Waher.Script.Data) project is a class library that extends the script engine with functions for accessing external MS SQL, OleDB or OBDC databases. |
| **Waher.Script.Data.MySQL**      | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Data.MySQL/)      | The [Waher.Script.Data.MySQL](Script/Waher.Script.Data.MySQL) project is a class library that extends the script engine with functions for accessing external MySQL databases. |
| **Waher.Script.Data.PostgreSQL** | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Data.PostgreSQL/) | The [Waher.Script.Data.PostgreSQL](Script/Waher.Script.Data.PostgreSQL) project is a class library that extends the script engine with functions for accessing external PostgreSQL databases. |
| **Waher.Script.Fractals**        | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Fractals/)        | The [Waher.Script.Fractals](Script/Waher.Script.Fractals) project is a class library that adds fractal image functions to the script engine, suitable for generating backgound images. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.FullTextSearch**  | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.FullTextSearch/)  | The [Waher.Script.FullTextSearch](Script/Waher.Script.FullTextSearch) project is a class library that adds full-text-search functions to the script engine. Full-text-search is provided by the [Waher.Persistence.FullTextSearch](Persistence/Waher.Persistence.FullTextSearch) library. |
| **Waher.Script.Graphs**          | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Graphs/)          | The [Waher.Script.Graphs](Script/Waher.Script.Graphs) project is a class library that adds graphing functions to the script engine. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.Graphs3D**        | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Graphs3D/)        | The [Waher.Script.Graphs3D](Script/Waher.Script.Graphs3D) project is a class library that adds 3D-graphing functions to the script engine. It uses [SkiaSharp](https://www.nuget.org/packages/SkiaSharp) for cross-platform 2D graphics manipulation. |
| **Waher.Script.Networking**      | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Script.Networking/)      | The [Waher.Script.Networking](Script/Waher.Script.Networking) project is a class library that extends the script engine with functions for different network protocols. |
| **Waher.Script.Persistence**     | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Persistence/)     | The [Waher.Script.Persistence](Script/Waher.Script.Persistence) project is a class library that contains script extensions for the persistence layer (defined in [Waher.Persistence](Persistence/Waher.Persistence)). Allows for searching for, creating, updating and deleting objects in the object database from script. Includes support for SQL queries against the object database persistence layer, and SPARQL queries for semantic-web queries using graph notation, linked data and graph databases. |
| **Waher.Script.Statisics**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Statistics/)      | The [Waher.Script.Statisics](Script/Waher.Script.Statisics) project is a class library that adds statistical functions to the script engine. |
| **Waher.Script.System**          | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.System/)          | The [Waher.Script.System](Script/Waher.Script.System) project is a class library that extends the script engine with functions for accessing the system, including executing processes. |
| **Waher.Script.Threading**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Threading/)       | The [Waher.Script.Threading](Script/Waher.Script.Threading) project is a class library that adds functions to the script engine for executing script in a threaded environmet. This includes parallel execution, as well as protected serialized execution. |
| **Waher.Script.Xml**             | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.Xml/)             | The [Waher.Script.Xml](Script/Waher.Script.Xml) project is a class library that contains script extensions for parsing XML. |
| **Waher.Script.XmlDSig**         | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Script.XmlDSig/)         | The [Waher.Script.XmlDSig](Script/Waher.Script.XmlDSig) project is a class library that contains script extensions for signing and verifying XML documents using the [XMLDSIG](https://www.w3.org/TR/xmldsig-core/) standard. |

The folder also contains the following unit test projects:

| Project                            | Type     | Project description |
|------------------------------------|----------|---------------------|
| **Waher.Script.Test**              | .NET 8.0 | The [Waher.Script.Test](Script/Waher.Script.Test) project contains unit tests for the script-related projects in this section. |

Security
----------------------

The [Security](Security) folder contains libraries that are dedicated at solving particular security or data protection such as authentication, 
authorization and encryption.

| Project                             | Type         | Link                                                                     | Project description |
|-------------------------------------|--------------|--------------------------------------------------------------------------|---------------------|
| **Waher.Security**                  | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security/)                  | The [Waher.Security](Security/Waher.Security) project provides a basic security model based on users, roles and privileges. It's not based on operating system features, to allow code to be platform independent. |
| **Waher.Security.ACME**             | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.ACME/)             | The [Waher.Security.ACME](Security/Waher.Security.ACME) project contains a class library implementing the ACME v2 protocol for the generation of certificates using ACME-compliant certificate servers, as defined in the [ACME draft](https://tools.ietf.org/html/draft-ietf-acme-acme-13). |
| **Waher.Security.CallStack**        | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.CallStack/)        | The [Waher.Security.CallStack](Security/Waher.Security.CallStack) project provide tools for securing access to methods and properties in code, by limiting access to them to a given set of callers. This prevents unintentional leaks of information through code running in the same process. |
| **Waher.Security.ChaChaPoly**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.ChaChaPoly/)       | The [Waher.Security.ChaChaPoly](Security/Waher.Security.ChaChaPoly) project implements the ChaCha20, Poly1305 and AEAD_CHACHA20_POLY1305 algorithms, as defined in [RFC 8439](https://tools.ietf.org/html/rfc8439). |
| **Waher.Security.DTLS**             | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Security.DTLS/)             | The [Waher.Security.DTLS](Security/Waher.Security.DTLS) project contains a class library implementing the Datagram Transport Layer Security (DTLS) Version 1.2, as defined in [RFC 6347](https://tools.ietf.org/html/rfc6347). |
| **Waher.Security.DTLS.UWP**         | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Security.DTLS.UWP/)         | The [Waher.Security.DTLS.UWP](Security/Waher.Security.DTLS.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Security.DTLS** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Security.EllipticCurves**   | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.EllipticCurves/)   | The [Waher.Security.EllipticCurves](Security/Waher.Security.EllipticCurves) project contains a class library implementing algorithms for Elliptic Curve Cryptography, such as ECDH, ECDSA, EdDSA, NIST P-192, NIST P-224, NIST P-256, NIST P-384, NIST P-521, Curve25519, Curve448, Edwards25519 and Edwards448. |
| **Waher.Security.JWS**              | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.JWS/)              | The [Waher.Security.JWS](Security/Waher.Security.JWS) project implements a framework for JSON Web Signatures (JWS), as defined in [RFC 7515](https://tools.ietf.org/html/rfc7515). |
| **Waher.Security.JWT**              | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Security.JWT/)              | The [Waher.Security.JWT](Security/Waher.Security.JWT) project helps applications with the creation and validation of Java Web Tokens (JWT), as defined in [RFC 7519](https://tools.ietf.org/html/rfc7519). |
| **Waher.Security.JWT.UWP**          | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Security.JWT.UWP/)          | The [Waher.Security.JWT.UWP](Security/Waher.Security.JWT.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Security.JWT** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Security.LoginMonitor**     | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Security.LoginMonitor/)     | The [Waher.Security.LoginMonitor](Security/Waher.Security.LoginMonitor) helps applications monitor login activity, and help block malicious entities from the system. |
| **Waher.Security.LoginMonitor.UWP** | UWP          | [NuGet](https://www.nuget.org/packages/Waher.Security.LoginMonitor.UWP/) | The [Waher.Security.LoginMonitor.UWP](Security/Waher.Security.LoginMonitor.UWP) project provides a Universal Windows Platform compatible version of the **Waher.Security.LoginMonitor** Library. This library can be used to develop applications for Windows 10, on for instance Rasperry Pi. |
| **Waher.Security.PKCS**             | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.PKCS/)             | The [Waher.Security.PKCS](Security/Waher.Security.PKCS) project contains classes and tools for working with Public Key Cryptography Standards (PKCS). |
| **Waher.Security.SHA3**             | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Security.SHA3/)             | The [Waher.Security.SHA3](Security/Waher.Security.SHA3) project implements SHA-3, as defined in [NIST FIPS 202](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.202.pdf), including the general KECCAK algorithm and the SHAKE128, SHAKE256, RawSHAKE128 and RawSHAKE256 XOF functions. |
| **Waher.Security.SPF**              | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Security.SPF/)              | The [Waher.Security.SPF](Security/Waher.Security.SPF) project contains a class library for resolving Sender Policy Framework (SPF) strings as defined in [RFC 7208](https://tools.ietf.org/html/rfc7208). |
| **Waher.Security.Users**            | .NET Std 2.1 | [NuGet](https://www.nuget.org/packages/Waher.Security.Users/)            | The [Waher.Security.Users](Security/Waher.Security.Users) project defines an architecture of persistent users, roles and privileges that can be used to provide detailed authorization in applications. Privileges are ordered in a tree structure. Roles contains a list of allowed privileges (nodes or entire branches), or explicitly prohibited privileges (nodes or branches). Each user can be assigned one or more roles. Credentials are protected using hash digests. Objects are persisted through the object database abstraction layer, defined in Waher.Persistence. |

The folder also contains the following unit test projects:

| Project                                | Type     | Project description |
|----------------------------------------|----------|---------------------|
| **Waher.Security.ACME.Test**           | .NET 8.0 | The [Waher.Security.ACME.Test](Security/Waher.Security.ACME.Test) project contains unit tests for the  [Waher.Security.ACME](Security/Waher.Security.ACME) project.                                         |
| **Waher.Security.ChaChaPoly.Test**     | .NET 8.0 | The [Waher.Security.ChaChaPoly.Test](Security/Waher.Security.ChaChaPoly.Test) project contains unit tests for the  [Waher.Security.ChaChaPoly](Security/Waher.Security.ChaChaPoly) project. |
| **Waher.Security.DTLS.Test**           | .NET 8.0 | The [Waher.Security.DTLS.Test](Security/Waher.Security.DTLS.Test) project contains unit tests for the  [Waher.Security.DTLS](Security/Waher.Security.DTLS) project.                                         |
| **Waher.Security.EllipticCurves.Test** | .NET 8.0 | The [Waher.Security.EllipticCurves.Test](Security/Waher.Security.EllipticCurves.Test) project contains unit tests for the  [Waher.Security.EllipticCurves](Security/Waher.Security.EllipticCurves) project. |
| **Waher.Security.JWT.Test**            | .NET 8.0 | The [Waher.Security.JWT.Test](Security/Waher.Security.JWT.Test) project contains unit tests for the  [Waher.Security.JWT](Security/Waher.Security.JWT) project.                                             |
| **Waher.Security.LoginMonitor.Test**   | .NET 8.0 | The [Waher.Security.LoginMonitor.Test](Security/Waher.Security.LoginMonitor.Test) project contains unit tests for the  [Waher.Security.LoginMonitor](Security/Waher.Security.LoginMonitor) project. |
| **Waher.Security.PKCS.Test**           | .NET 8.0 | The [Waher.Security.PKCS.Test](Security/Waher.Security.PKCS.Test) project contains unit tests for the  [Waher.Security.PKCS](Security/Waher.Security.PKCS) project.                                         |
| **Waher.Security.SHA3.Test**           | .NET 8.0 | The [Waher.Security.SHA3.Test](Security/Waher.Security.SHA3.Test) project contains unit tests for the  [Waher.Security.SHA3](Security/Waher.Security.SHA3) project. |
| **Waher.Security.SPF.Test**            | .NET 8.0 | The [Waher.Security.SPF.Test](Security/Waher.Security.SPF.Test) project contains unit tests for the  [Waher.Security.SPF](Security/Waher.Security.SPF) project. |

Services
----------------------

The [Services](Services) folder contains standalone service applications.

| Project                    | Type       | Link                                                          | Project description |
|----------------------------|------------|---------------------------------------------------------------|---------------------|
| **Waher.Service.PcSensor** | .NET 8.0   |                                                               | The [Waher.Service.PcSensor](Services/Waher.Service.PcSensor) project defines an application that converts your PC into an IoT sensor, by publishing performace counters as sensor values. [Full Screen Shot 1.](Images/Waher.Service.PcSensor.1.png) [Full Screen Shot 2.](Images/Waher.Service.PcSensor.2.png) [Full Screen Shot 3.](Images/Waher.Service.PcSensor.3.png) |

The following project has been discontinued:

| Project                    | Type       | Link                                                          | Project description |
|----------------------------|------------|---------------------------------------------------------------|---------------------|
| **Waher.Service.GPIO**     | UWP        |                                                               | The [Waher.Service.GPIO](Services/Waher.Service.GPIO) project defines a Universal Windows Platform application that can be installed on Windows 10 IoT devices. It will publish available GPIO inputs/outputs over XMPP sensor, control and chat interfaces. It will also publish Digital and Analog Arduino interfaces, if an Arduino using the Firmata protocol is connected to an USB port of the device. The application can be used to elaborate with GPIO peripherals using a simple chat client. |

Themes
----------------------

The [Themes](Themes) folder contains libraries that contain content files for different visual themes.

| Project                       | Type         | Link                                                               | Project description |
|-------------------------------|--------------|--------------------------------------------------------------------|---------------------|
| **Waher.Theme.CactusRose**    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.CactusRose/)    | The [Waher.Theme.CactusRose](Themes/Waher.Theme.CactusRose) project contains content files for the Cactus Rose theme.          |
| **Waher.Theme.GothicPeacock** | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.GothicPeacock/) | The [Waher.Theme.GothicPeacock](Themes/Waher.Theme.GothicPeacock) project contains content files for the Gothic Peacock theme. |
| **Waher.Theme.Retro64**       | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.Retro64/)       | The [Waher.Theme.Retro64](Themes/Waher.Theme.Retro64) project contains content files for the Retro-64 theme.                   |
| **Waher.Theme.SpaceGravel**   | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.SpaceGravel/)   | The [Waher.Theme.SpaceGravel](Themes/Waher.Theme.SpaceGravel) project contains content files for the Space Gravel theme.       |
| **Waher.Theme.WinterDawn**    | .NET Std 2.0 | [NuGet](https://www.nuget.org/packages/Waher.Theme.WinterDawn/)    | The [Waher.Theme.WinterDawn](Themes/Waher.Theme.WinterDawn) project contains content files for the Winter Dawn theme.          |

Things
----------------------

The [Things](Things) folder contains libraries that define a hardware and data abstraction layer for interacting with things. This includes describing 
sensor data, control parameters, attributes, displayable parameters, commands, queries and data sources. It also includes embedding things dynamically,
to form more complex devices, such as concentrators or bridges.

| Project                   | Type          | Link                                                           | Project description |
|---------------------------|---------------|----------------------------------------------------------------|---------------------|
| **Waher.Things**          | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Things/)          | The [Waher.Things](Things/Waher.Things) project is a class library that provides basic abstraction of things, errors, sensor data and control operations. |
| **Waher.Things.Ieee1451** | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Ieee1451/) | The [Waher.Things.Ieee1451](Things/Waher.Things.Ieee1451) project is a class library that publishes nodes that communicate using the IEEE 1451 family of standards. |
| **Waher.Things.Files**    | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Files/)    | The [Waher.Things.Files](Things/Waher.Things.Files) project publishes nodes that permit you to define sensor and actuator nodes in the network based on files in the file-system. |
| **Waher.Things.Ip**       | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Ip/)       | The [Waher.Things.Ip](Things/Waher.Things.Ip) project is a class library that publishes nodes representing nodes on an IP network. |
| **Waher.Things.Metering** | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Metering/) | The [Waher.Things.Metering](Things/Waher.Things.Metering) project is a class library that defines a basic metering infrastructure. |
| **Waher.Things.Modbus**   | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Modbus/)   | The [Waher.Things.Modbus](Things/Waher.Things.Modbus) project is a class library that publishes nodes representing Modbus devices. |
| **Waher.Things.Mqtt**     | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Mqtt/)     | The [Waher.Things.Mqtt](Things/Waher.Things.Mqtt) project is a class library that publishes nodes representing devices connected to MQTT brokers. |
| **Waher.Things.Script**   | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Script/)   | The [Waher.Things.Script](Things/Waher.Things.Script) project is a class library that publishes nodes that permit you to define sensors and actuators using script. |
| **Waher.Things.Semantic** | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Semantic/) | The [Waher.Things.Semantic](Things/Waher.Things.Semantic) project is a class library that publishes nodes representing Semantic Web devices. It also makes other non-semantic metering devices available via semantic web interfaces, such as SPARQL queries. |
| **Waher.Things.Snmp**     | .NET Std 2.0  | [NuGet](https://www.nuget.org/packages/Waher.Things.Snmp/)     | The [Waher.Things.Snmp](Things/Waher.Things.Snmp) project is a class library that publishes nodes representing SNMP devices on the local area network. |
| **Waher.Things.Virtual**  | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Virtual/)  | The [Waher.Things.Virtual](Things/Waher.Things.Virtual) project is a class library that publishes virtual nodes that can act as placeholders for software that wishes to publish nodes on the network. |
| **Waher.Things.Xmpp**     | .NET Std 2.1  | [NuGet](https://www.nuget.org/packages/Waher.Things.Xmpp/)     | The [Waher.Things.Xmpp](Things/Waher.Things.Xmpp) project is a class library that publishes nodes for communication with devices over XMPP. |

The folder also contains the following unit test projects:

| Project                   | Type     | Project description |
|---------------------------|----------|---------------------|
| **Waher.Things.Test**     | .NET 8.0 | The [Waher.Things.Test](Things/Waher.Things.Test) project contains unit tests related to the thing libraries. |

The following projects have been discontinued:

| Project                   | Type     | Project description |
|---------------------------|----------|---------------------|
| **Waher.Things.Arduino**  | UWP      | [NuGet](https://www.nuget.org/packages/Waher.Things.Arduino/)  | The [Waher.Things.Arduino](Things/Waher.Things.Arduino) project is a class library that publishes nodes for interaction with Arduinos and connected modules via Firmata. |
| **Waher.Things.Gpio**     | UWP      | [NuGet](https://www.nuget.org/packages/Waher.Things.Gpio/)     | The [Waher.Things.Gpio](Things/Waher.Things.Gpio) project is a class library that publishes nodes for interaction with onboard General Purpose Input/Output (GPIO) modules. |


Utilities
----------------------

The [Utilities](Utilities) folder contains applications that help the developer or administrator with different tasks.

| Project                            | Type       | Link  | Project description |
|------------------------------------|------------|-------|---------------------|
| **Waher.Utility.Acme**             | .NET 8.0   |       | The [Waher.Utility.Acme](Utilities/Waher.Utility.Acme) is a command-line tool that helps you create certificates using the Automatic Certificate Management Environment (ACME) v2 protocol. |
| **Waher.Utility.AnalyzeClock**     | .NET 8.0   |       | The [Waher.Utility.AnalyzeClock](Utilities/Waher.Utility.AnalyzeClock) is a command-line tool that helps you analyze the difference in clocks between machines compatible with the [Neuro-Foundation](https://neuro-foundation.io). |
| **Waher.Utility.AnalyzeDB**        | .NET 8.0   |       | The [Waher.Utility.AnalyzeDB](Utilities/Waher.Utility.AnalyzeDB) is a command-line tool that helps you analyze an object database created by the [Waher.Persistence.Files](Persistence/Waher.Persistence.Files) or [Waher.Persistence.FilesLW](Persistence/Waher.Persistence.FilesLW) libraries, such as the IoT Gateway database. |
| **Waher.Utility.Asn1ToCSharp**     | .NET 8.0   |       | The [Waher.Utility.Asn1ToCSharp](Utilities/Waher.Utility.Asn1ToCSharp) is a command-line tool that creates C# files from definitions made in ASN.1 files. |
| **Waher.Utility.Csp**              | .NET 8.0   |       | The [Waher.Utility.Csp](Utilities/Waher.Utility.Csp) is a command-line tool that helps you perform operations on keys managed by the system Cryptographic Service Provider CSP. |
| **Waher.Utility.DeleteDB**         | .NET 8.0   |       | The [Waher.Utility.DeleteDB](Utilities/Waher.Utility.DeleteDB) is a command-line tool that helps you delete an object database created by the Waher.Persistence.Files or Waher.Persistence.FilesLW libraries, such as the IoT Gateway database, including any cryptographic keys stored in the CSP. |
| **Waher.Utility.ExStat**           | .NET 8.0   |       | The [Waher.Utility.ExStat](Utilities/Waher.Utility.ExStat) is a command-line tool that helps you extract statistical information about exceptions occurring in an IoT Gateway, with logging of exceptions activated. |
| **Waher.Utility.Extract**          | .NET 8.0   |       | The [Waher.Utility.Extract](Utilities/Waher.Utility.Extract) is a command-line tool that helps you extract information from a backup file generated by the IoT Gateway. |
| **Waher.Utility.GenManifest**      | .NET 8.0   |       | The [Waher.Utility.GenManifest](Utilities/Waher.Utility.GenManifest) project provides a command-line tool that can generate manifest files from files existing in folders. |
| **Waher.Utility.GetEmojiCatalog**  | .NET 8.0   |       | The [Waher.Utility.GetEmojiCatalog](Utilities/Waher.Utility.GetEmojiCatalog) project downloads an [emoji catalog](http://unicodey.com/emoji-data/table.htm) and extracts the information and generates code for handling emojis. |
| **Waher.Utility.Install**          | .NET 8.0   |       | The [Waher.Utility.Install](Utilities/Waher.Utility.Install) is a command-line tool that helps you install pluggable modules into the IoT Gateway. |
| **Waher.Utility.Markdown**         | .NET 8.0   |       | The [Waher.Utility.Markdown](Utilities/Waher.Utility.Markdown) is a command-line tool that helps you automate the conversion of Markdown documents to different formats. |
| **Waher.Utility.RegEx**            | .NET 8.0   |       | The [Waher.Utility.RegEx](Utilities/Waher.Utility.RegEx) is a command-line tool that helps you find content in files using regular expressions, and optionally either export the findings or replace them with something else. |
| **Waher.Utility.RunScript**        | .NET 8.0   |       | The [Waher.Utility.RunScript](Utilities/Waher.Utility.RunScript) is a command-line tool that allows you to execute script. |
| **Waher.Utility.Sign**             | .NET 8.0   |       | The [Waher.Utility.Sign](Utilities/Waher.Utility.Sign) is a command-line tool that helps you sign files using asymmetric keys. |
| **Waher.Utility.TextDiff**         | .NET 8.0   |       | The [Waher.Utility.TextDiff](Utilities/Waher.Utility.TextDiff) is a command-line tool that compares two text files and outputs the differences between them. |
| **Waher.Utility.Transform**        | .NET 8.0   |       | The [Waher.Utility.Transform](Utilities/Waher.Utility.Transform) is a command-line tool that transforms an XML file utilizing an XSL Transform (XSLT). |
| **Waher.Utility.Translate**        | .NET 8.0   |       | The [Waher.Utility.Translate](Utilities/Waher.Utility.Translate) is a command-line tool that helps translating resource strings from one language to another. It uses an internal database to check for updates, and performs translations only of new or updated strings accordingly. |

Web Services
----------------------

The [WebServices](WebServices) folder contains modules that add web service capabilities to projects they are used in.

| Project                        | Type         | Link  | Project description |
|--------------------------------|--------------|-------|---------------------|
| **Waher.WebService.Script**    | .NET Std 2.1 |       | The [Waher.WebService.Script](WebServices/Waher.WebService.Script) project provides a web service that can be used to execute script on the server, from the client. |
| **Waher.WebService.Sparql**    | .NET Std 2.1 |       | The [Waher.WebService.Sparql](WebServices/Waher.WebService.Sparql) project provides a SPARQL endpoint that can be used to execute federated queries on the server and across the network. |
| **Waher.WebService.Tesseract** | .NET Std 2.1 |       | The [Waher.WebService.Tesseract](WebServices/Waher.WebService.Tesseract) project provides a web service that can act as a gateway to [Tesseract](https://tesseract-ocr.github.io/tessdoc/Downloads.html), installed on the server. |

The folder also contains the following unit test projects:

| Project                             | Type     | Project description |
|-------------------------------------|----------|---------------------|
| **Waher.Webservice.Tesseract.Test** | .NET 8.0 | The [Waher.Security.SPF.Test](Security/Waher.Security.SPF.Test) project contains unit tests for the  [Waher.Security.SPF](Security/Waher.Security.SPF) project. |

Environment Variables
------------------------

During intial configuration and setup of the **IoT Gateway**, and any hosted services, a sequence of configuration steps have to be processed.
The operator of the Gateway, and hosted services, can choose to enter the information manually in each step via the web interface, or provide
the information via *environment variables*, allowing for the automation of the configuration and setup procedure.

### Web setup

The default method of configuring the **IoT Gateway** is via the web interface. The first time the gateway is run, the web interface is overridden
to display the configuration steps required to be completed. The user can choose a *simplified* or a *detailed* configuration. The *detailed*
configuration shows each step in the configuration process, while the *simplified* only shows required steps, while the others receive a *default*
configuration. As soon as all steps have been completed, the setup override is removed, and the gateway starts operating in a configured state.

### Automated setup

The setup procedure can be automated, by providing the values required for the setup in *Environment Variables*. This permits the operator
to automate the configuration, and to create containers with preprogrammed configurations. Available Environment Variables depends on the
amount of services hosted by the gateway. This repository contains a list of Environment Variables that can be used to automate the setup,
in the [Configuration Steps](ConfigurationSteps.md) article.

### Mixed setup

An operator can choose to partially pre-configure the gateway using only a subset of available *Environment Variables* necessary for a complete
configuration. During initial start of the gateway, the configuraiton steps with these preconfigured values will be skipped, as they will be seen
as completed. Only the configuration steps without preconfigured values will be shown.

Compiling solution
---------------------

You need to setup the build environment properly before you can compile the projects in this repository. The following subsections describe the configurations that need to be made:

### Instances

The IoT Gateway can be executed in multiple instances. The default instance uses the empty *instance name*. Each other instance, has a given instance name that is assigned to the executable via a command-line argument.  When building the IoT Gateway, it is assumed the instance name is `Dev`. This allows you to have a default installed version of the Gateway (or multiple installed instances), while your development version is running as a separate instance named `Dev` in parallel.

### Project Folders

Build events and script available in solutions assume the projects reside certain folders. On a Windows machine, the IoT Gateway repository is assumed to be cloned into the `C:\My Projects\IoTGateway` folder. On a MAC, that folder is assumed to be `/My Projects/IoT Gateway`.

#### Virtual root folders on MAC computers

To create a root folder on a MAC you need to create a virtual folder that appears in the root folder, due to restrictions in the operating system. To create the `/My Projects` folder, follow these steps:

1. Open the Terminal app.
2. Type `sudo nano /etc/synthetic.conf` and press Enter. You may need to enter your administrator password.
3. In the nano text editor, enter:
	
		My Projects	/Users/yourusername/My Projects
	
	There should be a space between `My` and `Projects`, but a TAB character between `My Projects` and `/Users...`. Also, replace the `yourusername` with the name of your user name.

4. Press Control + O to write the file, then Enter to confirm, and Control + X to exit nano.
5. Reboot your Mac.

After rebooting, you should see the virtual folder `/My Projects` in the root directory. 

### Windows

To compile projects on Windows, use Visual Studio 2022 (or later). The IoT Gateway separates executable and compiled code (which is later installed in any of the computer's Program Files folder(s)), and application data, which the application can read and write. The latter is stored in the corresponding `ProgramData` folder. The default installation of IoT Gateway, stores data in `C:\ProgramData\IoT Gateway`. A non-default instance, stores its application data in `C:\ProgramData\IoT Gateway INST`, where `INST` is replaced by the instance name. So before you can compile, you need to make sure the folder `C:\ProgramData\IoT Gateway Dev` exists, and that the user performing the build has access to this folder.

### MAC

The following sections list the preparations that need to be performed before you can compile and run the IoT Gateway on a MAC:

#### Application data folder

To compile projects on MAC, use VS Code for MAC. The IoT Gateway separates executable and compiled code (which is later installed in a computer's `/Applications` folder), and application data, which the application can read and write. The latter is stored in the corresponding `/usr/share` folder. The default installation of IoT Gateway, stores data in `/usr/share/IoT Gateway`. A non-default instance, stores its application data in `/usr/share/IoT Gateway INST`, where `INST` is replaced by the instance name. So before you can compile, you need to make sure the folder `/usr/share/IoT Gateway Dev` exists, and that the user performing the build has access to this folder.

**Note**: You might not be permitted to create a folder under `/usr/share` for security reasons, even if using `sudo`. In such cases, you can create a folder under `/usr/local/share` instead. This folder will be valid for development purposes. Once the gateway is placed in an installation package, the package tool will be able to create the proper share folder for you.

Terminal script to create and configure folder:

	sudo mkdir -p "/usr/share/IoT Gateway Dev"
	sudo chown yourusername "/usr/share/IoT Gateway Dev"
	sudo chmod 755 "/usr/share/IoT Gateway Dev"

In case you are not permitted to execute the above script, you can create a development folder instead:

	sudo mkdir -p "/usr/local/share/IoT Gateway Dev"
	sudo chown yourusername "/usr/local/share/IoT Gateway Dev"
	sudo chmod 755 "/usr/local/share/IoT Gateway Dev"

#### Database enryption

You also need to create an environment variable named `FILES_DB_SALT`. Since the MAC computer does not provide access to cryptographic resources, database encryption requires a salt that needs to be unique for each machine. The value can be anything, but cannot change, as it would render existing information unreadable.

To set an environment variable in mac OS, you can follow this procedure:

1.	Open the Terminal app.
2.	Edit the shell configuration file, such as ~/.bash_profile for Bash or ~/.zshrc for Zsh (the default shell in newer macOS versions), using a text editor like nano or vim:

		nano ~/.bash_profile

	or

		nano ~/.zshrc

3.	Add the export command to the end of the file:

		export FILES_DB_SALT="value"

	You need to replace the `value` with a random string you keep unique for the machine.

4.	Save the file and exit the editor.
5.	To apply the changes immediately, source the file:

		source ~/.bash_profile

	or

		source ~/.zshrc

	(You can also reboot the computer.)

### Running IoT Gateway

You have to have the correct .NET Core framework installed on your machine to run the gateway: [Download .NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). 

When the gateway starts up, it reads the `gateway.config` file from the program data folder (see above) corresponding to the instance, if available. If not available, it reads the one provided in the build. This file contains information about what ports to open for different protocols, and any web folders to use. Review the file and its corresponding XML schema to get acquainted with options available. To modify this file, copy the file (if one does not exist) from the build output to the program data folder, and edit it there. Then restart the gateway.
