Configuration Steps
======================

Depending on the modules available in the **IoT Gateway** installation, different configuration steps will be available during initial configuration
of the gateway. The following sections lists the steps available in the **IoT Gateway** itself, as well as some associated projects using the gateway
as a host. The sections will be ordered by priority. The priority is used to sort the steps, as they are being presented.

Overview
-----------

| Priority | Module        | Category    | Class                        |
|---------:|:--------------|:------------|:-----------------------------|
|     -100 | IoT Gateway   | Information | Introduction                 |
|        0 | IoT Gateway   | Database    | DatabaseConfiguration        |
|      100 | IoT Gateway   | Information | PersonalDataConfiguration    |
|      150 | IoT Gateway   | Database    | RestoreConfiguration         |
|      175 | IoT Gateway   | Database    | BackupConfiguration          |
|      190 | IoT Broker    | Internet    | InternetGatewayConfiguration |
|      200 | IoT Gateway   | Internet    | DomainConfiguration          |
|      250 | IoT Broker    | Internet    | DnsConfiguration             |
|      300 | IoT Gateway   | XMPP        | XmppConfiguration            |
|      320 | IoT Gateway   | XMPP        | LegalIdentityConfiguration   |
|      350 | Neuro-Ledger  | Ledger      | LedgerConfiguration          |
|      380 | IoT Broker    | XMPP        | PeerReviewConfiguration      |
|      400 | IoT Gateway   | XMPP        | RosterConfiguration          |
|      450 | IoT Broker    | Mail        | RelayConfiguration           |
|      460 | IoT Broker    | Push        | PushConfiguration            |
|      500 | IoT Gateway   | GUI         | ThemeConfiguration           |
|      600 | IoT Gateway   | Operation   | NotificationConfiguration    |


Introduction
---------------

| Configuration step                                                      ||
|:------------|:-----------------------------------------------------------|
| Priority    | -100                                                       |
| Module      | IoT Gateway                                                |
| Category    | Information                                                |
| Class       | `Waher.IoTGateway.Setup.Introduction`                      |
| Description | Selects between *simplified* and *detailed* configuration. |

| Environment Variable   | Description                                                            |
|:-----------------------|:-----------------------------------------------------------------------|
| `GATEWAY_SIMPLE_SETUP` | `true` or `1` for simplified setup, `false` or `0` for detailed setup. |


Database Configuration
--------------------------

| Configuration step                                          ||
|:------------|:-----------------------------------------------|
| Priority    | 0                                              |
| Module      | IoT Gateway                                    |
| Category    | Database                                       |
| Class       | `Waher.IoTGateway.Setup.DatabaseConfiguration` |
| Description | Configures main database provider to use.      |

| Environment Variable   | Description                                           |
|:-----------------------|:------------------------------------------------------|
| `GATEWAY_DB_PROVIDER`  | Fully qualified name of the database provider to use. |

Configuration of the specific provider depends on the type of provider selected, as follows:

### Internal Database

| Environment Variable   | Description                                         |
|:-----------------------|:----------------------------------------------------|
| `GATEWAY_DB_PROVIDER`  | `Waher.IoTGateway.Setup.Databases.InternalDatabase` |

### MongoDB

| Environment Variable   | Description                                                                                              |
|:-----------------------|:---------------------------------------------------------------------------------------------------------|
| `GATEWAY_DB_PROVIDER`  | `Waher.IoTGateway.Setup.Databases.MongoDBDatabase`                                                       |
| `MONGO_DB_HOST`        | Host name of MongoDB database service.                                                                   |
| `MONGO_DB_NAME`        | Name of database to connect to.                                                                          |
| `MONGO_DB_DEFAULT`     | Optional name of default collection. If not provided, the default collection name will be `Default`.     |
| `MONGO_DB_USER`        | User name to use when connecting to MongoDB database service.                                            |
| `MONGO_DB_PASSWORD`    | Password to authenticate user.                                                                           |
| `MONGO_DB_PORT`        | Optional port number used in connnection. If not provided, the library default port number will be used. |


