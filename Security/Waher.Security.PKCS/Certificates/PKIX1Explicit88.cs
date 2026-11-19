using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Security\Waher.Security.PKCS\Certificates\Asn1\PKIX1Explicit88.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Security.PKCS.Certificates.PKIX1Explicit88
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId id_pkix = new Asn1.ObjectId(1 /* iso */, 3 /* identified_organization */, 6 /* dod */, 1 /* internet */, 5 /* security */, 5 /* mechanisms */, 7 /* pkix */);
		public static readonly Asn1.ObjectId id_pe = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pkix, 1);
		public static readonly Asn1.ObjectId id_qt = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pkix, 2);
		public static readonly Asn1.ObjectId id_kp = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pkix, 3);
		public static readonly Asn1.ObjectId id_ad = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_pkix, 48);
		public static readonly Asn1.ObjectId id_qt_cps = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_qt, 1);
		public static readonly Asn1.ObjectId id_qt_unotice = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_qt, 2);
		public static readonly Asn1.ObjectId id_ad_ocsp = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_ad, 1);
		public static readonly Asn1.ObjectId id_ad_caIssuers = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_ad, 2);
		public static readonly Asn1.ObjectId id_ad_timeStamping = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_ad, 3);
		public static readonly Asn1.ObjectId id_ad_caRepository = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_ad, 5);
		public static readonly Asn1.ObjectId id_at = new Asn1.ObjectId(2 /* joint_iso_ccitt */, 5 /* ds */, 4);
		public static readonly Asn1.ObjectId id_at_name = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 41);
		public static readonly Asn1.ObjectId id_at_surname = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 4);
		public static readonly Asn1.ObjectId id_at_givenName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 42);
		public static readonly Asn1.ObjectId id_at_initials = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 43);
		public static readonly Asn1.ObjectId id_at_generationQualifier = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 44);
		public static readonly Asn1.Construct.ISet teletexStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_name);
		public static readonly Asn1.Construct.ISet printableStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_name);
		public static readonly Asn1.Construct.ISet universalStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_name);
		public static readonly Asn1.Construct.ISet utf8StringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_name);
		public static readonly Asn1.Construct.ISet bmpStringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_name);
		public static readonly Asn1.ObjectId id_at_commonName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 3);
		public static readonly Asn1.Construct.ISet teletexStringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_common_name);
		public static readonly Asn1.Construct.ISet printableStringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_common_name);
		public static readonly Asn1.Construct.ISet universalStringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_common_name);
		public static readonly Asn1.Construct.ISet utf8StringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_common_name);
		public static readonly Asn1.Construct.ISet bmpStringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_common_name);
		public static readonly Asn1.ObjectId id_at_localityName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 7);
		public static readonly Asn1.Construct.ISet teletexStringSize3 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_locality_name);
		public static readonly Asn1.Construct.ISet printableStringSize3 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_locality_name);
		public static readonly Asn1.Construct.ISet universalStringSize3 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_locality_name);
		public static readonly Asn1.Construct.ISet utf8StringSize3 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_locality_name);
		public static readonly Asn1.Construct.ISet bmpStringSize3 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_locality_name);
		public static readonly Asn1.ObjectId id_at_stateOrProvinceName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 8);
		public static readonly Asn1.Construct.ISet teletexStringSize4 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_state_name);
		public static readonly Asn1.Construct.ISet printableStringSize4 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_state_name);
		public static readonly Asn1.Construct.ISet universalStringSize4 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_state_name);
		public static readonly Asn1.Construct.ISet utf8StringSize4 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_state_name);
		public static readonly Asn1.Construct.ISet bmpStringSize4 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_state_name);
		public static readonly Asn1.ObjectId id_at_organizationName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 10);
		public static readonly Asn1.Construct.ISet teletexStringSize5 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name);
		public static readonly Asn1.Construct.ISet printableStringSize5 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name);
		public static readonly Asn1.Construct.ISet universalStringSize5 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name);
		public static readonly Asn1.Construct.ISet utf8StringSize5 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name);
		public static readonly Asn1.Construct.ISet bmpStringSize5 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name);
		public static readonly Asn1.ObjectId id_at_organizationalUnitName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 11);
		public static readonly Asn1.Construct.ISet teletexStringSize6 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organizational_unit_name);
		public static readonly Asn1.Construct.ISet printableStringSize6 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organizational_unit_name);
		public static readonly Asn1.Construct.ISet universalStringSize6 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organizational_unit_name);
		public static readonly Asn1.Construct.ISet utf8StringSize6 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organizational_unit_name);
		public static readonly Asn1.Construct.ISet bmpStringSize6 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organizational_unit_name);
		public static readonly Asn1.ObjectId id_at_title = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 12);
		public static readonly Asn1.Construct.ISet teletexStringSize7 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_title);
		public static readonly Asn1.Construct.ISet printableStringSize7 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_title);
		public static readonly Asn1.Construct.ISet universalStringSize7 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_title);
		public static readonly Asn1.Construct.ISet utf8StringSize7 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_title);
		public static readonly Asn1.Construct.ISet bmpStringSize7 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_title);
		public static readonly Asn1.ObjectId id_at_dnQualifier = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 46);
		public static readonly Asn1.ObjectId id_at_countryName = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 6);
		public static readonly Asn1.ObjectId id_at_serialNumber = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 5);
		public static readonly Asn1.ObjectId id_at_pseudonym = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.id_at, 65);
		public static readonly Asn1.Construct.ISet teletexStringSize8 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pseudonym);
		public static readonly Asn1.Construct.ISet printableStringSize8 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pseudonym);
		public static readonly Asn1.Construct.ISet universalStringSize8 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pseudonym);
		public static readonly Asn1.Construct.ISet utf8StringSize8 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pseudonym);
		public static readonly Asn1.Construct.ISet bmpStringSize8 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pseudonym);
		public static readonly Asn1.ObjectId id_domainComponent = new Asn1.ObjectId(0, 9, 2342, 19200300, 100, 1, 25);
		public static readonly Asn1.ObjectId pkcs_9 = new Asn1.ObjectId(1 /* iso */, 2 /* member_body */, 840 /* us */, 113549 /* rsadsi */, 1 /* pkcs */, 9);
		public static readonly Asn1.ObjectId id_emailAddress = new Asn1.ObjectId(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.pkcs_9, 1);
		public static readonly Asn1.Construct.ISet teletexStringSize9 = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(1);
		public static readonly Asn1.Construct.ISet printableStringSize9 = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(1);
		public static readonly Asn1.Construct.ISet universalStringSize9 = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(1);
		public static readonly Asn1.Construct.ISet utf8StringSize9 = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(1);
		public static readonly Asn1.Construct.ISet bmpStringSize9 = new Asn1.Construct.IntervalOpenUp<System.Numerics.BigInteger>(1);
		public static readonly Asn1.Construct.ISet network_addressSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_x121_address_length);
		public static readonly Asn1.Construct.ISet terminal_identifierSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_terminal_id_length);
		public static readonly Asn1.Construct.ISet organization_nameSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_organization_name_length);
		public static readonly Asn1.Construct.ISet numeric_user_identifierSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_numeric_user_id_length);
		public static readonly Asn1.Construct.ISet x121_dcc_codeSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_country_name_numeric_length);
		public static readonly Asn1.Construct.ISet iso_3166_alpha2_codeSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_country_name_alpha_length);
		public static readonly Asn1.Construct.ISet numericSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_name_length);
		public static readonly Asn1.Construct.ISet printableSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_name_length);
		public static readonly Asn1.Construct.ISet numericSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_name_length);
		public static readonly Asn1.Construct.ISet printableSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_name_length);
		public static readonly Asn1.Construct.ISet surnameSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_surname_length);
		public static readonly Asn1.Construct.ISet given_nameSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_given_name_length);
		public static readonly Asn1.Construct.ISet initialsSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_initials_length);
		public static readonly Asn1.Construct.ISet generation_qualifierSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_generation_qualifier_length);
		public static readonly Asn1.Construct.ISet typeSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_defined_attribute_type_length);
		public static readonly Asn1.Construct.ISet @valueSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_defined_attribute_value_length);
		public static readonly Asn1.Construct.ISet extension_attribute_typeInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_extension_attributes);
		public static readonly System.Numerics.BigInteger common_name = 1;
		public static readonly System.Numerics.BigInteger teletex_common_name = 2;
		public static readonly System.Numerics.BigInteger teletex_organization_name = 3;
		public static readonly System.Numerics.BigInteger teletex_personal_name = 4;
		public static readonly Asn1.Construct.ISet surnameSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_surname_length);
		public static readonly Asn1.Construct.ISet given_nameSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_given_name_length);
		public static readonly Asn1.Construct.ISet initialsSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_initials_length);
		public static readonly Asn1.Construct.ISet generation_qualifierSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_generation_qualifier_length);
		public static readonly System.Numerics.BigInteger teletex_organizational_unit_names = 5;
		public static readonly System.Numerics.BigInteger pds_name = 7;
		public static readonly System.Numerics.BigInteger physical_delivery_country_name = 8;
		public static readonly Asn1.Construct.ISet x121_dcc_codeSize2 = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_country_name_numeric_length);
		public static readonly Asn1.Construct.ISet iso_3166_alpha2_codeSize2 = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_country_name_alpha_length);
		public static readonly System.Numerics.BigInteger postal_code = 9;
		public static readonly Asn1.Construct.ISet numeric_codeSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_postal_code_length);
		public static readonly Asn1.Construct.ISet printable_codeSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_postal_code_length);
		public static readonly System.Numerics.BigInteger physical_delivery_office_name = 10;
		public static readonly System.Numerics.BigInteger physical_delivery_office_number = 11;
		public static readonly System.Numerics.BigInteger extension_OR_address_components = 12;
		public static readonly System.Numerics.BigInteger physical_delivery_personal_name = 13;
		public static readonly System.Numerics.BigInteger physical_delivery_organization_name = 14;
		public static readonly System.Numerics.BigInteger extension_physical_delivery_address_components = 15;
		public static readonly System.Numerics.BigInteger unformatted_postal_address = 16;
		public static readonly Asn1.Construct.ISet printable_addressSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pds_parameter_length);
		public static readonly Asn1.Construct.ISet teletex_stringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_unformatted_address_length);
		public static readonly System.Numerics.BigInteger street_address = 17;
		public static readonly System.Numerics.BigInteger post_office_box_address = 18;
		public static readonly System.Numerics.BigInteger poste_restante_address = 19;
		public static readonly System.Numerics.BigInteger unique_postal_name = 20;
		public static readonly System.Numerics.BigInteger local_postal_attributes = 21;
		public static readonly Asn1.Construct.ISet printable_stringSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pds_parameter_length);
		public static readonly Asn1.Construct.ISet teletex_stringSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_pds_parameter_length);
		public static readonly System.Numerics.BigInteger extended_network_address = 22;
		public static readonly Asn1.Construct.ISet numberSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_e163_4_number_length);
		public static readonly Asn1.Construct.ISet sub_addressSize = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_e163_4_sub_address_length);
		public static readonly System.Numerics.BigInteger terminal_type = 23;
		public static readonly System.Numerics.BigInteger teletex_domain_defined_attributes = 6;
		public static readonly Asn1.Construct.ISet typeSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_defined_attribute_type_length);
		public static readonly Asn1.Construct.ISet @valueSize2 = new Asn1.Construct.Interval<System.Numerics.BigInteger>(1, Waher.Security.PKCS.Certificates.PKIX1Explicit88._DefinedValues.ub_domain_defined_attribute_value_length);
		public static readonly System.Numerics.BigInteger ub_name = 32768;
		public static readonly System.Numerics.BigInteger ub_common_name = 64;
		public static readonly System.Numerics.BigInteger ub_locality_name = 128;
		public static readonly System.Numerics.BigInteger ub_state_name = 128;
		public static readonly System.Numerics.BigInteger ub_organization_name = 64;
		public static readonly System.Numerics.BigInteger ub_organizational_unit_name = 64;
		public static readonly System.Numerics.BigInteger ub_title = 64;
		public static readonly System.Numerics.BigInteger ub_serial_number = 64;
		public static readonly System.Numerics.BigInteger ub_match = 128;
		public static readonly System.Numerics.BigInteger ub_emailaddress_length = 128;
		public static readonly System.Numerics.BigInteger ub_common_name_length = 64;
		public static readonly System.Numerics.BigInteger ub_country_name_alpha_length = 2;
		public static readonly System.Numerics.BigInteger ub_country_name_numeric_length = 3;
		public static readonly System.Numerics.BigInteger ub_domain_defined_attributes = 4;
		public static readonly System.Numerics.BigInteger ub_domain_defined_attribute_type_length = 8;
		public static readonly System.Numerics.BigInteger ub_domain_defined_attribute_value_length = 128;
		public static readonly System.Numerics.BigInteger ub_domain_name_length = 16;
		public static readonly System.Numerics.BigInteger ub_extension_attributes = 256;
		public static readonly System.Numerics.BigInteger ub_e163_4_number_length = 15;
		public static readonly System.Numerics.BigInteger ub_e163_4_sub_address_length = 40;
		public static readonly System.Numerics.BigInteger ub_generation_qualifier_length = 3;
		public static readonly System.Numerics.BigInteger ub_given_name_length = 16;
		public static readonly System.Numerics.BigInteger ub_initials_length = 5;
		public static readonly System.Numerics.BigInteger ub_integer_options = 256;
		public static readonly System.Numerics.BigInteger ub_numeric_user_id_length = 32;
		public static readonly System.Numerics.BigInteger ub_organization_name_length = 64;
		public static readonly System.Numerics.BigInteger ub_organizational_unit_name_length = 32;
		public static readonly System.Numerics.BigInteger ub_organizational_units = 4;
		public static readonly System.Numerics.BigInteger ub_pds_name_length = 16;
		public static readonly System.Numerics.BigInteger ub_pds_parameter_length = 30;
		public static readonly System.Numerics.BigInteger ub_pds_physical_address_lines = 6;
		public static readonly System.Numerics.BigInteger ub_postal_code_length = 16;
		public static readonly System.Numerics.BigInteger ub_pseudonym = 128;
		public static readonly System.Numerics.BigInteger ub_surname_length = 40;
		public static readonly System.Numerics.BigInteger ub_terminal_id_length = 24;
		public static readonly System.Numerics.BigInteger ub_unformatted_address_length = 180;
		public static readonly System.Numerics.BigInteger ub_x121_address_length = 16;
	}

	public partial class Attribute : Asn1.Construct
	{
		public Asn1.ObjectId type;
		public object[] values;

		public static Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute _Result = new Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadObjectIdExplicit(Reader, 0);
			this.values = ReadArray<object>(Reader, (Asn1F.AsnReader Reader, out object _Item) => TryReadAnyExplicit(Reader, 1, default, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Waher.Security.PKCS.Certificates.PKIX1Explicit88.Attribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdExplicit(Writer, 0, this.type);
				WriteArrayExplicit(Writer, 1, this.values);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Attribute _Typed)) return false;
			if (!AreEqual(this.type, _Typed.type)) return false;
			if (!AreEqual(this.values, _Typed.values)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.values);
			return _Result;
		}
	}

	public partial class AttributeTypeAndValue : Asn1.Construct
	{
		public Asn1.ObjectId type;
		public object @value;

		public static AttributeTypeAndValue Create(Asn1F.AsnReader Reader)
		{
			AttributeTypeAndValue _Result = new AttributeTypeAndValue();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadObjectIdExplicit(Reader, 0);
			this.@value = ReadAnyExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AttributeTypeAndValue _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AttributeTypeAndValue();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdExplicit(Writer, 0, this.type);
				WriteAnyExplicit(Writer, 1, this.@value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AttributeTypeAndValue _Typed)) return false;
			if (!AreEqual(this.type, _Typed.type)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

	public partial class X520name : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520name Create(Asn1F.AsnReader Reader)
		{
			X520name _Result = new X520name();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520name _Result)
		{
			try
			{
				_Result = new X520name();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520name _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520CommonName : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520CommonName Create(Asn1F.AsnReader Reader)
		{
			X520CommonName _Result = new X520CommonName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize2)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize2)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize2)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize2)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize2)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520CommonName _Result)
		{
			try
			{
				_Result = new X520CommonName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize2)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize2)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize2)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize2)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize2)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520CommonName _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520LocalityName : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520LocalityName Create(Asn1F.AsnReader Reader)
		{
			X520LocalityName _Result = new X520LocalityName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize3)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize3)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize3)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize3)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize3)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520LocalityName _Result)
		{
			try
			{
				_Result = new X520LocalityName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize3)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize3)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize3)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize3)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize3)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520LocalityName _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520StateOrProvinceName : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520StateOrProvinceName Create(Asn1F.AsnReader Reader)
		{
			X520StateOrProvinceName _Result = new X520StateOrProvinceName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize4)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize4)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize4)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize4)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize4)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520StateOrProvinceName _Result)
		{
			try
			{
				_Result = new X520StateOrProvinceName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize4)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize4)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize4)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize4)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize4)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520StateOrProvinceName _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520OrganizationName : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520OrganizationName Create(Asn1F.AsnReader Reader)
		{
			X520OrganizationName _Result = new X520OrganizationName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize5)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize5)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize5)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize5)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize5)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520OrganizationName _Result)
		{
			try
			{
				_Result = new X520OrganizationName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize5)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize5)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize5)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize5)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize5)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520OrganizationName _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520OrganizationalUnitName : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520OrganizationalUnitName Create(Asn1F.AsnReader Reader)
		{
			X520OrganizationalUnitName _Result = new X520OrganizationalUnitName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize6)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize6)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize6)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize6)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize6)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520OrganizationalUnitName _Result)
		{
			try
			{
				_Result = new X520OrganizationalUnitName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize6)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize6)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize6)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize6)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize6)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520OrganizationalUnitName _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520Title : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520Title Create(Asn1F.AsnReader Reader)
		{
			X520Title _Result = new X520Title();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize7)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize7)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize7)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize7)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize7)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520Title _Result)
		{
			try
			{
				_Result = new X520Title();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize7)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize7)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize7)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize7)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize7)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520Title _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class X520Pseudonym : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static X520Pseudonym Create(Asn1F.AsnReader Reader)
		{
			X520Pseudonym _Result = new X520Pseudonym();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize8)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize8)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize8)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize8)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize8)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out X520Pseudonym _Result)
		{
			try
			{
				_Result = new X520Pseudonym();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize8)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize8)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize8)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize8)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize8)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is X520Pseudonym _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class Name : Asn1.Construct
	{
		public enum _Options
		{
			rdnSequence
		}

		public _Options _choice;
		public AttributeTypeAndValue[][] rdnSequence;

		public static Name Create(Asn1F.AsnReader Reader)
		{
			Name _Result = new Name();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.rdnSequence = ReadArray<AttributeTypeAndValue[]>(Reader, (Asn1F.AsnReader Reader, out AttributeTypeAndValue[] _Item) => TryReadArray<AttributeTypeAndValue>(Reader, (Asn1F.AsnReader Reader, out AttributeTypeAndValue _Item) => AttributeTypeAndValue.TryCreate(Reader, out _Item), default, out _Item));
					this._choice = _Options.rdnSequence;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Name _Result)
		{
			try
			{
				_Result = new Name();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.rdnSequence:
					if (!(this.rdnSequence is null))
						WriteArrayExplicit(Writer, 0, this.rdnSequence);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Name _Typed)) return false;
			if (!AreEqual(this.rdnSequence, _Typed.rdnSequence)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.rdnSequence);
			return _Result;
		}
	}

	public partial class DirectoryString : Asn1.Construct
	{
		public enum _Options
		{
			teletexString,
			printableString,
			universalString,
			utf8String,
			bmpString
		}

		public _Options _choice;
		public string teletexString;
		public string printableString;
		public string universalString;
		public string utf8String;
		public string bmpString;

		public static DirectoryString Create(Asn1F.AsnReader Reader)
		{
			DirectoryString _Result = new DirectoryString();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.teletexString = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize9)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					this._choice = _Options.teletexString;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printableString = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize9)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					this._choice = _Options.printableString;
					break;

				case 130: // CONTEXT SPECIFIC [2]
					this.universalString = ReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.UniversalString);
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize9)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					this._choice = _Options.universalString;
					break;

				case 131: // CONTEXT SPECIFIC [3]
					this.utf8String = ReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.UTF8String);
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize9)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					this._choice = _Options.utf8String;
					break;

				case 132: // CONTEXT SPECIFIC [4]
					this.bmpString = ReadStringExplicit(Reader, 4, Asn1F.UniversalTagNumber.BMPString);
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize9)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					this._choice = _Options.bmpString;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out DirectoryString _Result)
		{
			try
			{
				_Result = new DirectoryString();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.teletexString:
					if (!(Asn1.Construct.CheckSize(this.teletexString, _DefinedValues.teletexStringSize9)))
						throw new Asn1F.AsnContentException("teletexString out of bounds.");
					if (!(this.teletexString is null))
						WriteStringExplicit(Writer, 0, this.teletexString, Asn1F.UniversalTagNumber.TeletexString);
					break;

				case _Options.printableString:
					if (!(Asn1.Construct.CheckSize(this.printableString, _DefinedValues.printableStringSize9)))
						throw new Asn1F.AsnContentException("printableString out of bounds.");
					if (!(this.printableString is null))
						WriteStringExplicit(Writer, 1, this.printableString, Asn1F.UniversalTagNumber.PrintableString);
					break;

				case _Options.universalString:
					if (!(Asn1.Construct.CheckSize(this.universalString, _DefinedValues.universalStringSize9)))
						throw new Asn1F.AsnContentException("universalString out of bounds.");
					if (!(this.universalString is null))
						WriteStringExplicit(Writer, 2, this.universalString, Asn1F.UniversalTagNumber.UniversalString);
					break;

				case _Options.utf8String:
					if (!(Asn1.Construct.CheckSize(this.utf8String, _DefinedValues.utf8StringSize9)))
						throw new Asn1F.AsnContentException("utf8String out of bounds.");
					if (!(this.utf8String is null))
						WriteStringExplicit(Writer, 3, this.utf8String, Asn1F.UniversalTagNumber.UTF8String);
					break;

				case _Options.bmpString:
					if (!(Asn1.Construct.CheckSize(this.bmpString, _DefinedValues.bmpStringSize9)))
						throw new Asn1F.AsnContentException("bmpString out of bounds.");
					if (!(this.bmpString is null))
						WriteStringExplicit(Writer, 4, this.bmpString, Asn1F.UniversalTagNumber.BMPString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is DirectoryString _Typed)) return false;
			if (!AreEqual(this.teletexString, _Typed.teletexString)) return false;
			if (!AreEqual(this.printableString, _Typed.printableString)) return false;
			if (!AreEqual(this.universalString, _Typed.universalString)) return false;
			if (!AreEqual(this.utf8String, _Typed.utf8String)) return false;
			if (!AreEqual(this.bmpString, _Typed.bmpString)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletexString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printableString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.universalString);
			_Result ^= _Result << 5 ^ CalcHashCode(this.utf8String);
			_Result ^= _Result << 5 ^ CalcHashCode(this.bmpString);
			return _Result;
		}
	}

	public partial class Certificate : Asn1.Construct
	{
		public TBSCertificate tbsCertificate;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signature;

		public static Certificate Create(Asn1F.AsnReader Reader)
		{
			Certificate _Result = new Certificate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.tbsCertificate = TBSCertificate.Create(Reader);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadBitStringExplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Certificate _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Certificate();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.tbsCertificate.Write(Writer);
				this.signatureAlgorithm.Write(Writer);
				WriteBitStringExplicit(Writer, 2, this.signature);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Certificate _Typed)) return false;
			if (!AreEqual(this.tbsCertificate, _Typed.tbsCertificate)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.tbsCertificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			return _Result;
		}
	}

	public partial class TBSCertificate : Asn1.Construct
	{
		public Version? version = Version.v1;
		public System.Numerics.BigInteger serialNumber;
		public AlgorithmIdentifier signature;
		public Name issuer;
		public Validity validity;
		public Name subject;
		public SubjectPublicKeyInfo subjectPublicKeyInfo;
		public byte[] issuerUniqueID;
		public byte[] subjectUniqueID;
		public Extension[] extensions;

		public static TBSCertificate Create(Asn1F.AsnReader Reader)
		{
			TBSCertificate _Result = new TBSCertificate();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadEnumerationExplicit<Version>(Reader, 0, this.version, out this.version);
			this.serialNumber = ReadIntegerExplicit(Reader, 1);
			this.signature = AlgorithmIdentifier.Create(Reader);
			this.issuer = Name.Create(Reader);
			this.validity = Validity.Create(Reader);
			this.subject = Name.Create(Reader);
			this.subjectPublicKeyInfo = SubjectPublicKeyInfo.Create(Reader);
			TryReadBitStringExplicit(Reader, 7, this.issuerUniqueID, out this.issuerUniqueID);
			TryReadBitStringExplicit(Reader, 8, this.subjectUniqueID, out this.subjectUniqueID);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.extensions, out this.extensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TBSCertificate _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TBSCertificate();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.version.HasValue)
					WriteIntegerExplicit(Writer, 0, (int)this.version.Value);
				WriteIntegerExplicit(Writer, 1, this.serialNumber);
				this.signature.Write(Writer);
				this.issuer.Write(Writer);
				this.validity.Write(Writer);
				this.subject.Write(Writer);
				this.subjectPublicKeyInfo.Write(Writer);
				if (!(this.issuerUniqueID is null))
					WriteBitStringExplicit(Writer, 7, this.issuerUniqueID);
				if (!(this.subjectUniqueID is null))
					WriteBitStringExplicit(Writer, 8, this.subjectUniqueID);
				if (!(this.extensions is null))
					WriteArrayExplicit(Writer, 9, this.extensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TBSCertificate _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.serialNumber, _Typed.serialNumber)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.validity, _Typed.validity)) return false;
			if (!AreEqual(this.subject, _Typed.subject)) return false;
			if (!AreEqual(this.subjectPublicKeyInfo, _Typed.subjectPublicKeyInfo)) return false;
			if (!AreEqual(this.issuerUniqueID, _Typed.issuerUniqueID)) return false;
			if (!AreEqual(this.subjectUniqueID, _Typed.subjectUniqueID)) return false;
			if (!AreEqual(this.extensions, _Typed.extensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.serialNumber);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.validity);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subject);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectPublicKeyInfo);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuerUniqueID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectUniqueID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extensions);
			return _Result;
		}
	}

	public enum Version
	{
		v1 = 0,
		v2 = 1,
		v3 = 2
	}

	public partial class Validity : Asn1.Construct
	{
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time notBefore;
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time notAfter;

		public static Validity Create(Asn1F.AsnReader Reader)
		{
			Validity _Result = new Validity();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.notBefore = Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.Create(Reader);
			this.notAfter = Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Validity _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Validity();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.notBefore.Write(Writer);
				this.notAfter.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Validity _Typed)) return false;
			if (!AreEqual(this.notBefore, _Typed.notBefore)) return false;
			if (!AreEqual(this.notAfter, _Typed.notAfter)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.notBefore);
			_Result ^= _Result << 5 ^ CalcHashCode(this.notAfter);
			return _Result;
		}
	}

	public partial class Time : Asn1.Construct
	{
		public enum _Options
		{
			utcTime,
			generalTime
		}

		public _Options _choice;
		public System.DateTimeOffset? utcTime;
		public System.TimeSpan? generalTime;

		public static Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time Create(Asn1F.AsnReader Reader)
		{
			Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time _Result = new Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.utcTime = ReadUtcTimeExplicit(Reader, 0);
					this._choice = _Options.utcTime;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.generalTime = ReadGeneralizedTimeExplicit(Reader, 1);
					this._choice = _Options.generalTime;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time _Result)
		{
			try
			{
				_Result = new Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.utcTime:
					if (this.utcTime.HasValue)
						WriteUtcTimeExplicit(Writer, 0, this.utcTime.Value);
					break;

				case _Options.generalTime:
					if (this.generalTime.HasValue)
						WriteGeneralizedTimeExplicit(Writer, 1, this.generalTime.Value);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Time _Typed)) return false;
			if (!AreEqual(this.utcTime, _Typed.utcTime)) return false;
			if (!AreEqual(this.generalTime, _Typed.generalTime)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.utcTime);
			_Result ^= _Result << 5 ^ CalcHashCode(this.generalTime);
			return _Result;
		}
	}

	public partial class SubjectPublicKeyInfo : Asn1.Construct
	{
		public AlgorithmIdentifier algorithm;
		public byte[] subjectPublicKey;

		public static SubjectPublicKeyInfo Create(Asn1F.AsnReader Reader)
		{
			SubjectPublicKeyInfo _Result = new SubjectPublicKeyInfo();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.algorithm = AlgorithmIdentifier.Create(Reader);
			this.subjectPublicKey = ReadBitStringExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out SubjectPublicKeyInfo _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new SubjectPublicKeyInfo();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.algorithm.Write(Writer);
				WriteBitStringExplicit(Writer, 1, this.subjectPublicKey);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is SubjectPublicKeyInfo _Typed)) return false;
			if (!AreEqual(this.algorithm, _Typed.algorithm)) return false;
			if (!AreEqual(this.subjectPublicKey, _Typed.subjectPublicKey)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.algorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.subjectPublicKey);
			return _Result;
		}
	}

	public partial class Extension : Asn1.Construct
	{
		public Asn1.ObjectId extnID;
		public bool? critical = false;
		public byte[] extnValue;

		public static Extension Create(Asn1F.AsnReader Reader)
		{
			Extension _Result = new Extension();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.extnID = ReadObjectIdExplicit(Reader, 0);
			TryReadBooleanExplicit(Reader, 1, this.critical, out this.critical);
			this.extnValue = ReadOctetStringExplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out Extension _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new Extension();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdExplicit(Writer, 0, this.extnID);
				if (this.critical.HasValue)
					WriteBooleanExplicit(Writer, 1, this.critical.Value);
				WriteOctetStringExplicit(Writer, 2, this.extnValue);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is Extension _Typed)) return false;
			if (!AreEqual(this.extnID, _Typed.extnID)) return false;
			if (!AreEqual(this.critical, _Typed.critical)) return false;
			if (!AreEqual(this.extnValue, _Typed.extnValue)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.extnID);
			_Result ^= _Result << 5 ^ CalcHashCode(this.critical);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extnValue);
			return _Result;
		}
	}

	public partial class CertificateList : Asn1.Construct
	{
		public TBSCertList tbsCertList;
		public AlgorithmIdentifier signatureAlgorithm;
		public byte[] signature;

		public static CertificateList Create(Asn1F.AsnReader Reader)
		{
			CertificateList _Result = new CertificateList();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.tbsCertList = TBSCertList.Create(Reader);
			this.signatureAlgorithm = AlgorithmIdentifier.Create(Reader);
			this.signature = ReadBitStringExplicit(Reader, 2);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out CertificateList _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new CertificateList();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.tbsCertList.Write(Writer);
				this.signatureAlgorithm.Write(Writer);
				WriteBitStringExplicit(Writer, 2, this.signature);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CertificateList _Typed)) return false;
			if (!AreEqual(this.tbsCertList, _Typed.tbsCertList)) return false;
			if (!AreEqual(this.signatureAlgorithm, _Typed.signatureAlgorithm)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.tbsCertList);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signatureAlgorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			return _Result;
		}
	}

	public partial class revokedCertificatesSeq : Asn1.Construct
	{
		public System.Numerics.BigInteger userCertificate;
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time revocationDate;
		public Extension[] crlEntryExtensions;

		public static revokedCertificatesSeq Create(Asn1F.AsnReader Reader)
		{
			revokedCertificatesSeq _Result = new revokedCertificatesSeq();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.userCertificate = ReadIntegerExplicit(Reader, 0);
			this.revocationDate = Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.Create(Reader);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.crlEntryExtensions, out this.crlEntryExtensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out revokedCertificatesSeq _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new revokedCertificatesSeq();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteIntegerExplicit(Writer, 0, this.userCertificate);
				this.revocationDate.Write(Writer);
				if (!(this.crlEntryExtensions is null))
					WriteArrayExplicit(Writer, 2, this.crlEntryExtensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is revokedCertificatesSeq _Typed)) return false;
			if (!AreEqual(this.userCertificate, _Typed.userCertificate)) return false;
			if (!AreEqual(this.revocationDate, _Typed.revocationDate)) return false;
			if (!AreEqual(this.crlEntryExtensions, _Typed.crlEntryExtensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.userCertificate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.revocationDate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.crlEntryExtensions);
			return _Result;
		}
	}

	public partial class TBSCertList : Asn1.Construct
	{
		public Version? version;
		public AlgorithmIdentifier signature;
		public Name issuer;
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time thisUpdate;
		public Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time nextUpdate;
		public revokedCertificatesSeq[] revokedCertificates;
		public Extension[] crlExtensions;

		public static TBSCertList Create(Asn1F.AsnReader Reader)
		{
			TBSCertList _Result = new TBSCertList();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadEnumerationExplicit<Version>(Reader, 0, this.version, out this.version);
			this.signature = AlgorithmIdentifier.Create(Reader);
			this.issuer = Name.Create(Reader);
			this.thisUpdate = Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.Create(Reader);
			Waher.Security.PKCS.Certificates.PKIX1Explicit88.Time.TryCreate(Reader, out this.nextUpdate);
			TryReadArray<revokedCertificatesSeq>(Reader, (Asn1F.AsnReader Reader, out revokedCertificatesSeq _Item) => revokedCertificatesSeq.TryCreate(Reader, out _Item), this.revokedCertificates, out this.revokedCertificates);
			TryReadArray<Extension>(Reader, (Asn1F.AsnReader Reader, out Extension _Item) => Extension.TryCreate(Reader, out _Item), this.crlExtensions, out this.crlExtensions);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TBSCertList _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TBSCertList();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (this.version.HasValue)
					WriteIntegerExplicit(Writer, 0, (int)this.version.Value);
				this.signature.Write(Writer);
				this.issuer.Write(Writer);
				this.thisUpdate.Write(Writer);
				if (!(this.nextUpdate is null))
					this.nextUpdate.Write(Writer);
				if (!(this.revokedCertificates is null))
					WriteArrayExplicit(Writer, 5, this.revokedCertificates);
				if (!(this.crlExtensions is null))
					WriteArrayExplicit(Writer, 6, this.crlExtensions);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TBSCertList _Typed)) return false;
			if (!AreEqual(this.version, _Typed.version)) return false;
			if (!AreEqual(this.signature, _Typed.signature)) return false;
			if (!AreEqual(this.issuer, _Typed.issuer)) return false;
			if (!AreEqual(this.thisUpdate, _Typed.thisUpdate)) return false;
			if (!AreEqual(this.nextUpdate, _Typed.nextUpdate)) return false;
			if (!AreEqual(this.revokedCertificates, _Typed.revokedCertificates)) return false;
			if (!AreEqual(this.crlExtensions, _Typed.crlExtensions)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.version);
			_Result ^= _Result << 5 ^ CalcHashCode(this.signature);
			_Result ^= _Result << 5 ^ CalcHashCode(this.issuer);
			_Result ^= _Result << 5 ^ CalcHashCode(this.thisUpdate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.nextUpdate);
			_Result ^= _Result << 5 ^ CalcHashCode(this.revokedCertificates);
			_Result ^= _Result << 5 ^ CalcHashCode(this.crlExtensions);
			return _Result;
		}
	}

	public partial class AlgorithmIdentifier : Asn1.Construct
	{
		public Asn1.ObjectId algorithm;
		public object parameters;

		public static AlgorithmIdentifier Create(Asn1F.AsnReader Reader)
		{
			AlgorithmIdentifier _Result = new AlgorithmIdentifier();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.algorithm = ReadObjectIdExplicit(Reader, 0);
			TryReadAnyExplicit(Reader, 1, this.parameters, out this.parameters);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AlgorithmIdentifier _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AlgorithmIdentifier();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteObjectIdExplicit(Writer, 0, this.algorithm);
				if (!(this.parameters is null))
					WriteAnyExplicit(Writer, 1, this.parameters);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AlgorithmIdentifier _Typed)) return false;
			if (!AreEqual(this.algorithm, _Typed.algorithm)) return false;
			if (!AreEqual(this.parameters, _Typed.parameters)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.algorithm);
			_Result ^= _Result << 5 ^ CalcHashCode(this.parameters);
			return _Result;
		}
	}

	public partial class ORAddress : Asn1.Construct
	{
		public BuiltInStandardAttributes built_in_standard_attributes;
		public BuiltInDomainDefinedAttribute[] built_in_domain_defined_attributes;
		public ExtensionAttribute[] extension_attributes;

		public static ORAddress Create(Asn1F.AsnReader Reader)
		{
			ORAddress _Result = new ORAddress();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.built_in_standard_attributes = BuiltInStandardAttributes.Create(Reader);
			TryReadArray<BuiltInDomainDefinedAttribute>(Reader, (Asn1F.AsnReader Reader, out BuiltInDomainDefinedAttribute _Item) => BuiltInDomainDefinedAttribute.TryCreate(Reader, out _Item), this.built_in_domain_defined_attributes, out this.built_in_domain_defined_attributes);
			TryReadArray<ExtensionAttribute>(Reader, (Asn1F.AsnReader Reader, out ExtensionAttribute _Item) => ExtensionAttribute.TryCreate(Reader, out _Item), this.extension_attributes, out this.extension_attributes);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out ORAddress _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new ORAddress();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				this.built_in_standard_attributes.Write(Writer);
				if (!(this.built_in_domain_defined_attributes is null))
					WriteArrayExplicit(Writer, 1, this.built_in_domain_defined_attributes);
				if (!(this.extension_attributes is null))
					WriteArrayExplicit(Writer, 2, this.extension_attributes);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ORAddress _Typed)) return false;
			if (!AreEqual(this.built_in_standard_attributes, _Typed.built_in_standard_attributes)) return false;
			if (!AreEqual(this.built_in_domain_defined_attributes, _Typed.built_in_domain_defined_attributes)) return false;
			if (!AreEqual(this.extension_attributes, _Typed.extension_attributes)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.built_in_standard_attributes);
			_Result ^= _Result << 5 ^ CalcHashCode(this.built_in_domain_defined_attributes);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extension_attributes);
			return _Result;
		}
	}

	public partial class BuiltInStandardAttributes : Asn1.Construct
	{
		public CountryName country_name;
		public AdministrationDomainName administration_domain_name;
		public string network_address;
		public string terminal_identifier;
		public PrivateDomainName private_domain_name;
		public string organization_name;
		public string numeric_user_identifier;
		public PersonalName personal_name;
		public string[] organizational_unit_names;

		public static BuiltInStandardAttributes Create(Asn1F.AsnReader Reader)
		{
			BuiltInStandardAttributes _Result = new BuiltInStandardAttributes();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			CountryName.TryCreate(Reader, out this.country_name);
			AdministrationDomainName.TryCreate(Reader, out this.administration_domain_name);
			TryReadStringExplicit(Reader, 2, Asn1F.UniversalTagNumber.NumericString, this.network_address, out this.network_address);
			if (!(this.network_address is null))
			{
				if (!(Asn1.Construct.CheckSize(this.network_address, _DefinedValues.network_addressSize)))
					throw new Asn1F.AsnContentException("network_address out of bounds.");
			}
			TryReadStringExplicit(Reader, 3, Asn1F.UniversalTagNumber.PrintableString, this.terminal_identifier, out this.terminal_identifier);
			if (!(this.terminal_identifier is null))
			{
				if (!(Asn1.Construct.CheckSize(this.terminal_identifier, _DefinedValues.terminal_identifierSize)))
					throw new Asn1F.AsnContentException("terminal_identifier out of bounds.");
			}
			PrivateDomainName.TryCreate(Reader, out this.private_domain_name);
			TryReadStringExplicit(Reader, 5, Asn1F.UniversalTagNumber.PrintableString, this.organization_name, out this.organization_name);
			if (!(this.organization_name is null))
			{
				if (!(Asn1.Construct.CheckSize(this.organization_name, _DefinedValues.organization_nameSize)))
					throw new Asn1F.AsnContentException("organization_name out of bounds.");
			}
			TryReadStringExplicit(Reader, 6, Asn1F.UniversalTagNumber.NumericString, this.numeric_user_identifier, out this.numeric_user_identifier);
			if (!(this.numeric_user_identifier is null))
			{
				if (!(Asn1.Construct.CheckSize(this.numeric_user_identifier, _DefinedValues.numeric_user_identifierSize)))
					throw new Asn1F.AsnContentException("numeric_user_identifier out of bounds.");
			}
			PersonalName.TryCreate(Reader, out this.personal_name);
			TryReadArray<string>(Reader, (Asn1F.AsnReader Reader, out string _Item) => TryReadStringExplicit(Reader, 8, Asn1F.UniversalTagNumber.PrintableString, default, out _Item), this.organizational_unit_names, out this.organizational_unit_names);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out BuiltInStandardAttributes _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new BuiltInStandardAttributes();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.country_name is null))
					this.country_name.Write(Writer);
				if (!(this.administration_domain_name is null))
					this.administration_domain_name.Write(Writer);
				if (!(this.network_address is null))
				{
					if (!(Asn1.Construct.CheckSize(this.network_address, _DefinedValues.network_addressSize)))
						throw new Asn1F.AsnContentException("network_address out of bounds.");
				}
				if (!(this.network_address is null))
					WriteStringExplicit(Writer, 2, this.network_address, Asn1F.UniversalTagNumber.NumericString);
				if (!(this.terminal_identifier is null))
				{
					if (!(Asn1.Construct.CheckSize(this.terminal_identifier, _DefinedValues.terminal_identifierSize)))
						throw new Asn1F.AsnContentException("terminal_identifier out of bounds.");
				}
				if (!(this.terminal_identifier is null))
					WriteStringExplicit(Writer, 3, this.terminal_identifier, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.private_domain_name is null))
					this.private_domain_name.Write(Writer);
				if (!(this.organization_name is null))
				{
					if (!(Asn1.Construct.CheckSize(this.organization_name, _DefinedValues.organization_nameSize)))
						throw new Asn1F.AsnContentException("organization_name out of bounds.");
				}
				if (!(this.organization_name is null))
					WriteStringExplicit(Writer, 5, this.organization_name, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.numeric_user_identifier is null))
				{
					if (!(Asn1.Construct.CheckSize(this.numeric_user_identifier, _DefinedValues.numeric_user_identifierSize)))
						throw new Asn1F.AsnContentException("numeric_user_identifier out of bounds.");
				}
				if (!(this.numeric_user_identifier is null))
					WriteStringExplicit(Writer, 6, this.numeric_user_identifier, Asn1F.UniversalTagNumber.NumericString);
				if (!(this.personal_name is null))
					this.personal_name.Write(Writer);
				if (!(this.organizational_unit_names is null))
					WriteArrayExplicit(Writer, 8, this.organizational_unit_names);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is BuiltInStandardAttributes _Typed)) return false;
			if (!AreEqual(this.country_name, _Typed.country_name)) return false;
			if (!AreEqual(this.administration_domain_name, _Typed.administration_domain_name)) return false;
			if (!AreEqual(this.network_address, _Typed.network_address)) return false;
			if (!AreEqual(this.terminal_identifier, _Typed.terminal_identifier)) return false;
			if (!AreEqual(this.private_domain_name, _Typed.private_domain_name)) return false;
			if (!AreEqual(this.organization_name, _Typed.organization_name)) return false;
			if (!AreEqual(this.numeric_user_identifier, _Typed.numeric_user_identifier)) return false;
			if (!AreEqual(this.personal_name, _Typed.personal_name)) return false;
			if (!AreEqual(this.organizational_unit_names, _Typed.organizational_unit_names)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.country_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.administration_domain_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.network_address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.terminal_identifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.private_domain_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.organization_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.numeric_user_identifier);
			_Result ^= _Result << 5 ^ CalcHashCode(this.personal_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.organizational_unit_names);
			return _Result;
		}
	}

	public partial class CountryName : Asn1.Construct
	{
		public enum _Options
		{
			x121_dcc_code,
			iso_3166_alpha2_code
		}

		public _Options _choice;
		public string x121_dcc_code;
		public string iso_3166_alpha2_code;

		public static CountryName Create(Asn1F.AsnReader Reader)
		{
			CountryName _Result = new CountryName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.x121_dcc_code = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
					if (!(Asn1.Construct.CheckSize(this.x121_dcc_code, _DefinedValues.x121_dcc_codeSize)))
						throw new Asn1F.AsnContentException("x121_dcc_code out of bounds.");
					this._choice = _Options.x121_dcc_code;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.iso_3166_alpha2_code = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.iso_3166_alpha2_code, _DefinedValues.iso_3166_alpha2_codeSize)))
						throw new Asn1F.AsnContentException("iso_3166_alpha2_code out of bounds.");
					this._choice = _Options.iso_3166_alpha2_code;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out CountryName _Result)
		{
			try
			{
				_Result = new CountryName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.x121_dcc_code:
					if (!(Asn1.Construct.CheckSize(this.x121_dcc_code, _DefinedValues.x121_dcc_codeSize)))
						throw new Asn1F.AsnContentException("x121_dcc_code out of bounds.");
					if (!(this.x121_dcc_code is null))
						WriteStringExplicit(Writer, 0, this.x121_dcc_code, Asn1F.UniversalTagNumber.NumericString);
					break;

				case _Options.iso_3166_alpha2_code:
					if (!(Asn1.Construct.CheckSize(this.iso_3166_alpha2_code, _DefinedValues.iso_3166_alpha2_codeSize)))
						throw new Asn1F.AsnContentException("iso_3166_alpha2_code out of bounds.");
					if (!(this.iso_3166_alpha2_code is null))
						WriteStringExplicit(Writer, 1, this.iso_3166_alpha2_code, Asn1F.UniversalTagNumber.PrintableString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is CountryName _Typed)) return false;
			if (!AreEqual(this.x121_dcc_code, _Typed.x121_dcc_code)) return false;
			if (!AreEqual(this.iso_3166_alpha2_code, _Typed.iso_3166_alpha2_code)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.x121_dcc_code);
			_Result ^= _Result << 5 ^ CalcHashCode(this.iso_3166_alpha2_code);
			return _Result;
		}
	}

	public partial class AdministrationDomainName : Asn1.Construct
	{
		public enum _Options
		{
			numeric,
			printable
		}

		public _Options _choice;
		public string numeric;
		public string printable;

		public static AdministrationDomainName Create(Asn1F.AsnReader Reader)
		{
			AdministrationDomainName _Result = new AdministrationDomainName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.numeric = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
					if (!(Asn1.Construct.CheckSize(this.numeric, _DefinedValues.numericSize)))
						throw new Asn1F.AsnContentException("numeric out of bounds.");
					this._choice = _Options.numeric;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printable = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printable, _DefinedValues.printableSize)))
						throw new Asn1F.AsnContentException("printable out of bounds.");
					this._choice = _Options.printable;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out AdministrationDomainName _Result)
		{
			try
			{
				_Result = new AdministrationDomainName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.numeric:
					if (!(Asn1.Construct.CheckSize(this.numeric, _DefinedValues.numericSize)))
						throw new Asn1F.AsnContentException("numeric out of bounds.");
					if (!(this.numeric is null))
						WriteStringExplicit(Writer, 0, this.numeric, Asn1F.UniversalTagNumber.NumericString);
					break;

				case _Options.printable:
					if (!(Asn1.Construct.CheckSize(this.printable, _DefinedValues.printableSize)))
						throw new Asn1F.AsnContentException("printable out of bounds.");
					if (!(this.printable is null))
						WriteStringExplicit(Writer, 1, this.printable, Asn1F.UniversalTagNumber.PrintableString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AdministrationDomainName _Typed)) return false;
			if (!AreEqual(this.numeric, _Typed.numeric)) return false;
			if (!AreEqual(this.printable, _Typed.printable)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.numeric);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printable);
			return _Result;
		}
	}

	public partial class PrivateDomainName : Asn1.Construct
	{
		public enum _Options
		{
			numeric,
			printable
		}

		public _Options _choice;
		public string numeric;
		public string printable;

		public static PrivateDomainName Create(Asn1F.AsnReader Reader)
		{
			PrivateDomainName _Result = new PrivateDomainName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.numeric = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
					if (!(Asn1.Construct.CheckSize(this.numeric, _DefinedValues.numericSize2)))
						throw new Asn1F.AsnContentException("numeric out of bounds.");
					this._choice = _Options.numeric;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printable = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printable, _DefinedValues.printableSize2)))
						throw new Asn1F.AsnContentException("printable out of bounds.");
					this._choice = _Options.printable;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out PrivateDomainName _Result)
		{
			try
			{
				_Result = new PrivateDomainName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.numeric:
					if (!(Asn1.Construct.CheckSize(this.numeric, _DefinedValues.numericSize2)))
						throw new Asn1F.AsnContentException("numeric out of bounds.");
					if (!(this.numeric is null))
						WriteStringExplicit(Writer, 0, this.numeric, Asn1F.UniversalTagNumber.NumericString);
					break;

				case _Options.printable:
					if (!(Asn1.Construct.CheckSize(this.printable, _DefinedValues.printableSize2)))
						throw new Asn1F.AsnContentException("printable out of bounds.");
					if (!(this.printable is null))
						WriteStringExplicit(Writer, 1, this.printable, Asn1F.UniversalTagNumber.PrintableString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PrivateDomainName _Typed)) return false;
			if (!AreEqual(this.numeric, _Typed.numeric)) return false;
			if (!AreEqual(this.printable, _Typed.printable)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.numeric);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printable);
			return _Result;
		}
	}

	public partial class PersonalName : Asn1.Construct
	{
		public string surname;
		public string given_name;
		public string initials;
		public string generation_qualifier;

		public static PersonalName Create(Asn1F.AsnReader Reader)
		{
			PersonalName _Result = new PersonalName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.surname = ReadStringImplicit(Reader, 0, Asn1F.UniversalTagNumber.PrintableString);
			if (!(Asn1.Construct.CheckSize(this.surname, _DefinedValues.surnameSize)))
				throw new Asn1F.AsnContentException("surname out of bounds.");
			TryReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString, this.given_name, out this.given_name);
			if (!(this.given_name is null))
			{
				if (!(Asn1.Construct.CheckSize(this.given_name, _DefinedValues.given_nameSize)))
					throw new Asn1F.AsnContentException("given_name out of bounds.");
			}
			TryReadStringImplicit(Reader, 2, Asn1F.UniversalTagNumber.PrintableString, this.initials, out this.initials);
			if (!(this.initials is null))
			{
				if (!(Asn1.Construct.CheckSize(this.initials, _DefinedValues.initialsSize)))
					throw new Asn1F.AsnContentException("initials out of bounds.");
			}
			TryReadStringImplicit(Reader, 3, Asn1F.UniversalTagNumber.PrintableString, this.generation_qualifier, out this.generation_qualifier);
			if (!(this.generation_qualifier is null))
			{
				if (!(Asn1.Construct.CheckSize(this.generation_qualifier, _DefinedValues.generation_qualifierSize)))
					throw new Asn1F.AsnContentException("generation_qualifier out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PersonalName _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PersonalName();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSetOf())
			{
				if (!(Asn1.Construct.CheckSize(this.surname, _DefinedValues.surnameSize)))
					throw new Asn1F.AsnContentException("surname out of bounds.");
				WriteStringExplicit(Writer, 0, this.surname, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.given_name is null))
				{
					if (!(Asn1.Construct.CheckSize(this.given_name, _DefinedValues.given_nameSize)))
						throw new Asn1F.AsnContentException("given_name out of bounds.");
				}
				if (!(this.given_name is null))
					WriteStringExplicit(Writer, 1, this.given_name, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.initials is null))
				{
					if (!(Asn1.Construct.CheckSize(this.initials, _DefinedValues.initialsSize)))
						throw new Asn1F.AsnContentException("initials out of bounds.");
				}
				if (!(this.initials is null))
					WriteStringExplicit(Writer, 2, this.initials, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.generation_qualifier is null))
				{
					if (!(Asn1.Construct.CheckSize(this.generation_qualifier, _DefinedValues.generation_qualifierSize)))
						throw new Asn1F.AsnContentException("generation_qualifier out of bounds.");
				}
				if (!(this.generation_qualifier is null))
					WriteStringExplicit(Writer, 3, this.generation_qualifier, Asn1F.UniversalTagNumber.PrintableString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PersonalName _Typed)) return false;
			if (!AreEqual(this.surname, _Typed.surname)) return false;
			if (!AreEqual(this.given_name, _Typed.given_name)) return false;
			if (!AreEqual(this.initials, _Typed.initials)) return false;
			if (!AreEqual(this.generation_qualifier, _Typed.generation_qualifier)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.surname);
			_Result ^= _Result << 5 ^ CalcHashCode(this.given_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.initials);
			_Result ^= _Result << 5 ^ CalcHashCode(this.generation_qualifier);
			return _Result;
		}
	}

	public partial class BuiltInDomainDefinedAttribute : Asn1.Construct
	{
		public string type;
		public string @value;

		public static BuiltInDomainDefinedAttribute Create(Asn1F.AsnReader Reader)
		{
			BuiltInDomainDefinedAttribute _Result = new BuiltInDomainDefinedAttribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.PrintableString);
			if (!(Asn1.Construct.CheckSize(this.type, _DefinedValues.typeSize)))
				throw new Asn1F.AsnContentException("type out of bounds.");
			this.@value = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
			if (!(Asn1.Construct.CheckSize(this.@value, _DefinedValues.@valueSize)))
				throw new Asn1F.AsnContentException("@value out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out BuiltInDomainDefinedAttribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new BuiltInDomainDefinedAttribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.type, _DefinedValues.typeSize)))
					throw new Asn1F.AsnContentException("type out of bounds.");
				WriteStringExplicit(Writer, 0, this.type, Asn1F.UniversalTagNumber.PrintableString);
				if (!(Asn1.Construct.CheckSize(this.@value, _DefinedValues.@valueSize)))
					throw new Asn1F.AsnContentException("@value out of bounds.");
				WriteStringExplicit(Writer, 1, this.@value, Asn1F.UniversalTagNumber.PrintableString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is BuiltInDomainDefinedAttribute _Typed)) return false;
			if (!AreEqual(this.type, _Typed.type)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

	public partial class ExtensionAttribute : Asn1.Construct
	{
		public System.Numerics.BigInteger extension_attribute_type;
		public object extension_attribute_value;

		public static ExtensionAttribute Create(Asn1F.AsnReader Reader)
		{
			ExtensionAttribute _Result = new ExtensionAttribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.extension_attribute_type = ReadIntegerImplicit(Reader, 0);
			if (!(Asn1.Construct.CheckInSet(this.extension_attribute_type, _DefinedValues.extension_attribute_typeInSet)))
				throw new Asn1F.AsnContentException("extension_attribute_type out of bounds.");
			this.extension_attribute_value = ReadAnyExplicit(Reader, 1);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out ExtensionAttribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new ExtensionAttribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckInSet(this.extension_attribute_type, _DefinedValues.extension_attribute_typeInSet)))
					throw new Asn1F.AsnContentException("extension_attribute_type out of bounds.");
				WriteIntegerExplicit(Writer, 0, this.extension_attribute_type);
				WriteAnyExplicit(Writer, 1, this.extension_attribute_value);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ExtensionAttribute _Typed)) return false;
			if (!AreEqual(this.extension_attribute_type, _Typed.extension_attribute_type)) return false;
			if (!AreEqual(this.extension_attribute_value, _Typed.extension_attribute_value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.extension_attribute_type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.extension_attribute_value);
			return _Result;
		}
	}

	public partial class TeletexPersonalName : Asn1.Construct
	{
		public string surname;
		public string given_name;
		public string initials;
		public string generation_qualifier;

		public static TeletexPersonalName Create(Asn1F.AsnReader Reader)
		{
			TeletexPersonalName _Result = new TeletexPersonalName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.surname = ReadStringImplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
			if (!(Asn1.Construct.CheckSize(this.surname, _DefinedValues.surnameSize2)))
				throw new Asn1F.AsnContentException("surname out of bounds.");
			TryReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.TeletexString, this.given_name, out this.given_name);
			if (!(this.given_name is null))
			{
				if (!(Asn1.Construct.CheckSize(this.given_name, _DefinedValues.given_nameSize2)))
					throw new Asn1F.AsnContentException("given_name out of bounds.");
			}
			TryReadStringImplicit(Reader, 2, Asn1F.UniversalTagNumber.TeletexString, this.initials, out this.initials);
			if (!(this.initials is null))
			{
				if (!(Asn1.Construct.CheckSize(this.initials, _DefinedValues.initialsSize2)))
					throw new Asn1F.AsnContentException("initials out of bounds.");
			}
			TryReadStringImplicit(Reader, 3, Asn1F.UniversalTagNumber.TeletexString, this.generation_qualifier, out this.generation_qualifier);
			if (!(this.generation_qualifier is null))
			{
				if (!(Asn1.Construct.CheckSize(this.generation_qualifier, _DefinedValues.generation_qualifierSize2)))
					throw new Asn1F.AsnContentException("generation_qualifier out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TeletexPersonalName _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TeletexPersonalName();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSetOf())
			{
				if (!(Asn1.Construct.CheckSize(this.surname, _DefinedValues.surnameSize2)))
					throw new Asn1F.AsnContentException("surname out of bounds.");
				WriteStringExplicit(Writer, 0, this.surname, Asn1F.UniversalTagNumber.TeletexString);
				if (!(this.given_name is null))
				{
					if (!(Asn1.Construct.CheckSize(this.given_name, _DefinedValues.given_nameSize2)))
						throw new Asn1F.AsnContentException("given_name out of bounds.");
				}
				if (!(this.given_name is null))
					WriteStringExplicit(Writer, 1, this.given_name, Asn1F.UniversalTagNumber.TeletexString);
				if (!(this.initials is null))
				{
					if (!(Asn1.Construct.CheckSize(this.initials, _DefinedValues.initialsSize2)))
						throw new Asn1F.AsnContentException("initials out of bounds.");
				}
				if (!(this.initials is null))
					WriteStringExplicit(Writer, 2, this.initials, Asn1F.UniversalTagNumber.TeletexString);
				if (!(this.generation_qualifier is null))
				{
					if (!(Asn1.Construct.CheckSize(this.generation_qualifier, _DefinedValues.generation_qualifierSize2)))
						throw new Asn1F.AsnContentException("generation_qualifier out of bounds.");
				}
				if (!(this.generation_qualifier is null))
					WriteStringExplicit(Writer, 3, this.generation_qualifier, Asn1F.UniversalTagNumber.TeletexString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TeletexPersonalName _Typed)) return false;
			if (!AreEqual(this.surname, _Typed.surname)) return false;
			if (!AreEqual(this.given_name, _Typed.given_name)) return false;
			if (!AreEqual(this.initials, _Typed.initials)) return false;
			if (!AreEqual(this.generation_qualifier, _Typed.generation_qualifier)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.surname);
			_Result ^= _Result << 5 ^ CalcHashCode(this.given_name);
			_Result ^= _Result << 5 ^ CalcHashCode(this.initials);
			_Result ^= _Result << 5 ^ CalcHashCode(this.generation_qualifier);
			return _Result;
		}
	}

	public partial class PhysicalDeliveryCountryName : Asn1.Construct
	{
		public enum _Options
		{
			x121_dcc_code,
			iso_3166_alpha2_code
		}

		public _Options _choice;
		public string x121_dcc_code;
		public string iso_3166_alpha2_code;

		public static PhysicalDeliveryCountryName Create(Asn1F.AsnReader Reader)
		{
			PhysicalDeliveryCountryName _Result = new PhysicalDeliveryCountryName();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.x121_dcc_code = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
					if (!(Asn1.Construct.CheckSize(this.x121_dcc_code, _DefinedValues.x121_dcc_codeSize2)))
						throw new Asn1F.AsnContentException("x121_dcc_code out of bounds.");
					this._choice = _Options.x121_dcc_code;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.iso_3166_alpha2_code = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.iso_3166_alpha2_code, _DefinedValues.iso_3166_alpha2_codeSize2)))
						throw new Asn1F.AsnContentException("iso_3166_alpha2_code out of bounds.");
					this._choice = _Options.iso_3166_alpha2_code;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out PhysicalDeliveryCountryName _Result)
		{
			try
			{
				_Result = new PhysicalDeliveryCountryName();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.x121_dcc_code:
					if (!(Asn1.Construct.CheckSize(this.x121_dcc_code, _DefinedValues.x121_dcc_codeSize2)))
						throw new Asn1F.AsnContentException("x121_dcc_code out of bounds.");
					if (!(this.x121_dcc_code is null))
						WriteStringExplicit(Writer, 0, this.x121_dcc_code, Asn1F.UniversalTagNumber.NumericString);
					break;

				case _Options.iso_3166_alpha2_code:
					if (!(Asn1.Construct.CheckSize(this.iso_3166_alpha2_code, _DefinedValues.iso_3166_alpha2_codeSize2)))
						throw new Asn1F.AsnContentException("iso_3166_alpha2_code out of bounds.");
					if (!(this.iso_3166_alpha2_code is null))
						WriteStringExplicit(Writer, 1, this.iso_3166_alpha2_code, Asn1F.UniversalTagNumber.PrintableString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PhysicalDeliveryCountryName _Typed)) return false;
			if (!AreEqual(this.x121_dcc_code, _Typed.x121_dcc_code)) return false;
			if (!AreEqual(this.iso_3166_alpha2_code, _Typed.iso_3166_alpha2_code)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.x121_dcc_code);
			_Result ^= _Result << 5 ^ CalcHashCode(this.iso_3166_alpha2_code);
			return _Result;
		}
	}

	public partial class PostalCode : Asn1.Construct
	{
		public enum _Options
		{
			numeric_code,
			printable_code
		}

		public _Options _choice;
		public string numeric_code;
		public string printable_code;

		public static PostalCode Create(Asn1F.AsnReader Reader)
		{
			PostalCode _Result = new PostalCode();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.numeric_code = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
					if (!(Asn1.Construct.CheckSize(this.numeric_code, _DefinedValues.numeric_codeSize)))
						throw new Asn1F.AsnContentException("numeric_code out of bounds.");
					this._choice = _Options.numeric_code;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.printable_code = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.PrintableString);
					if (!(Asn1.Construct.CheckSize(this.printable_code, _DefinedValues.printable_codeSize)))
						throw new Asn1F.AsnContentException("printable_code out of bounds.");
					this._choice = _Options.printable_code;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out PostalCode _Result)
		{
			try
			{
				_Result = new PostalCode();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.numeric_code:
					if (!(Asn1.Construct.CheckSize(this.numeric_code, _DefinedValues.numeric_codeSize)))
						throw new Asn1F.AsnContentException("numeric_code out of bounds.");
					if (!(this.numeric_code is null))
						WriteStringExplicit(Writer, 0, this.numeric_code, Asn1F.UniversalTagNumber.NumericString);
					break;

				case _Options.printable_code:
					if (!(Asn1.Construct.CheckSize(this.printable_code, _DefinedValues.printable_codeSize)))
						throw new Asn1F.AsnContentException("printable_code out of bounds.");
					if (!(this.printable_code is null))
						WriteStringExplicit(Writer, 1, this.printable_code, Asn1F.UniversalTagNumber.PrintableString);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PostalCode _Typed)) return false;
			if (!AreEqual(this.numeric_code, _Typed.numeric_code)) return false;
			if (!AreEqual(this.printable_code, _Typed.printable_code)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.numeric_code);
			_Result ^= _Result << 5 ^ CalcHashCode(this.printable_code);
			return _Result;
		}
	}

	public partial class UnformattedPostalAddress : Asn1.Construct
	{
		public string[] printable_address;
		public string teletex_string;

		public static UnformattedPostalAddress Create(Asn1F.AsnReader Reader)
		{
			UnformattedPostalAddress _Result = new UnformattedPostalAddress();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadArray<string>(Reader, (Asn1F.AsnReader Reader, out string _Item) => TryReadString(Reader, Asn1F.UniversalTagNumber.PrintableString, default, out _Item), this.printable_address, out this.printable_address);
			if (!(this.printable_address is null))
			{
				if (!(Asn1.Construct.CheckSize(this.printable_address, _DefinedValues.printable_addressSize)))
					throw new Asn1F.AsnContentException("printable_address out of bounds.");
			}
			TryReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.TeletexString, this.teletex_string, out this.teletex_string);
			if (!(this.teletex_string is null))
			{
				if (!(Asn1.Construct.CheckSize(this.teletex_string, _DefinedValues.teletex_stringSize)))
					throw new Asn1F.AsnContentException("teletex_string out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out UnformattedPostalAddress _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new UnformattedPostalAddress();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSetOf())
			{
				if (!(this.printable_address is null))
				{
					if (!(Asn1.Construct.CheckSize(this.printable_address, _DefinedValues.printable_addressSize)))
						throw new Asn1F.AsnContentException("printable_address out of bounds.");
				}
				if (!(this.printable_address is null))
					WriteArrayExplicit(Writer, 0, this.printable_address);
				if (!(this.teletex_string is null))
				{
					if (!(Asn1.Construct.CheckSize(this.teletex_string, _DefinedValues.teletex_stringSize)))
						throw new Asn1F.AsnContentException("teletex_string out of bounds.");
				}
				if (!(this.teletex_string is null))
					WriteStringExplicit(Writer, 1, this.teletex_string, Asn1F.UniversalTagNumber.TeletexString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is UnformattedPostalAddress _Typed)) return false;
			if (!AreEqual(this.printable_address, _Typed.printable_address)) return false;
			if (!AreEqual(this.teletex_string, _Typed.teletex_string)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.printable_address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletex_string);
			return _Result;
		}
	}

	public partial class PDSParameter : Asn1.Construct
	{
		public string printable_string;
		public string teletex_string;

		public static PDSParameter Create(Asn1F.AsnReader Reader)
		{
			PDSParameter _Result = new PDSParameter();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.PrintableString, this.printable_string, out this.printable_string);
			if (!(this.printable_string is null))
			{
				if (!(Asn1.Construct.CheckSize(this.printable_string, _DefinedValues.printable_stringSize)))
					throw new Asn1F.AsnContentException("printable_string out of bounds.");
			}
			TryReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.TeletexString, this.teletex_string, out this.teletex_string);
			if (!(this.teletex_string is null))
			{
				if (!(Asn1.Construct.CheckSize(this.teletex_string, _DefinedValues.teletex_stringSize2)))
					throw new Asn1F.AsnContentException("teletex_string out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PDSParameter _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PDSParameter();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSetOf())
			{
				if (!(this.printable_string is null))
				{
					if (!(Asn1.Construct.CheckSize(this.printable_string, _DefinedValues.printable_stringSize)))
						throw new Asn1F.AsnContentException("printable_string out of bounds.");
				}
				if (!(this.printable_string is null))
					WriteStringExplicit(Writer, 0, this.printable_string, Asn1F.UniversalTagNumber.PrintableString);
				if (!(this.teletex_string is null))
				{
					if (!(Asn1.Construct.CheckSize(this.teletex_string, _DefinedValues.teletex_stringSize2)))
						throw new Asn1F.AsnContentException("teletex_string out of bounds.");
				}
				if (!(this.teletex_string is null))
					WriteStringExplicit(Writer, 1, this.teletex_string, Asn1F.UniversalTagNumber.TeletexString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PDSParameter _Typed)) return false;
			if (!AreEqual(this.printable_string, _Typed.printable_string)) return false;
			if (!AreEqual(this.teletex_string, _Typed.teletex_string)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.printable_string);
			_Result ^= _Result << 5 ^ CalcHashCode(this.teletex_string);
			return _Result;
		}
	}

	public partial class e163_4_addressSeq : Asn1.Construct
	{
		public string number;
		public string sub_address;

		public static e163_4_addressSeq Create(Asn1F.AsnReader Reader)
		{
			e163_4_addressSeq _Result = new e163_4_addressSeq();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.number = ReadStringImplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
			if (!(Asn1.Construct.CheckSize(this.number, _DefinedValues.numberSize)))
				throw new Asn1F.AsnContentException("number out of bounds.");
			TryReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.NumericString, this.sub_address, out this.sub_address);
			if (!(this.sub_address is null))
			{
				if (!(Asn1.Construct.CheckSize(this.sub_address, _DefinedValues.sub_addressSize)))
					throw new Asn1F.AsnContentException("sub_address out of bounds.");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out e163_4_addressSeq _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new e163_4_addressSeq();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.number, _DefinedValues.numberSize)))
					throw new Asn1F.AsnContentException("number out of bounds.");
				WriteStringExplicit(Writer, 0, this.number, Asn1F.UniversalTagNumber.NumericString);
				if (!(this.sub_address is null))
				{
					if (!(Asn1.Construct.CheckSize(this.sub_address, _DefinedValues.sub_addressSize)))
						throw new Asn1F.AsnContentException("sub_address out of bounds.");
				}
				if (!(this.sub_address is null))
					WriteStringExplicit(Writer, 1, this.sub_address, Asn1F.UniversalTagNumber.NumericString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is e163_4_addressSeq _Typed)) return false;
			if (!AreEqual(this.number, _Typed.number)) return false;
			if (!AreEqual(this.sub_address, _Typed.sub_address)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.number);
			_Result ^= _Result << 5 ^ CalcHashCode(this.sub_address);
			return _Result;
		}
	}

	public partial class ExtendedNetworkAddress : Asn1.Construct
	{
		public partial class e163_4_addressSeq : Asn1.Construct
		{
			public string number;
			public string sub_address;

			public static e163_4_addressSeq Create(Asn1F.AsnReader Reader)
			{
				e163_4_addressSeq _Result = new e163_4_addressSeq();
				_Result.Read(Reader);
				return _Result;
			}

			public override void Read(Asn1F.AsnReader Reader0)
			{
				Asn1F.AsnReader Reader = Reader0.ReadSequence();
				this.number = ReadStringImplicit(Reader, 0, Asn1F.UniversalTagNumber.NumericString);
				if (!(Asn1.Construct.CheckSize(this.number, _DefinedValues.numberSize)))
					throw new Asn1F.AsnContentException("number out of bounds.");
				TryReadStringImplicit(Reader, 1, Asn1F.UniversalTagNumber.NumericString, this.sub_address, out this.sub_address);
				if (!(this.sub_address is null))
				{
					if (!(Asn1.Construct.CheckSize(this.sub_address, _DefinedValues.sub_addressSize)))
						throw new Asn1F.AsnContentException("sub_address out of bounds.");
				}
			}

			public static bool TryCreate(Asn1F.AsnReader Reader0, out e163_4_addressSeq _Result)
			{
				Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
				if (Reader is null)
				{
					_Result = null;
					return false;
				}

				_Result = new e163_4_addressSeq();
				_Result.Read(Reader);
				return true;
			}

			public override void Write(Asn1F.AsnWriter Writer)
			{
				using (Writer.PushSequence())
				{
					if (!(Asn1.Construct.CheckSize(this.number, _DefinedValues.numberSize)))
						throw new Asn1F.AsnContentException("number out of bounds.");
					WriteStringExplicit(Writer, 0, this.number, Asn1F.UniversalTagNumber.NumericString);
					if (!(this.sub_address is null))
					{
						if (!(Asn1.Construct.CheckSize(this.sub_address, _DefinedValues.sub_addressSize)))
							throw new Asn1F.AsnContentException("sub_address out of bounds.");
					}
					if (!(this.sub_address is null))
						WriteStringExplicit(Writer, 1, this.sub_address, Asn1F.UniversalTagNumber.NumericString);
				}
			}

			public override bool Equals(object Object)
			{
				if (!(Object is e163_4_addressSeq _Typed)) return false;
				if (!AreEqual(this.number, _Typed.number)) return false;
				if (!AreEqual(this.sub_address, _Typed.sub_address)) return false;
				return true;
			}

			public override int GetHashCode()
			{
				int _Result = 0;
				_Result ^= _Result << 5 ^ CalcHashCode(this.number);
				_Result ^= _Result << 5 ^ CalcHashCode(this.sub_address);
				return _Result;
			}
		}

		public enum _Options
		{
			e163_4_address,
			psap_address
		}

		public _Options _choice;
		public e163_4_addressSeq e163_4_address;
		public PresentationAddress psap_address;

		public static ExtendedNetworkAddress Create(Asn1F.AsnReader Reader)
		{
			ExtendedNetworkAddress _Result = new ExtendedNetworkAddress();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader)
		{
			Asn1F.Asn1Tag Tag = Reader.PeekTag();

			switch (ToInt32(Tag))
			{
				case 128: // CONTEXT SPECIFIC [0]
					this.e163_4_address = e163_4_addressSeq.Create(Reader);
					this._choice = _Options.e163_4_address;
					break;

				case 129: // CONTEXT SPECIFIC [1]
					this.psap_address = PresentationAddress.Create(Reader);
					this._choice = _Options.psap_address;
					break;

				default:
					throw new Asn1F.AsnContentException("Unexpected tag: " + Tag.TagClass.ToString() + ", " + Tag.TagValue.ToString() + ".");
			}
		}

		public static bool TryCreate(Asn1F.AsnReader Reader, out ExtendedNetworkAddress _Result)
		{
			try
			{
				_Result = new ExtendedNetworkAddress();
				Asn1F.AsnReader Reader2 = Reader.Clone();
				_Result.Read(Reader2);
				_Result.Read(Reader);
				return true;
			}
			catch (System.Exception)
			{
				_Result = null;
				return false;
			}
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			switch (this._choice)
			{
				case _Options.e163_4_address:
					if (!(this.e163_4_address is null))
						this.e163_4_address.Write(Writer);
					break;

				case _Options.psap_address:
					if (!(this.psap_address is null))
						this.psap_address.Write(Writer);
					break;

				default:
					throw new Asn1F.AsnContentException("No option selected.");
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is ExtendedNetworkAddress _Typed)) return false;
			if (!AreEqual(this.e163_4_address, _Typed.e163_4_address)) return false;
			if (!AreEqual(this.psap_address, _Typed.psap_address)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.e163_4_address);
			_Result ^= _Result << 5 ^ CalcHashCode(this.psap_address);
			return _Result;
		}
	}

	public partial class PresentationAddress : Asn1.Construct
	{
		public byte[] pSelector;
		public byte[] sSelector;
		public byte[] tSelector;
		public byte[][] nAddresses;

		public static PresentationAddress Create(Asn1F.AsnReader Reader)
		{
			PresentationAddress _Result = new PresentationAddress();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			TryReadOctetStringExplicit(Reader, 0, this.pSelector, out this.pSelector);
			TryReadOctetStringExplicit(Reader, 1, this.sSelector, out this.sSelector);
			TryReadOctetStringExplicit(Reader, 2, this.tSelector, out this.tSelector);
			this.nAddresses = ReadArray<byte[]>(Reader, (Asn1F.AsnReader Reader, out byte[] _Item) => TryReadOctetString(Reader, default, out _Item));
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out PresentationAddress _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new PresentationAddress();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(this.pSelector is null))
					WriteOctetStringExplicit(Writer, 0, this.pSelector);
				if (!(this.sSelector is null))
					WriteOctetStringExplicit(Writer, 1, this.sSelector);
				if (!(this.tSelector is null))
					WriteOctetStringExplicit(Writer, 2, this.tSelector);
				WriteArrayExplicit(Writer, 3, this.nAddresses);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is PresentationAddress _Typed)) return false;
			if (!AreEqual(this.pSelector, _Typed.pSelector)) return false;
			if (!AreEqual(this.sSelector, _Typed.sSelector)) return false;
			if (!AreEqual(this.tSelector, _Typed.tSelector)) return false;
			if (!AreEqual(this.nAddresses, _Typed.nAddresses)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.pSelector);
			_Result ^= _Result << 5 ^ CalcHashCode(this.sSelector);
			_Result ^= _Result << 5 ^ CalcHashCode(this.tSelector);
			_Result ^= _Result << 5 ^ CalcHashCode(this.nAddresses);
			return _Result;
		}
	}

	public enum TerminalType
	{
		telex = 3,
		teletex = 4,
		g3_facsimile = 5,
		g4_facsimile = 6,
		ia5_terminal = 7,
		videotex = 8
	}

	public partial class TeletexDomainDefinedAttribute : Asn1.Construct
	{
		public string type;
		public string @value;

		public static TeletexDomainDefinedAttribute Create(Asn1F.AsnReader Reader)
		{
			TeletexDomainDefinedAttribute _Result = new TeletexDomainDefinedAttribute();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.type = ReadStringExplicit(Reader, 0, Asn1F.UniversalTagNumber.TeletexString);
			if (!(Asn1.Construct.CheckSize(this.type, _DefinedValues.typeSize2)))
				throw new Asn1F.AsnContentException("type out of bounds.");
			this.@value = ReadStringExplicit(Reader, 1, Asn1F.UniversalTagNumber.TeletexString);
			if (!(Asn1.Construct.CheckSize(this.@value, _DefinedValues.@valueSize2)))
				throw new Asn1F.AsnContentException("@value out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TeletexDomainDefinedAttribute _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TeletexDomainDefinedAttribute();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.type, _DefinedValues.typeSize2)))
					throw new Asn1F.AsnContentException("type out of bounds.");
				WriteStringExplicit(Writer, 0, this.type, Asn1F.UniversalTagNumber.TeletexString);
				if (!(Asn1.Construct.CheckSize(this.@value, _DefinedValues.@valueSize2)))
					throw new Asn1F.AsnContentException("@value out of bounds.");
				WriteStringExplicit(Writer, 1, this.@value, Asn1F.UniversalTagNumber.TeletexString);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TeletexDomainDefinedAttribute _Typed)) return false;
			if (!AreEqual(this.type, _Typed.type)) return false;
			if (!AreEqual(this.@value, _Typed.@value)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.type);
			_Result ^= _Result << 5 ^ CalcHashCode(this.@value);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
