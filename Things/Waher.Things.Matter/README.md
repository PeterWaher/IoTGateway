Waher.Things.Matter
======================

The **Waher.Things.Matter** library integrates [Matter](https://csa-iot.org/all-solutions/matter/) devices into the
IoT Gateway architecture, as sensors and actuators in the metering topology. It contains a Matter *controller*
(commissioner and administrator), implemented in C# for .NET Standard 2.1 without third-party protocol or
cryptography libraries, and metering nodes publishing Matter devices and their endpoints as readable and
controllable nodes.

Metering nodes
-----------------

| Node                    | Base class                 | Description |
|-------------------------|----------------------------|-------------|
| `MatterControllerNode`  | `ProvisionedMeteringNode`  | A Matter controller with its own fabric. The fabric is created automatically. Private keys are stored as encrypted properties. The node is sniffable. |
| `MatterDeviceNode`      | `IpHost`                   | A commissioned Matter device (operational node). Reports device information, power source and diagnostics, and optionally the values of all its endpoints. |
| `MatterEndpointNode`    | `ProvisionedMeteringNode`  | An endpoint on a device (sensor and/or actuator). Created automatically when a device is commissioned or scanned. |

Typical use:

1. Add a *Matter Controller* node to the topology.
2. Execute the *Commission device...* command, and enter the pairing code (QR code payload `MT:...` or manual
   pairing code) of the device. Devices already in another ecosystem (Apple Home, Google Home, Amazon Alexa,
   SmartThings, etc.) can be shared with the gateway, using the *share* or *pairing mode* feature of that ecosystem.
3. A *Matter Device* node is created, with one *Matter Endpoint* node for each application endpoint on the device.
   Endpoints can be read (sensor data), controlled (control parameters) and receive commands. Changes are
   subscribed to, and reported as momentary values.

Other commands include *Find commissionable devices*, *Scan fabric...*, *Scan endpoints*, *Read all attributes*,
*Read events*, *Share with another ecosystem...* (opens a commissioning window), *Identify...*, cluster-specific
commands (for instance *Toggle*, *Lock*, *Open*), and *Remove device from fabric*.

Clusters
-----------

Clusters are mapped to fields, control parameters and commands by *cluster handlers*, implementing `IClusterHandler`.
Built-in handlers exist for On/Off, Level Control, Color Control, Temperature, Humidity, Pressure, Flow, Illuminance,
Occupancy, Boolean State, Concentration Measurement, Air Quality, Power Source, Electrical Power and Energy
Measurement, Thermostat, Fan Control, Window Covering, Door Lock, Switch (incl. events), Mode Select and Mode Base
derived clusters, Valve, Basic Information, Bridged Device Basic Information and diagnostics clusters. Other clusters
are handled by a generic handler, using metadata of the Matter data model (generated from the Matter SDK), presenting
scalar attributes as fields, writable attributes as control parameters, and commands without mandatory arguments as
commands. Handlers can be added or overridden by implementing `IClusterHandler` in another module.

Protocol stack
-----------------

| Namespace           | Contents |
|---------------------|----------|
| `Tlv`               | Matter TLV encoding and decoding. |
| `Crypto`            | P-256, ECDSA, ECDH, SPAKE2+, AES-CCM, HKDF, PBKDF2. |
| `Messaging`         | Message layer, sessions, exchanges, Message Reliability Protocol (MRP), UDP transport (IPv6 and IPv4). |
| `SecureChannel`     | PASE and CASE session establishment, including CASE session resumption. |
| `Certificates`      | Matter operational certificates (TLV and X.509), certificate authority functions. |
| `Attestation`       | Device attestation, certification declarations, and trusted PAA certificates (production and development). |
| `Fabric`            | Fabric management (root CA, NOC issuance, IPK, compressed fabric ID). |
| `Commissioning`     | On-network commissioning flow. |
| `InteractionModel`  | Read, write, invoke, subscribe, timed interactions, chunking, events. |
| `Discovery`         | mDNS / DNS-SD discovery of commissionable and operational nodes. |
| `Onboarding`        | QR code and manual pairing code payloads. |
| `Controller`        | High-level controller (`MatterController`) and node (`MatterNode`) API. |
| `DataModel`         | Cluster, attribute, command, event, data type and device type metadata. |

Limitations
--------------

* Commissioning is performed on the IP network. Devices must already be connected to the network (for instance by
  sharing them from another ecosystem, or because they use Ethernet). Commissioning over Bluetooth LE, and
  provisioning of Wi-Fi or Thread network credentials, are not supported.
* Group communication, OTA software updates and check-in registration of Long Idle Time (LIT) intermittently
  connected devices are not supported.

Unit tests
-------------

The unit tests in **Waher.Things.Matter.Test** run against Matter test devices based on the open-source
[matter.js](https://github.com/project-chip/matter.js) reference implementation, using the public test credentials
of the Matter SDK. Node.js (v20 or later) must be installed. The matter.js packages are installed using `npm` the
first time the tests are run. Tests requiring test devices are reported as inconclusive if Node.js is not available.
Optionally, the environment variable `MATTER_TEST_PAIRING_CODE` can be set to the pairing code of a physical device,
to run additional tests against it.