Personal Data Processing Configuration
-----------------------------------------

| Configuration step                                                      ||
|:------------|:-----------------------------------------------------------|
| Priority    | 100                                                        |
| Module      | IoT Gateway                                                |
| Category    | Information                                                |
| Class       | `Waher.IoTGateway.Setup.PersonalDataConfiguration`         |
| Description | Requests consent for processing of personal information.   |

| Environment Variable  | Description                                                                                  |
|:----------------------|:---------------------------------------------------------------------------------------------|
| `GATEWAY_PII_CONSENT` | `true` or `1` to give consent for processing personal inforamtion, `false` or `0` otherwise. |


Restore Configuration
------------------------

| Configuration step                                                   ||
|:------------|:--------------------------------------------------------|
| Priority    | 150                                                     |
| Module      | IoT Gateway                                             |
| Category    | Database                                                |
| Class       | `Waher.IoTGateway.Setup.RestoreConfiguration`           |
| Description | Allows a previous backup to be restored on the gateway. |

| Environment Variable          | Description                                                                                                                                         |
|:------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_RESTORE`             | `true` or `1` if a restore should be performed, `false` or `0` if no restore should be performed. (If not, the following variables can be skipped.) |
| `GATEWAY_RESTORE_BAKFILE`     | File name of backup file to restore.                                                                                                                |
| `GATEWAY_RESTORE_KEYFILE`     | Optional file name of key file corresponding to the backup file, if available.                                                                      |
| `GATEWAY_RESTORE_OVERWRITE`   | If restore should overwrite existing information.                                                                                                   |
| `GATEWAY_RESTORE_COLLECTIONS` | Optional comma-separated list of collections to restore. Empty value represents all collections.                                                    |
| `GATEWAY_RESTORE_PARTS`       | Optional comma-separated list of parts to restore. Empty value represents all parts. Available parts include `Database`, `Ledger` and `Files`.      |

**Note**: The list of parts can be extended by modules hosted on the gateway.


Backup Configuration
------------------------

| Configuration step                                                                                         ||
|:------------|:----------------------------------------------------------------------------------------------|
| Priority    | 175                                                                                           |
| Module      | IoT Gateway                                                                                   |
| Category    | Database                                                                                      |
| Class       | `Waher.IoTGateway.Setup.BackupConfiguration`                                                  |
| Description | Configures when backups are performed, where they are stored, and for how long they are kept. |

| Environment Variable          | Description                                                                                                                                                           |
|:------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_BACKUP`              | `true` or `1` if automatic backups should be performed, `false` or `0` if no automatic backups should be performed. (If not, the following variables can be skipped.) |
| `GATEWAY_BACKUP_TIME`         | A TimeSpan value representing a time of day, determining when automatic backups are performed.                                                                        |
| `GATEWAY_BACKUP_DAYS`         | the number of days daily backups are kept.                                                                                                                            |
| `GATEWAY_BACKUP_MONTHS`       | the number of months monthly backups are kept.                                                                                                                        |
| `GATEWAY_BACKUP_YEARS`        | the number of years yearly backups are kept.                                                                                                                          |
| `GATEWAY_BACKUP_FOLDER`       | Optional variable determining the folder backup files are to be stored, if different from the default backup folder.                                                  |
| `GATEWAY_KEY_FOLDER`          | Optional variable determining the folder key files are to be stored, if different from the default key folder.                                                        |
| `GATEWAY_BACKUP_HOSTS`        | A comma-separated list of secondary backup hosts for redundant storage of backup files.                                                                               |
| `GATEWAY_KEY_HOSTS`           | A comma-separated list of secondary key hosts for redundant storage of key files.                                                                                     |


Internet Gateway Configuration
---------------------------------

| Configuration step                                                                                      ||
|:------------|:-------------------------------------------------------------------------------------------|
| Priority    | 190                                                                                        |
| Module      | IoT Broker                                                                                 |
| Category    | Internet                                                                                   |
| Class       | `Waher.Service.IoTBroker.Setup.InternetGatewayConfiguration`                               |
| Description | Configures if the broker should register itself in available Internet Gateways in the LAN. |

