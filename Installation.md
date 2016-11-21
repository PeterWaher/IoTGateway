# Installation

Here the incremental achievements to making use of this repo:

1. Load Raspian Jessie on 3 SD cards.

0. Load the "Waher IoT Gateway install script" on the SD card.

   ### Initial device:

0. On Pi \#1 run the install script to install from the web
   <a href="#Mono-Install">Mono .NET 4.6</a>,
   <a href="#MongoDB-Install">Mongo DB</a>, 
   and IoT Gateway.
  
0. Manually configure Gateway from web client (from any internet browser).

   ### Initial device:

0. Create a device on Pi \#2 using Raspbian Jessie and .NET Core

0. On Pi \#2 (the Device) get __ program to recognize temperature data from sensor.

0. Configure Gateway to display statistics over time (to any internet browser).

0. Use Mock Loaded on Win10 using WPF to communicate with device.

   ### Additional device:

0. Create a device on Pi \#3 using Raspbian Jessie and .NET 4.6 UWP.

0. On Pi \#3 (the Device) get __ program to recognize temperature data from sensor.

   ### Developers:

0. Create C# dev platform from GitHub: install on Win10 VS 2015, .NET 4.6, 

0. Install Git client for Windows and get Waher IoT Gateway source.


<hr />

1. Load Raspian Jessie on 3 SD cards.

   Reference: 
   http://raspberrypihq.com/how-to-install-windows-10-iot-on-the-raspberry-pi/
   describes use of FFU2IMG


2. Load the "Waher IoT Gateway install script" on the SD card,


The script does the rest:

   <a name="Mono-Install"></a>

   ## Mono Install

   Reference:
   https://blogs.msdn.microsoft.com/brunoterkaly/2014/06/11/running-net-applications-on-a-raspberry-pi-that-communicates-with-azure/

0. Install trusted root certificates on to the raspberry pi

0. Install <strong>Download Mono 64-bit</strong> (Preview, no GTK#):

   http://www.mono-project.com/download/#download-win

   | File                   | Size  |
   | ---------------------- | ----: |
   | mono-4.6.2.7-x64-0.msi | 109 MB |

   ## Load Gateway

0. Install a Git CLI client -
   
   <tt><strong>
   sudo apt-get install git
   </strong></tt>
   
0. Use Git to clone:

   <tt><strong>
   git clone --depth=1 https://github.com/PeterWaher/IoTGateway/blob/master/
   </strong></tt>

0. Run setup:

   <tt><strong>
   sudo mono IoTGatewaySetup.exe
   </strong></tt>

   The IoTGatewaySetup.exe in
   <a target="_blank" href="https://github.com/PeterWaher/IoTGateway/blob/master/Executables/">
   GitHub</a> is for regular Windows.


   <a name="MongoDB-Install"></a>

   ## MongoDB Install

0. The shell script to install MongoDB on the Raspbian:

   <tt><strong>
   sudo apt-get update
   sudo apt-get upgrade
   sudo apt-get install mongodb-server
   </strong></tt>

   Binaries are stored in folder <strong>/usr/bin/</strong>.<br />
   Datas is stored in folder <strong>/var/lib/mongodb/</strong>. 

0. Configure the MongoDB service to start when the Raspberry Pi boots up:

   <tt><strong>
   sudo service mongod start
   </strong></tt>

   The MongoDB shell would be invoked remotely only as needed for debugging:

   <tt><strong>
   mongo
   </strong></tt>


   ## Invoke server

   <tt><strong>
   sudo mono IoTGateway.exe 
   </strong></tt>


References:
   http://github.com/sedouard/SmartDoorDemo
SmartDoor Client on Github – Mono-based C# Raspberry PI Client:
