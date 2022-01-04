# Request for Tenders - Transport Service \(Example\)

This contract defines an auction consignment of the service of buying a transport service. The assignment of a seller to this consignment is negotiated by an online auction managed by *TAG Marketplace*™. The document defines the rules for the automatic processing and matching of prospective sellers to this request for tenders.

## Introduction

This contract defines the request to buy the service of transportation of item\(s\) using the *TAG Marketplace*™ auctioning service. The **Buyer** have asked the **Auctioneer** to find the best match among offers from prospective *Sellers* to this Request for Tenders. The **Auctioneer** matches incoming bids from potential *Sellers*, until the best candidate is found, the first acceptable bid has been received, or the auction expires due to lack of sufficient interest.

## Manifest

The **Buyer** requests transport of **[%Quantity]** item\(s\) of **[%ProductNumber]** \([%Description]\).

## Pickup Point

The carrier will pick up the delivery at **[%FromAddress1] [%FromAddress2] [%FromAddress3]**, **[%FromPostalCode] [%FromCity]**,  **[%FromCountry]**.

## Delivery Point

The carrier will transport the delivery to **[%ToAddress1] [%ToAddress2] [%ToAddress3]**, **[%ToPostalCode] [%ToCity]**,  **[%ToCountry]**.

## Auction Consignment

Upon the signature of this contract, the **Buyer** consigns to the **Auctioneer** the task of finding a service provider that sells the requested service of transporting the item\(s\) specified in the *Manifest* above, with an initial *asking price* of **[%AskingPrice] [%Currency]**. The **Auctioneer** publishes information about the *Request for Tenders* to prospective *Sellers* who can make *Offers* for the duration of **[%AvailableDays]** days from the signature of the agreement, or until the first *acceptable* bid of **[%AcceptPrice] [%Currency]** is received. Any bid above **[%RejectPrice] [%Currency]** will be automatically rejected. Information of the best bid will also be presented.

The consignment concludes at the end of **[%AvailableDays]** days from the signature of the agreement, or when the first *acceptable* bid of **[%AcceptPrice] [%Currency]** is received. If no bids are available when the auction expires, the consignment concludes without any payments being processed. Otherwise, two payments will be made:

*	An instant payment representing the agreed upon price for the best or first accepted bid plus the agreed upon commission, as a percentage of the price \(**[%CommissionPercent] %**\), will be made from the **Buyer** to the *Auctioneer*.
*	An instant payment representing the agreed upon price for the best or first accepted bid, will be made from the **Auctioneer** to the **Seller**. The **Auctioneer** keeps its **[%CommissionPercent] %** as payment for its services.

All payments will be made using *e-Daler*. A buyer without funds in their *wallet* will have their request for tender invalidated, and the sale assignment cancelled.