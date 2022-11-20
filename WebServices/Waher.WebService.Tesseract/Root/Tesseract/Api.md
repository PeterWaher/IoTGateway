Title: Tesseract API
Description: Describes the Tesseract API
Date: 2022-11-20
Author: Peter Waher
Master: /Master.md

===========================================================================================================================

Tesseract API
=============================

The Tesseract API allows connected clients to perform OCR operations using Tesseract on the gateway. For this to work,
the Tesseract OCR application must be installed on the same machine as the IoT Gateway.

Authentication
----------------

To access the API, the client needs to authenticate itself. This is done using `WWW-Authenticate` in the HTTP request.
The client can choose to provide credentials that match a user on the gateway, or a Bearer-token containing a JWT-token
issued by the gateway, to gain access to the OCR api. No special privileges are required.

Performing OCR
----------------

OCR is performed, by sending a `POST` request to `/Tesseract/Apí` with an image as content. The image will be sent to
the Tesseract application, and the text result will be returned as plain text.

Personal Data
----------------

Any images send to the API for OCR will be stored on the gateway for up to 7 days, for troubleshooting and security
purposes. They are automatically deleted when the time interval elapses.


Reference links
------------------

Following are some Tesseract-related reference links that may be of interest:

* [Tesseract Downloads](https://tesseract-ocr.github.io/tessdoc/Downloads.html)
* [Windows Installer, from Mannheim University](https://github.com/UB-Mannheim/tesseract/wiki)
* [Tesseract Source Code](https://github.com/tesseract-ocr/)
* [Tesseract model for MRZ](https://github.com/DoubangoTelecom/tesseractMRZ)