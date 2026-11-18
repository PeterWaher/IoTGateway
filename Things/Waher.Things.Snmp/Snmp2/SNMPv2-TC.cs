using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Things.Snmp.Snmp2.SNMPv2_SMI;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp2\Asn1\SNMPv2-TC.asn1
 *
 ********************************************************************************/

namespace Waher.Things.Snmp.Snmp2.SNMPv2_TC
{
	public enum TruthValueEnum
	{
		@true = 1,
		@false = 2
	}

	public enum RowStatusEnum
	{
		active = 1,
		notInService = 2,
		notReady = 3,
		createAndGo = 4,
		createAndWait = 5,
		destroy = 6
	}

	public enum StorageTypeEnum
	{
		other = 1,
		@volatile = 2,
		nonVolatile = 3,
		permanent = 4,
		readOnly = 5
	}

}
