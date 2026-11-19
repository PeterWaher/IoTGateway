using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;
using Waher.Things.Snmp.Snmp1.RFC1213_MIB;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp1\Asn1\RFC1215.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Things.Snmp.Snmp1.RFC1215
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId snmp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1213_MIB._DefinedValues.mib_2, 11);
		public static readonly System.Numerics.BigInteger coldStart = 0;
		public static readonly System.Numerics.BigInteger warmStart = 1;
		public static readonly System.Numerics.BigInteger linkDown = 2;
		public static readonly System.Numerics.BigInteger linkUp = 3;
		public static readonly System.Numerics.BigInteger authenticationFailure = 4;
		public static readonly System.Numerics.BigInteger egpNeighborLoss = 5;
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
