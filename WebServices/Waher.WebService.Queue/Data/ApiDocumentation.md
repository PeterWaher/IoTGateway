Title: Queue API Documentation
Description: This document provides an overview of the Queue API, including its endpoints, request and response formats, and usage examples.
Author: Peter Waher
Date: 2026-05-27
Master: /Master.md

=============================================================================================

Queue API
============

This endpoint allows clients to enqueue, peek and dequeue items from local queues.

![Table of Contents](ToC)

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

The authenticated user account must have the `Admin.Queues[.QUEUE_NAME].[OPERATION]` where 
`OPERATION` is the Queue operation requested (e.g. `Enqueue`, `Dequeue` and `Clear`). 
`QUEUE_NAME` is the name of the queue being accessed. For example, to enqueue an item to a queue
named `MyQueue`, the user account must have the `Admin.Queues.MyQueue.Enqueue` privilege in order 
to be authorized to enqueue an item.

HTTP Methods
---------------

The following HTTP methods are supported by the Queue API:

| Method   | Description                                                                 |
|:---------|:----------------------------------------------------------------------------|
| `GET`    | Retrieves this API documentation.                                           |
| `POST`   | Peeks or Dequeues an item from a specified queue.                           |
| `PUT`    | Enqueues an item to a specified queue.                                      |
| `DELETE` | Clears a queue of all items.                                                |

Specifying Queue
-------------------

The queue to be accessed is specified in the URL path. For example, to access a queue named
`MyQueue` on a domain `example.com`, the URL would be `https://example.com/Queues/MyQueue`.

Response codes
-----------------

The following table lists the possible response codes that the Sensor Data Receptor API may 
return:

| Code | Phrase                 | Description                                                                                  |
|-----:|:-----------------------|:---------------------------------------------------------------------------------------------|
| 200  | OK                     | Operation performed successfully, with item in the payload.                                  |
| 204  | No Content             | No item available to be dequeued, item successfully enqueued, or queue successfully cleared. |
| 400  | Bad Request            | Invalid or badly formatted payload.                                                          |
| 403  | Forbidden              | The user is not authorized to access the queue as attempted.                                 |
| 404  | Not Found              | If an item was not available on the queue.                                                   |
| 406  | Not Acceptable         | The requested item cannot be encoded as requested.                                           |
| 415  | Unsupported Media Type | The content could not be encoded or decoded.                                                 |
| 422  | Unprocessable Entity   | An item cannot be serialized, and cannot therefore be enqueued.                              |
| 503  | Service Unavailable    | The queue is full, and cannot accept more items at the current time.                         |

Content encoding and decoding
--------------------------------

Items dequeued from the queue are returned in the response body, encoded in accordance with 
the `Accept` header of the request. If no `Accept` header is specified, items are returned 
using a default encoding, as defined by the item itself. When encoding items, the server
will first decode the item using the Content-Type specified in the `Content-Type` header.

You can enqueue and dequeue multiple items at once, by using the `multipart/mixed` 
Content-Type.

Content-types supported for both encoding and decoding on the Neuron include:

{{
DecodeContentTypes:={};
EncodeContentTypes:={};
CodecContentTypes:={};

foreach s in Waher.Content.InternetContent.CanDecodeContentTypes do DecodeContentTypes[s]:=true;
foreach s in Waher.Content.InternetContent.CanEncodeContentTypes do EncodeContentTypes[s]:=true;
foreach s in DecodeContentTypes.Keys do if EncodeContentTypes.ContainsKey(s) and 
	!Waher.Networking.HTTP.HttpFolderResource.IsProtected(s) then CodecContentTypes[s]:=true;

foreach s in Sort(CodecContentTypes.Keys) do ]]
* `((s))`[[
}}

If JSON content is requested, use `application/json` in the `Accept` header. If dequeueing
multiple elements at once, use `multipart/mixed` in the `Accept` header, to ensure each item
is encoded separately in the response body, possibly using different Content-Types.

Items being enqueued or dequeued are encoded raw in the payload, in the format necessary to
encode or decode the object. To provide parameters for the call, query parameters are used.

**Note**: Enqueing and dequeueing passes two steps of encoding/decoding. When enqueuing, the
first step is to decode the content being enqueued from the request. This is done using the
`Content-Type` header of the request. It must be listed above. If this step fails, the web
service will respond with `400 Bad Request` or `415 Unsupported Media Type` errors. If
successful, the content passes on to the second step, which comprises of serializing the 
content for storage in the queue. Not all content decoded in the first step can be serialized
in the second step. If the second step fails, the web service will respond with a 
`422 Unprocessable Entity` error.
	
For dequeueing, the process is reversed: First, the item is deserialized, as defined in
the queue. Then, the item will be encoded, in accordance with the `Accept` header of the
request. If the item cannot be encoded in the format specified in the `Accept` header, the 
web service will respond with a `406 Not Acceptable` error.

Delayed responses
--------------------

The web service delays responses until the successful conclusion of an operation. This avoids
the client having to rapidly poll the web service for quick updates. It also avoids the need
for callbacks, requiring the client to be accessible publicly online. For performance reasons,
it is recommended the API be accessed using HTTP/2 (or later, when applicable), to better
stream requests and responses over a single connection, and simplifies management of delayed
responses.

Query Parameters
-------------------

Parameters to requests are provided as Query Parameters in the URL, not the content payload.
Default parameter values are used if parameters are not provided in the request. The service
may also modify the parameter values, if unsuitable or out-of-range. The following table lists
available query parameters and which operations they apply to:

| Query Parameter | HTTP Methods  | Queue Operations | Valid Range      | Description |
|:----------------|:--------------|:-----------------|-----------------:|:------------|
| `Timeout`       | `PUT`, `POST` | Enqueue, Dequeue | 0 - 90000 ms     | Time, in milliseconds, to wait for an item to be enqueued or dequeued. Default is 30000 (30 seconds). |
| `Count`         | `POST`        | Dequeue          | 1 -              | (Maximum) number of items to dequeue. Default is 1. If the `Count` query parameter is used, response will always be multipart/mixed, even if set to `1`. |
| `MinTimeout`    | `POST`        | Dequeue          | 0 - `Timeout` ms | Minimum time to wait for items, if at least one has been dequeued, but not all items requested. Only has effect if `Count` is provided and is larger than `1`. |
| `Peek`          | `POST`        | Dequeue          | 0 - 1            | If a Peek operation is to be performed, rather than a Dequeue operation (i.e. item is not removed from queue). Cannot be used together with the `Count` query parameter. |

**Note**: Using a `Timeout` of `0` means the web service will not wait for an item to be 
enqueued, if no item is available to be dequeued. The web service will respond immediately.
