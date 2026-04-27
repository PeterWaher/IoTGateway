Title: Sensor Data Receptor API Documentation
Description: This document provides an overview of the Sensor Data Receptor API, including its endpoints, request and response formats, and usage examples.
Author: Peter Waher
Date: 2026-04-26
Master: /Master.md

=============================================================================================

Sensor Data Receptor API
===========================

This endpoint allows clients to send sensor data to the receptor for processing and storage.

Authentication
-----------------

Access to this resource is protected and requires authentication. Authentication is performed
using any of the following methods, in order of descending preference:

#.	Mututal TLS (mTLS) if Mutual TLS is enabled on the web server.
#.	JSON Web Tokens (JWT) with a valid `Bearer` token included in the Authorization header 
of the request.
#.	SHA3-256 Digest authentication with a username and password.
#.	SHA2-256 Digest authentication with a username and password.
#.	MD5 Digest authentication with a username and password.
#.	Basic authentication with a username and password.

If encryption is enabled on the web server (HTTPS), encryption is required for all calls, with
a minimum of 128 bit security.

The authenticated user account on the receptor must have the `Admin.SensorData.Post[.NODEPATH]` 
privilege in order to be authorized to post sensor data. The `.NODEPATH` suffix is replaced by 
the full ID path inside the Metering Topology, omitting the path up to and including the local
web server node), of the node to which the sensor data is being posted, if posted directly to 
a specific node. If posted to the receptor directly, without specifying a node in the URL, the 
user account must have the `Admin.SensorData.Post` privilege, and specify the node in the 
payload of the request.

Sensor Data
--------------

The API accepts sensor data in XML format (`Content-Type: text/xml`), as defined by the
[Neuro-Foundation](https://neuro-foundation.io/SensorData.md). Sensor data can be posted
directly to a node (using HTTP `POST`), providing the node path in the URL. In this case, the 
root element of the XML payload must be a `<ts/>` element, with the correct namespace. The
nodes can also be created by the system automatically, if they do not exist.

The data can also be posted to the receptor directly, with the node ID specified in the XML 
payload. In this case, the root element of the XML payload must be a `<nd/>` element 
(containing one or more `<ts/>` elements), or a `<resp/>` element containing one or more 
`<nd/>` elements. The nodes must also exist in the system, as the creator cannot create the 
nodes since the path is missing.

Example payload posted directly to the node `Node1` at 
`https://receptor.example.com/ReportSensorData/ExternalSensors/Node1` (this requires a user to 
have the privilege `Admin.SensorData.Post.ExternalSensors.Node1`):

```xml
<ts v="2017-09-22T15:22:33Z" xmlns="urn:nfi:iot:sd:1.0">
  <q n="Temperature" v="12.3" u="°C" m="true" ar="true"/>
  <s n="SN" v="12345678" i="true" ar="true"/>
</ts>
```

Example payload posted to the sensor data receptor at 
`https://receptor.example.com/ReportSensorData` (this requires a user to have the
privilege `Admin.SensorData.Post`):

```xml
<nd id="Node1" xmlns="urn:nfi:iot:sd:1.0">
  <ts v="2017-09-22T15:22:33Z">
    <q n="Temperature" v="12.3" u="°C" m="true" ar="true"/>
    <s n="SN" v="12345678" i="true" ar="true"/>
  </ts>
</nd>
```

### Response codes

The following table lists the possible response codes that the Sensor Data Receptor API may 
return:

| Code | Phrase      | Description                                                                 |
|-----:|:------------|:----------------------------------------------------------------------------|
| 204  | No Content  | The sensor data was successfully processed and stored.                      |
| 400  | Bad Request | Invalid or badly formatted payload.                                         |
| 403  | Forbidden   | The user is not authorized to post sensor data (to the corresponding node). |
| 404  | Not Found   | The specified node does not exist (and could not be created).               |

Nodes
--------

The [Neuro-Foundation](https://neuro-foundation.io/) interfaces define a hierarchical 
structure of nodes, which can be used to organize and manage sensor data, among other things.
Each node can represent a physical device, a logical grouping of devices, or any other entity 
relevant to the application. Nodes can have child nodes, allowing for a flexible and scalable 
organization of the sensor data.

Nodes are organized into data sources. One such data source is the *Metering Topology*, which
holds the nodes representing the physical and logical structure of the sensors used for 
metering (or telemetry) purposes. This Sensor Data Receptor API allows for external devices
to use `HTTP POST` to send Sensor Data to nodes in the system, provided the user has access
rights to do so, and the node exists (or can be created) and is a node that accepts such input. 
The system operator must define the nodes and/or users and user privileges before the external 
party can post sensor data to it.