| Environment Variable          | Description                                                                                                                                                      |
|:------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BROKER_INET_GATEWAY_REG`     | `true` or `1` if the broker should register itself in any Internet Gateway it finds in the Local Area Network, `false` or `0` if no registration should be made. |


Domain Configuration
------------------------

| Configuration step                                                                                                                                          ||
|:------------|:-----------------------------------------------------------------------------------------------------------------------------------------------|
| Priority    | 200                                                                                                                                            |
| Module      | IoT Gateway                                                                                                                                    |
| Category    | Internet                                                                                                                                       |
| Class       | `Waher.IoTGateway.Setup.DomainConfiguration`                                                                                                   |
| Description | Configures names for the Gateway. This includes domain names, alternative names, dynamic DNS, and human-readable names in different languages. |

| Environment Variable          | Description                                                                                                                                                |
|:------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_DOMAIN_USE`          | `true` or `1` if gateway uses a domain name, `false` or `0` if not.                                                                                        |
| `GATEWAY_HR_NAME`             | Default Human-readable name for gateway.                                                                                                                   |
| `GATEWAY_HR_NAME_LANG`        | Language code (ISO-639-1) of `GATEWAY_HR_NAME`.                                                                                                            |
| `GATEWAY_HR_DESC`             | Default Human-readable description of gateway.                                                                                                             |
| `GATEWAY_HR_DESC_LANG`        | Language code (ISO-639-1) of `GATEWAY_HR_DESC`.                                                                                                            |
| `GATEWAY_HR_NAME_LOC`         | Comma-separated list of Language Codes (ISO-639-1) for available localizations of the human-readable name for the gateway.                                 |
| `GATEWAY_HR_NAME_[lang]`      | Localized Human-readable name for the gateway, where `[lang]` is replaced by any of the ISO-639-1 language codes available in `GATEWAY_HR_NAME_LOC`.       |
| `GATEWAY_HR_DESC_LOC`         | Comma-separated list of Language Codes (ISO-639-1) for available localizations of the human-readable description of the gateway.                           |
| `GATEWAY_HR_DESC_[lang]`      | Localized Human-readable description of the gateway, where `[lang]` is replaced by any of the ISO-639-1 language codes available in `GATEWAY_HR_DESC_LOC`. |

If use of a Domain Name is configured (`GATEWAY_DOMAIN_USE` variable is `true` or `1`), the following variables define its operation:

