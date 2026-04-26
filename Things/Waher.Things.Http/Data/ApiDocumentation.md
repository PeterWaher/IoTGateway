Title: Sensor Data Receptor API Documentation
Description: This document provides an overview of the Sensor Data Receptor API, including its endpoints, request and response formats, and usage examples.
Author: Peter Waher
Date: 2026-04-26

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

The authenticated user account on the receptor must have the `Admin.SensorData.Post[.NODE]` 
privilege in order to be authorized to post sensor data. The `.NODE` suffix is replaced by the
ID of the node to which the sensor data is being posted, if posted directly to a specific node.
If posted to the receptor directly, without specifying a node in the URL, the user account must 
have the `Admin.SensorData.Post` privilege, and specify the node in the payload of the request.

Sensor Data
--------------



Nodes
--------

IEEE P1451.99 Bridge
-----------------------


