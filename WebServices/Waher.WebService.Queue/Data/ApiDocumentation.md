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
`OPERATION` is the Queue operation requested (e.g. `Enqueue`, `Dequeue`, `Peek` and `Clear`). 
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

| Code | Phrase                | Description                                                     |
|-----:|:----------------------|:----------------------------------------------------------------|
| 200  | OK                    | Operation performed successfully, with item in the payload.     |
| 204  | No Content            | No item available to be dequeued.                               |
| 400  | Bad Request           | Invalid or badly formatted payload.                             |
| 403  | Forbidden             | The user is not authorized to access the queue as attempted.    |
| 406  | Not Acceptable        | The requested item cannot be encoded as requested.              |
| 500  | Internal Server Error | An item cannot be serialized, and cannot therefore be enqueued. |

Content encoding
-------------------

Items dequeued from the queue are returned in the response body, encoded in accordance with the
`Accept` header of the request. If no `Accept` header is specified, items are returned using
a default encoding, as defined by the item itself.

If JSON content is requested, use `application/json` in the `Accept` header. If dequeueing
multiple elements at once, use `multipart/mixed` in the `Accept` header, to ensure each item
is encoded separately in the response body, possibly using different Content-Types.

Content persistence
----------------------

Content enqueued must be persistable on the Neuron(R). When enqueueing content, the `Content-Type`
header of the request must be set to a value that indicates how the content is encoded. If the 
content is not persistable, or if the `Content-Type` header is missing or invalid, the server will 
respond with a `400 Bad Request` status code, or a `500 Internal Server Error`.