| Environment Variable          | Description                                                                                                                                                                                                  |
|:------------------------------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_DOMAIN_NAME`         | Main Domain Name of the gateway.                                                                                                                                                                             |
| `GATEWAY_DOMAIN_ALT_NAMES`    | Comma-separated list of alternative domain names for the gateway, if defined.                                                                                                                                |
| `GATEWAY_DYNDNS`              | `true` or `1` if gateway should use a Dynamic DNS-service, `false` or `0` if IP-address of Gateway is static.                                                                                                |
| `GATEWAY_ENCRYPTION`          | `true` or `1` if gateway should use X.509-based encryption (for example TLS over HTTP, HTTPS), `false` or `0` if encryption is disabled.                                                                     |

If Encryption is configured (`GATEWAY_ENCRYPTION` variable), the following variables define its operation:

| Environment Variable          | Description                                                                                                                                                                                                  |
|:------------------------------|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_CA_CUSTOM`           | `true` or `1` if gateway should use a custom Certificate Authority (must support ACME), `false` or `0` if [Let's Encrypt](https://letsencrypt.org/) should be used to generate certificates for the gateway. |
| `GATEWAY_ACME_EMAIL`          | E-mail address for contact person associated with generated certificates.                                                                                                                                    |
| `GATEWAY_ACME_ACCEPT_TOS`     | If Certificate Authority Terms of Services are accepted.                                                                                                                                                     |

If Dynamic DNS is configured (`GATEWAY_DYNDNS` variable), the following variables define its operation:

| Environment Variable          | Description                                                                                      |
|:------------------------------|:-------------------------------------------------------------------------------------------------|
| `GATEWAY_DYNDNS_TEMPLATE`     | Name of template to use for reporting IP address changes to the Dynamic DNS-service.             |
| `GATEWAY_DYNDNS_CHECK`        | Script to use to check the current public IP address of the gateway.                             |
| `GATEWAY_DYNDNS_UPDATE`       | Script to use to update the current public IP address of the gateway in the Dynamic DNS service. |
| `GATEWAY_DYNDNS_ACCOUNT`      | Account of the gateway in the Dynamic DNS service.                                               |
| `GATEWAY_DYNDNS_PASSWORD`     | Password of the Dynamic DNS service account.                                                     |
| `GATEWAY_DYNDNS_INTERVAL`     | Interval (in seconds) for checking if the IP address has changed.                                |

If a Custom Certificate Authority is configured (`GATEWAY_CA_CUSTOM` variable), the following variables define its operation:

| Environment Variable          | Description                                                                                                        |
|:------------------------------|:-------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_ACME_DIRECTORY`      | URL to the custom ACME directory to use to generate certificates for the gateway if a custom CA has been selected. |


DNS Configuration
-------------------

| Configuration step                                                                                   ||
|:------------|:----------------------------------------------------------------------------------------|
| Priority    | 250                                                                                     |
| Module      | IoT Broker                                                                              |
| Category    | Internet                                                                                |
| Class       | `Waher.Service.IoTBroker.Setup.DnsConfiguration`                                        |
| Description | Tests the DNS to see if necessary records are available. No configuration is performed. |

**Note**: No configuration is performed in this step. The configuration step allows the installer to review necessary DNS settings.
When configuring the system using environment variables, the test will also be performed. Any errors will be logged to the event log,
but configuration will not fail.


XMPP Configuration
---------------------

| Configuration step                                                                               ||
|:------------|:------------------------------------------------------------------------------------|
| Priority    | 300                                                                                 |
| Module      | IoT Gateway                                                                         |
| Category    | XMPP                                                                                |
| Class       | `Waher.IoTGateway.Setup.XmppConfiguration`                                          |
| Description | Configures communication settings for how the gateway connects to the XMPP network. |

| Environment Variable          | Description                                                                                                                                                                        |
|:------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_XMPP_HOST`           | XMPP broker to connect to.                                                                                                                                                         |
| `GATEWAY_XMPP_TRANSPORT`      | XMPP transport method (a.k.a. binding). Can be `C2S` (default if variable not available), `BOSH` (Bidirectional HTTP) or `WS` (Web-socket).                                        |
| `GATEWAY_XMPP_PORT`           | Optional Port number to use when connecting to host. (If `C2S` binding has been selected.) If not provided, the default port number will be used.                                  |
| `GATEWAY_XMPP_BOSHURL`        | URL to use when connecting to host. (If `BOSH` binding has been selected).                                                                                                         |
| `GATEWAY_XMPP_WSURL`          | URL to use when connecting to host. (If `WS` binding has been selected).                                                                                                           |
| `GATEWAY_XMPP_CREATE`         | If an account is to be created.                                                                                                                                                    |
| `GATEWAY_XMPP_CREATE_KEY`     | API-Key to use when creating account, if host is not one of the featured hosts.                                                                                                    |
| `GATEWAY_XMPP_CREATE_SECRET`  | API-Key secret to use when creating account, if host is not one of the featured hosts.                                                                                             |
| `GATEWAY_XMPP_ACCOUNT`        | Name of account.                                                                                                                                                                   |
| `GATEWAY_XMPP_PASSWORD`       | Password of account. If creating an account, this variable is optional. If not available, a secure password will be generated.                                                     |
| `GATEWAY_XMPP_ACCOUNT_NAME`   | Optional Human-readable name of account.                                                                                                                                           |
| `GATEWAY_XMPP_LOG`            | Optional. `true` or `1` if gateway should log communication to program data folder, `false` or `0` if communication should not be logged (default).                                |
| `GATEWAY_XMPP_TRUST`          | Optional. `true` or `1` if gateway should trust server certificate, even if it does not validate, `false` or `0` if server should be distrusted (default).                         |
| `GATEWAY_XMPP_OBS_AUTH`       | Optional. `true` or `1` if gateway should be allowed to use obsolete and insecure authentication mechanisms, `false` or `0` if only secure mechanisms should be allowed (default). |
| `GATEWAY_XMPP_CLEAR_PWD`      | Optional. `true` or `1` if gateway should store password as-is in the database, `false` or `0` if only the password hash should be stored (default).                               |


Legal Identity Configuration
-------------------------------

| Configuration step                                                                                    ||
|:------------|:-----------------------------------------------------------------------------------------|
| Priority    | 320                                                                                      |
| Module      | IoT Gateway                                                                              |
| Category    | XMPP                                                                                     |
| Class       | `Waher.IoTGateway.Setup.LegalIdentityConfiguration`                                      |
| Description | Configures an optional legal identity of the gateway, and sends an identity application. |

| Environment Variable          | Description                                                                                                                                                 |
|:------------------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `GATEWAY_ID_USE`              | If a legal identity is to be used by the gateway. If used, the folllowing optional variables can be used to provide information going into the application. |
| `GATEWAY_ID_FIRST`            | First name of legal identity.                                                                                                                               |
| `GATEWAY_ID_MIDDLE`           | Middle name of legal identity.                                                                                                                              |
| `GATEWAY_ID_LAST`             | Last name of legal identity.                                                                                                                                |
| `GATEWAY_ID_PNR`              | Personal number of legal identity.                                                                                                                          |
| `GATEWAY_ID_ADDR`             | Address (line 1) of legal identity.                                                                                                                         |
| `GATEWAY_ID_ADDR2`            | Address (line 2) of legal identity.                                                                                                                         |
| `GATEWAY_ID_ZIP`              | Postal code of legal identity.                                                                                                                              |
| `GATEWAY_ID_AREA`             | Area of legal identity.                                                                                                                                     |
| `GATEWAY_ID_CITY`             | City of legal identity.                                                                                                                                     |
| `GATEWAY_ID_REGION`           | Region of legal identity.                                                                                                                                   |
| `GATEWAY_ID_COUNTRY`          | Country of legal identity.                                                                                                                                  |
| `GATEWAY_ID_NATIONALITY`      | Nationality of legal identity.                                                                                                                              |
| `GATEWAY_ID_GENDER`           | Gender of legal identity.                                                                                                                                   |
| `GATEWAY_ID_BDATE`            | Birth Date of legal identity.                                                                                                                               |
| `GATEWAY_ID_ORGNAME`          | Organization name of legal identity.                                                                                                                        |
| `GATEWAY_ID_ORGDEPT`          | Organization department of legal identity.                                                                                                                  |
| `GATEWAY_ID_ORGROLE`          | Organization role of legal identity.                                                                                                                        |
| `GATEWAY_ID_ORGNR`            | Organization number of legal identity.                                                                                                                      |
| `GATEWAY_ID_ORGADDR`          | Organization address (line 1) of legal identity.                                                                                                            |
| `GATEWAY_ID_ORGADDR2`         | Organization address (line 2) of legal identity.                                                                                                            |
| `GATEWAY_ID_ORGZIP`           | Organization postal code of legal identity.                                                                                                                 |
| `GATEWAY_ID_ORGAREA`          | Organization area of legal identity.                                                                                                                        |
| `GATEWAY_ID_ORGCITY`          | Organization city of legal identity.                                                                                                                        |
| `GATEWAY_ID_ORGREGION`        | Organization region of legal identity.                                                                                                                      |
| `GATEWAY_ID_ORGCOUNTRY`       | Organization country of legal identity.                                                                                                                     |
| `GATEWAY_ID_ALT`              | Comma-separated list of alternative fields to send in identity application.                                                                                 |
| `GATEWAY_ID_ALT_[field]`      | Value for alternative field `[field]` to send in the identity application.                                                                                  |
| `GATEWAY_ID_PASSWORD`         | Protect legal identity with this password.                                                                                                                  |


Neuro-Ledger Configuration
------------------------------

| Configuration step                                                 ||
|:------------|:------------------------------------------------------|
| Priority    | 350                                                   |
| Module      | Neuro-Ledger                                          |
| Category    | Ledger                                                |
| Class       | `Waher.Service.NeuroLedger.Setup.LedgerConfiguration` |
| Description | Configures collection parameter for the Neuro-Ledger. |

| Environment Variable          | Description                                                                        |
|:------------------------------|:-----------------------------------------------------------------------------------|
| `NEURO_LEDGER_COLLECTION`     | Collection time in seconds. If not provided, the default value will be used.       |
| `NEURO_LEDGER_MAXSIZE`        | Maximum size of blocks, in bytes. If not provided, the default value will be used. |


Peer Review Configuration
----------------------------

| Configuration step                                                                                   ||
|:------------|:----------------------------------------------------------------------------------------|
| Priority    | 380                                                                                     |
| Module      | IoT Broker                                                                              |
| Category    | XMPP                                                                                    |
| Class       | `Waher.Service.IoTBroker.Setup.PeerReviewConfiguration`                                 |
| Description | Configures requirements for peer-review of legal identities on the broker.              |

| Environment Variable        | Description                                                                                                                                                     |
|:----------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `BROKER_REVIEW_USE`         | If peer review is allowed on the broker (`true` or `1`), or not (`false` or `0`). If enabled, the following variables control what parameters must be included: |
| `BROKER_REVIEW_NRPEERS`     | Number of peers required to review and approve a legal identity application before it can be approved.                                                          |
| `BROKER_REVIEW_NRPHOTOS`    | Number of photos required in a legal identity application for a peer review to be accepted.                                                                     |
| `BROKER_REVIEW_FIRST`       | If first-name is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                            |
| `BROKER_REVIEW_MIDDLE`      | If middle-name is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                           |
| `BROKER_REVIEW_LAST`        | If last-name is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                             |
| `BROKER_REVIEW_PNR`         | If personal number is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                       |
| `BROKER_REVIEW_COUNTRY`     | If country is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                               |
| `BROKER_REVIEW_REGION`      | If region is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                                |
| `BROKER_REVIEW_CITY`        | If city is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                                  |
| `BROKER_REVIEW_AREA`        | If area is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                                  |
| `BROKER_REVIEW_ZIP`         | If postal code is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                           |
| `BROKER_REVIEW_ADDR`        | If address is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                               |
| `BROKER_REVIEW_ISO3166`     | If country codes must comply with ISO 3166 in a peer review (`true` or `1`), or not (`false` or `0`).                                                           |
| `BROKER_REVIEW_NATIONALITY` | If nationality is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                           |
| `BROKER_REVIEW_GENDER`      | If gender is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                                |
| `BROKER_REVIEW_BDATE`       | If birth date is required in a peer review (`true` or `1`), or not (`false` or `0`).                                                                            |


Roster Configuration
-----------------------

| Configuration step                                                        ||
|:------------|:-------------------------------------------------------------|
| Priority    | 400                                                          |
| Module      | IoT Gateway                                                  |
| Category    | XMPP                                                         |
| Class       | `Waher.IoTGateway.Setup.RosterConfiguration`                 |
| Description | Configures contacts that should be added to the XMPP roster. |

| Environment Variable          | Description                                                                               |
|:------------------------------|:------------------------------------------------------------------------------------------|
| `GATEWAY_ROSTER_ADD`          | Optional Comma-separated list of Bare JIDs to add to the roster.                          |
| `GATEWAY_ROSTER_SUBSCRIBE`    | Optional Comma-separated list of Bare JIDs to send presence subscription requests to.     |
| `GATEWAY_ROSTER_ACCEPT`       | Optional Comma-separated list of Bare JIDs to accept presence subscription requests from. |
| `GATEWAY_ROSTER_GROUPS`       | Optional Comma-separated list of groups to define.                                        |
| `GATEWAY_ROSTER_GRP_[group]`  | Optional Comma-separated list of Bare JIDs in the roster to add to the group `[group]`.   |
| `GATEWAY_ROSTER_NAME_[jid]`   | Optional human-readable name of a JID in the roster.                                      |


Mail Relay Configuration
----------------------------

| Configuration step                                              ||
|:------------|:---------------------------------------------------|
| Priority    | 450                                                |
| Module      | Broker                                             |
| Category    | Mail                                               |
| Class       | `Waher.Service.IoTBroker.Setup.RelayConfiguration` |
| Description | Configures e-mail Relay settings for the broker.   |

| Environment Variable   | Description                                                                                                                                      |
|:-----------------------|:-------------------------------------------------------------------------------------------------------------------------------------------------|
| `BROKER_RELAY_USE`     | If an SMTP relay server is to be used (`true` or `1`), or if the broker should connect to the recipient mail exchange directly (`false` or `0`). |
| `BROKER_RELAY_DOMAINS` | Optional Comma-separated list of domain names for which the broker can act as an SMTP relay.                                                     |
| `BROKER_RELAY_SENDER`  | Default sender of mail messages from broker.                                                                                                     |

If you choose to use a relay server to send e-mail (`NEURO_RELAY_USE` is `true`or `1`), the following variables configure the connection to the
relay server:

| Environment Variable    | Description                                                         |
|:------------------------|:--------------------------------------------------------------------|
| `BROKER_RELAY_HOST`     | Host (or domain) or the SMTP Relay server.                          |
| `BROKER_RELAY_PORT`     | Port number to use when connecting relay server.                    |
| `BROKER_RELAY_USER`     | User account in the relay server.                                   |
| `BROKER_RELAY_PASSWORD` | Password of account when authenticating access to the relay server. |


Push Notification Configuration
----------------------------------

| Configuration step                                                         ||
|:------------|:--------------------------------------------------------------|
| Priority    | 460                                                           |
| Module      | Broker                                                        |
| Category    | Push                                                          |
| Class       | `Waher.Service.IoTBroker.Setup.PushNotificationConfiguration` |
| Description | Configures Push-Notification settings for the broker.         |

| Environment Variable        | Description                                                                                                                       |
|:----------------------------|:----------------------------------------------------------------------------------------------------------------------------------|
| `BROKER_FIREBASE_USE`       | If Firebase should be used to push notifications to clients when they are not connected (`true` or `1`), or not (`false` or `0`). |
| `BROKER_FIREBASE_SERVICEID` | Firebase Service ID, identifying the service in Firebase.                                                                         |
| `BROKER_FIREBASE_SERVERKEY` | Server Key, authenticating access of the service in Firebase.                                                                     |


Theme Configuration
----------------------

| Configuration step                                                        ||
|:------------|:-------------------------------------------------------------|
| Priority    | 500                                                          |
| Module      | IoT Gateway                                                  |
| Category    | GUI                                                          |
| Class       | `Waher.IoTGateway.Setup.ThemeConfiguration`                  |
| Description | Configures contacts that should be added to the XMPP roster. |

| Environment Variable          | Description         |
|:------------------------------|:--------------------|
| `GATEWAY_THEME_ID`            | ID of theme to use. |


Notification Configuration
-----------------------------

| Configuration step                                                                                 ||
|:------------|:--------------------------------------------------------------------------------------|
| Priority    | 600                                                                                   |
| Module      | IoT Gateway                                                                           |
| Category    | Operation                                                                             |
| Class       | `Waher.IoTGateway.Setup.NotificationConfiguration`                                    |
| Description | Configures who gets notified of important events, and who can administer the gateway. |

| Environment Variable          | Description                   |
|:------------------------------|:------------------------------|
| `GATEWAY_NOTIFICATION_JIDS`   | JIDs of operators of gateway. |
