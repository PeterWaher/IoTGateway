using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Security.PKCS
{
	/// <summary>
	/// Contains information about a Certificate Signing Request (CSR).
	/// </summary>
	public class CertificateRequest
	{
		private readonly SignatureAlgorithm signatureAlgorithm;
		private string commonName = null;                   // 2.5.4.3		CN
		private string surname = null;                      // 2.5.4.4
		private string serialNumber = null;                 // 2.5.4.5
		private string country = null;                      // 2.5.4.6		C
		private string locality = null;                     // 2.5.4.7		L
		private string stateOrProvince = null;              // 2.5.4.8		ST
		private string streetAddress = null;                // 2.5.4.9
		private string organization = null;                 // 2.5.4.10		O
		private string organizationalUnit = null;           // 2.5.4.11		OU
		private string title = null;                        // 2.5.4.12
		private string description = null;                  // 2.5.4.13
		private string postalAddress = null;                // 2.5.4.16
		private string postalCode = null;                   // 2.5.4.17
		private string postOfficeBox = null;                // 2.5.4.18
		private string physicalDeliveryOfficeName = null;   // 2.5.4.19
		private string telephoneNumber = null;              // 2.5.4.20
		private string registeredAddress = null;            // 2.5.4.26
		private string presentationAddress = null;          // 2.5.4.29
		private string name = null;                         // 2.5.4.41
		private string givenName = null;                    // 2.5.4.42
		private string initials = null;                     // 2.5.4.43
		private string distinguishedName = null;            // 2.5.4.49
		private string houseIdentifier = null;              // 2.5.4.51
		private string[] subjectAlternativeNames = null;    // 2.5.29.17
		private string emailAddress = null;                 // 1.2.840.113549.1.9.1

		/// <summary>
		/// Contains information about a Certificate Signing Request (CSR).
		/// </summary>
		/// <param name="SignatureAlgorithm">Signature algorithm.</param>
		public CertificateRequest(SignatureAlgorithm SignatureAlgorithm)
		{
			this.signatureAlgorithm = SignatureAlgorithm;
		}

		/// <summary>
		/// Signature algorithm.
		/// </summary>
		public SignatureAlgorithm SignatureAlgorithm
		{
			get { return this.signatureAlgorithm; }
		}

		/// <summary>
		/// Common Name (OID 2.5.4.3)
		/// </summary>
		public string CommonName
		{
			get { return this.commonName; }
			set { this.commonName = value; }
		}

		/// <summary>
		/// Surname (OID 2.5.4.4)
		/// </summary>
		public string Surname
		{
			get { return this.surname; }
			set { this.surname = value; }
		}

		/// <summary>
		/// Serial Number (OID 2.5.4.5)
		/// </summary>
		public string SerialNumber
		{
			get { return this.serialNumber; }
			set { this.serialNumber = value; }
		}

		/// <summary>
		/// Country Name (OID 2.5.4.6)
		/// </summary>
		public string Country
		{
			get { return this.country; }
			set { this.country = value; }
		}

		/// <summary>
		/// Locality Name (OID 2.5.4.7)
		/// </summary>
		public string Locality
		{
			get { return this.locality; }
			set { this.locality = value; }
		}

		/// <summary>
		/// Country Name (OID 2.5.4.8)
		/// </summary>
		public string StateOrProvince
		{
			get { return this.stateOrProvince; }
			set { this.stateOrProvince = value; }
		}

		/// <summary>
		/// Street Address (OID 2.5.4.9)
		/// </summary>
		public string StreetAddress
		{
			get { return this.streetAddress; }
			set { this.streetAddress = value; }
		}

		/// <summary>
		/// Organization Name (OID 2.5.4.10)
		/// </summary>
		public string Organization
		{
			get { return this.organization; }
			set { this.organization = value; }
		}

		/// <summary>
		/// Organizational Unit Name (OID 2.5.4.11)
		/// </summary>
		public string OrganizationalUnit
		{
			get { return this.organizationalUnit; }
			set { this.organizationalUnit = value; }
		}

		/// <summary>
		/// Title (OID 2.5.4.12)
		/// </summary>
		public string Title
		{
			get { return this.title; }
			set { this.title = value; }
		}

		/// <summary>
		/// Description (OID 2.5.4.13)
		/// </summary>
		public string Description
		{
			get { return this.description; }
			set { this.description = value; }
		}

		/// <summary>
		/// Postal Address (OID 2.5.4.16)
		/// </summary>
		public string PostalAddress
		{
			get { return this.postalAddress; }
			set { this.postalAddress = value; }
		}

		/// <summary>
		/// Postal Code (OID 2.5.4.17)
		/// </summary>
		public string PostalCode
		{
			get { return this.postalCode; }
			set { this.postalCode = value; }
		}

		/// <summary>
		/// Post Office Box (OID 2.5.4.18)
		/// </summary>
		public string PostOfficeBox
		{
			get { return this.postOfficeBox; }
			set { this.postOfficeBox = value; }
		}

		/// <summary>
		/// Physical Delivery Office Name (OID 2.5.4.19)
		/// </summary>
		public string PhysicalDeliveryOfficeName
		{
			get { return this.physicalDeliveryOfficeName; }
			set { this.physicalDeliveryOfficeName = value; }
		}

		/// <summary>
		/// Telephone number (OID 2.5.4.20)
		/// </summary>
		public string TelephoneNumber
		{
			get { return this.telephoneNumber; }
			set { this.telephoneNumber = value; }
		}

		/// <summary>
		/// Registered Address (OID 2.5.4.26)
		/// </summary>
		public string RegisteredAddress
		{
			get { return this.registeredAddress; }
			set { this.registeredAddress = value; }
		}

		/// <summary>
		/// Presentation Address  (OID 2.5.4.29)
		/// </summary>
		public string PresentationAddress
		{
			get { return this.presentationAddress; }
			set { this.presentationAddress = value; }
		}

		/// <summary>
		/// Name (OID 2.5.4.41)
		/// </summary>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Given Name (OID 2.5.4.42)
		/// </summary>
		public string GivenName
		{
			get { return this.givenName; }
			set { this.givenName = value; }
		}

		/// <summary>
		/// Initials (OID 2.5.4.43)
		/// </summary>
		public string Initials
		{
			get { return this.initials; }
			set { this.initials = value; }
		}

		/// <summary>
		/// Distinguished name (OID 2.5.4.49)
		/// </summary>
		public string DistinguishedName
		{
			get { return this.distinguishedName; }
			set { this.distinguishedName = value; }
		}

		/// <summary>
		/// House identifier (OID 2.5.4.51)
		/// </summary>
		public string HouseIdentifier
		{
			get { return this.houseIdentifier; }
			set { this.houseIdentifier = value; }
		}

		/// <summary>
		/// Subject Alternative Names (OID 2.5.29.17)
		/// </summary>
		public string[] SubjectAlternativeNames
		{
			get { return this.subjectAlternativeNames; }
			set { this.subjectAlternativeNames = value; }
		}

		/// <summary>
		/// e-Mail Address (OID 1.2.840.113549.1.9.1)
		/// </summary>
		public string EMailAddress
		{
			get { return this.emailAddress; }
			set { this.emailAddress = value; }
		}

		/// <summary>
		/// Building a Certificate Signing Request (CSR) in accordance with RFC 2986
		/// </summary>
		/// <returns>CSR</returns>
		public byte[] BuildCSR()
		{
			DerEncoder DER = new DerEncoder();

			DER.StartSEQUENCE();     // CertificationRequestInfo 
			DER.INTEGER(0);          // Version

			DER.StartSEQUENCE();     // subject
			this.EncodeIfDefined(DER, "2.5.4.3", this.commonName);
			this.EncodeIfDefined(DER, "2.5.4.4", this.surname);
			this.EncodeIfDefined(DER, "2.5.4.5", this.serialNumber);
			this.EncodeIfDefined(DER, "2.5.4.6", this.country);
			this.EncodeIfDefined(DER, "2.5.4.7", this.locality);
			this.EncodeIfDefined(DER, "2.5.4.8", this.stateOrProvince);
			this.EncodeIfDefined(DER, "2.5.4.9", this.streetAddress);
			this.EncodeIfDefined(DER, "2.5.4.10", this.organization);
			this.EncodeIfDefined(DER, "2.5.4.11", this.organizationalUnit);
			this.EncodeIfDefined(DER, "2.5.4.12", this.title);
			this.EncodeIfDefined(DER, "2.5.4.13", this.description);
			this.EncodeIfDefined(DER, "2.5.4.16", this.postalAddress);
			this.EncodeIfDefined(DER, "2.5.4.17", this.postalCode);
			this.EncodeIfDefined(DER, "2.5.4.18", this.postOfficeBox);
			this.EncodeIfDefined(DER, "2.5.4.19", this.physicalDeliveryOfficeName);
			this.EncodeIfDefined(DER, "2.5.4.20", this.telephoneNumber);
			this.EncodeIfDefined(DER, "2.5.4.26", this.registeredAddress);
			this.EncodeIfDefined(DER, "2.5.4.29", this.presentationAddress);
			this.EncodeIfDefined(DER, "2.5.4.41", this.name);
			this.EncodeIfDefined(DER, "2.5.4.42", this.givenName);
			this.EncodeIfDefined(DER, "2.5.4.43", this.initials);
			this.EncodeIfDefined(DER, "2.5.4.49", this.distinguishedName);
			this.EncodeIfDefined(DER, "2.5.4.51", this.houseIdentifier);
			this.EncodeIfDefined(DER, "1.2.840.113549.1.9.1", this.emailAddress);
			DER.EndSEQUENCE();       // end of subject

			DER.StartSEQUENCE();     // subjectPKInfo
			DER.StartSEQUENCE();     // algorithm
			DER.OBJECT_IDENTIFIER(this.signatureAlgorithm.PkiAlgorithmOID);
			DER.NULL();  // No parameters
			DER.EndSEQUENCE();       // end of algorithm
			DER.StartBITSTRING();    // subjectPublicKey

			this.signatureAlgorithm.ExportPublicKey(DER);

			DER.EndBITSTRING();      // end of subjectPublicKey
			DER.EndSEQUENCE();       // end of subjectPKInfo

			DER.StartContent(Asn1TypeClass.ContextSpecific);	// attributes

			if (this.subjectAlternativeNames != null && this.subjectAlternativeNames.Length > 0)
			{
				DER.StartSEQUENCE();
				DER.OBJECT_IDENTIFIER("1.2.840.113549.1.9.14");  // extensionRequest
				DER.StartSET();
				DER.StartSEQUENCE();
				DER.StartSEQUENCE();
				DER.OBJECT_IDENTIFIER("2.5.29.17");
				DER.StartOCTET_STRING();
				DER.StartSEQUENCE();

				foreach (string s in this.subjectAlternativeNames)
				{
					int Pos = DER.Position;
					DER.IA5_STRING(s);
					DER[Pos] = 0x82;	// Encoded as Context-specific INTEGER...
				}

				DER.EndSEQUENCE();
				DER.EndOCTET_STRING();
				DER.EndSEQUENCE();
				DER.EndSEQUENCE();
				DER.EndSET();
				DER.EndSEQUENCE();
			}

			DER.EndContent(Asn1TypeClass.ContextSpecific);	// end of attributes
			DER.EndSEQUENCE();       // end of CertificationRequestInfo

			byte[] CertificationRequestInfo = DER.ToArray();

			DER.Clear();
			DER.StartSEQUENCE();     // CertificationRequest
			DER.Raw(CertificationRequestInfo);

			DER.StartSEQUENCE();     // signatureAlgorithm
			DER.OBJECT_IDENTIFIER(this.signatureAlgorithm.HashAlgorithmOID);
			DER.NULL();              // parameters
			DER.EndSEQUENCE();       // End of signatureAlgorithm

			DER.BITSTRING(this.signatureAlgorithm.Sign(CertificationRequestInfo));  // signature

			DER.EndSEQUENCE();       // end of CertificationRequest

			return DER.ToArray();
		}

		private void EncodeIfDefined(DerEncoder DER, string OID, string Value)
		{
			if (Value != null)
			{
				DER.StartSET();
				DER.StartSEQUENCE();
				DER.OBJECT_IDENTIFIER(OID);

				if (DerEncoder.IsPrintable(Value))
					DER.PRINTABLE_STRING(Value);
				else
					DER.IA5_STRING(Value);

				DER.EndSEQUENCE();
				DER.EndSET();
			}
		}

	}
}
