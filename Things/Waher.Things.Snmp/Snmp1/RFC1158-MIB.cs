using Asn1 = Waher.Content.Asn1;
using Asn1F = System.Formats.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

/********************************************************************************
 *
 * NOTE: This code has been automatically generated from ASN.1 file:
 * C:\My Projects\IoTGateway\Things\Waher.Things.Snmp\Snmp1\Asn1\RFC1158-MIB.asn1
 *
 ********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Waher.Things.Snmp.Snmp1.RFC1158_MIB
{
	public static partial class _DefinedValues
	{
		public static readonly Asn1.ObjectId mib_2 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1155_SMI._DefinedValues.mgmt, 1);
		public static readonly Asn1.ObjectId system = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 1);
		public static readonly Asn1.ObjectId interfaces = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 2);
		public static readonly Asn1.ObjectId at = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 3);
		public static readonly Asn1.ObjectId ip = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 4);
		public static readonly Asn1.ObjectId icmp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 5);
		public static readonly Asn1.ObjectId tcp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 6);
		public static readonly Asn1.ObjectId udp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 7);
		public static readonly Asn1.ObjectId egp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 8);
		public static readonly Asn1.ObjectId transmission = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 10);
		public static readonly Asn1.ObjectId snmp = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.mib_2, 11);
		public static readonly Asn1.ObjectId sysDescr = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 1);
		public static readonly Asn1.ObjectId sysObjectID = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 2);
		public static readonly Asn1.ObjectId sysUpTime = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 3);
		public static readonly Asn1.ObjectId sysContact = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 4);
		public static readonly Asn1.ObjectId sysName = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 5);
		public static readonly Asn1.ObjectId sysLocation = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 6);
		public static readonly Asn1.ObjectId sysServices = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.system, 7);
		public static readonly Asn1.ObjectId ifNumber = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.interfaces, 1);
		public static readonly Asn1.ObjectId ifTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.interfaces, 2);
		public static readonly Asn1.ObjectId ifEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifTable, 1);
		public static readonly Asn1.Construct.ISet ifSpeedInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifLastChangeInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInOctetsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInUcastPktsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInNUcastPktsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInDiscardsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInErrorsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifInUnknownProtosInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutOctetsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutUcastPktsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutNUcastPktsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutDiscardsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutErrorsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet ifOutQLenInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.ObjectId ifIndex = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 1);
		public static readonly Asn1.ObjectId ifDescr = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 2);
		public static readonly Asn1.ObjectId ifType = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 3);
		public static readonly Asn1.ObjectId ifMtu = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 4);
		public static readonly Asn1.ObjectId ifSpeed = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 5);
		public static readonly Asn1.ObjectId ifPhysAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 6);
		public static readonly Asn1.ObjectId ifAdminStatus = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 7);
		public static readonly Asn1.ObjectId ifOperStatus = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 8);
		public static readonly Asn1.ObjectId ifLastChange = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 9);
		public static readonly Asn1.ObjectId ifInOctets = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 10);
		public static readonly Asn1.ObjectId ifInUcastPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 11);
		public static readonly Asn1.ObjectId ifInNUcastPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 12);
		public static readonly Asn1.ObjectId ifInDiscards = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 13);
		public static readonly Asn1.ObjectId ifInErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 14);
		public static readonly Asn1.ObjectId ifInUnknownProtos = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 15);
		public static readonly Asn1.ObjectId ifOutOctets = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 16);
		public static readonly Asn1.ObjectId ifOutUcastPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 17);
		public static readonly Asn1.ObjectId ifOutNUcastPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 18);
		public static readonly Asn1.ObjectId ifOutDiscards = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 19);
		public static readonly Asn1.ObjectId ifOutErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 20);
		public static readonly Asn1.ObjectId ifOutQLen = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 21);
		public static readonly Asn1.ObjectId ifSpecific = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ifEntry, 22);
		public static readonly Asn1.ObjectId nullSpecific = new Asn1.ObjectId(0, 0);
		public static readonly Asn1.ObjectId atTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.at, 1);
		public static readonly Asn1.ObjectId atEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.atTable, 1);
		public static readonly Asn1.ObjectId atIfIndex = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.atEntry, 1);
		public static readonly Asn1.ObjectId atPhysAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.atEntry, 2);
		public static readonly Asn1.ObjectId atNetAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.atEntry, 3);
		public static readonly Asn1.ObjectId ipForwarding = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 1);
		public static readonly Asn1.ObjectId ipDefaultTTL = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 2);
		public static readonly Asn1.ObjectId ipInReceives = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 3);
		public static readonly Asn1.ObjectId ipInHdrErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 4);
		public static readonly Asn1.ObjectId ipInAddrErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 5);
		public static readonly Asn1.ObjectId ipForwDatagrams = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 6);
		public static readonly Asn1.ObjectId ipInUnknownProtos = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 7);
		public static readonly Asn1.ObjectId ipInDiscards = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 8);
		public static readonly Asn1.ObjectId ipInDelivers = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 9);
		public static readonly Asn1.ObjectId ipOutRequests = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 10);
		public static readonly Asn1.ObjectId ipOutDiscards = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 11);
		public static readonly Asn1.ObjectId ipOutNoRoutes = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 12);
		public static readonly Asn1.ObjectId ipReasmTimeout = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 13);
		public static readonly Asn1.ObjectId ipReasmReqds = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 14);
		public static readonly Asn1.ObjectId ipReasmOKs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 15);
		public static readonly Asn1.ObjectId ipReasmFails = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 16);
		public static readonly Asn1.ObjectId ipFragOKs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 17);
		public static readonly Asn1.ObjectId ipFragFails = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 18);
		public static readonly Asn1.ObjectId ipFragCreates = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 19);
		public static readonly Asn1.ObjectId ipAddrTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 20);
		public static readonly Asn1.ObjectId ipAddrEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrTable, 1);
		public static readonly Asn1.Construct.ISet ipAdEntAddrSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet ipAdEntNetMaskSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet ipAdEntReasmMaxSizeInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.ObjectId ipAdEntAddr = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrEntry, 1);
		public static readonly Asn1.ObjectId ipAdEntIfIndex = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrEntry, 2);
		public static readonly Asn1.ObjectId ipAdEntNetMask = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrEntry, 3);
		public static readonly Asn1.ObjectId ipAdEntBcastAddr = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrEntry, 4);
		public static readonly Asn1.ObjectId ipAdEntReasmMaxSiz = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipAddrEntry, 5);
		public static readonly Asn1.ObjectId ipRoutingTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 21);
		public static readonly Asn1.ObjectId ipRouteEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRoutingTable, 1);
		public static readonly Asn1.Construct.ISet ipRouteDestSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet ipRouteNextHopSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet ipRouteMaskSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.ObjectId ipRouteDest = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 1);
		public static readonly Asn1.ObjectId ipRouteIfIndex = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 2);
		public static readonly Asn1.ObjectId ipRouteMetric1 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 3);
		public static readonly Asn1.ObjectId ipRouteMetric2 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 4);
		public static readonly Asn1.ObjectId ipRouteMetric3 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 5);
		public static readonly Asn1.ObjectId ipRouteMetric4 = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 6);
		public static readonly Asn1.ObjectId ipRouteNextHop = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 7);
		public static readonly Asn1.ObjectId ipRouteType = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 8);
		public static readonly Asn1.ObjectId ipRouteProto = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 9);
		public static readonly Asn1.ObjectId ipRouteAge = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 10);
		public static readonly Asn1.ObjectId ipRouteMask = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipRouteEntry, 11);
		public static readonly Asn1.ObjectId ipNetToMediaTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ip, 22);
		public static readonly Asn1.ObjectId ipNetToMediaEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipNetToMediaTable, 1);
		public static readonly Asn1.Construct.ISet ipNetToMediaNetAddressSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.ObjectId ipNetToMediaIfIndex = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipNetToMediaEntry, 1);
		public static readonly Asn1.ObjectId ipNetToMediaPhysAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipNetToMediaEntry, 2);
		public static readonly Asn1.ObjectId ipNetToMediaNetAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipNetToMediaEntry, 3);
		public static readonly Asn1.ObjectId ipNetToMediaType = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.ipNetToMediaEntry, 4);
		public static readonly Asn1.ObjectId icmpInMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 1);
		public static readonly Asn1.ObjectId icmpInErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 2);
		public static readonly Asn1.ObjectId icmpInDestUnreachs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 3);
		public static readonly Asn1.ObjectId icmpInTimeExcds = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 4);
		public static readonly Asn1.ObjectId icmpInParmProbs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 5);
		public static readonly Asn1.ObjectId icmpInSrcQuenchs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 6);
		public static readonly Asn1.ObjectId icmpInRedirects = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 7);
		public static readonly Asn1.ObjectId icmpInEchos = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 8);
		public static readonly Asn1.ObjectId icmpInEchoReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 9);
		public static readonly Asn1.ObjectId icmpInTimestamps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 10);
		public static readonly Asn1.ObjectId icmpInTimestampReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 11);
		public static readonly Asn1.ObjectId icmpInAddrMasks = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 12);
		public static readonly Asn1.ObjectId icmpInAddrMaskReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 13);
		public static readonly Asn1.ObjectId icmpOutMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 14);
		public static readonly Asn1.ObjectId icmpOutErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 15);
		public static readonly Asn1.ObjectId icmpOutDestUnreachs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 16);
		public static readonly Asn1.ObjectId icmpOutTimeExcds = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 17);
		public static readonly Asn1.ObjectId icmpOutParmProbs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 18);
		public static readonly Asn1.ObjectId icmpOutSrcQuenchs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 19);
		public static readonly Asn1.ObjectId icmpOutRedirects = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 20);
		public static readonly Asn1.ObjectId icmpOutEchos = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 21);
		public static readonly Asn1.ObjectId icmpOutEchoReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 22);
		public static readonly Asn1.ObjectId icmpOutTimestamps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 23);
		public static readonly Asn1.ObjectId icmpOutTimestampReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 24);
		public static readonly Asn1.ObjectId icmpOutAddrMasks = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 25);
		public static readonly Asn1.ObjectId icmpOutAddrMaskReps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.icmp, 26);
		public static readonly Asn1.ObjectId tcpRtoAlgorithm = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 1);
		public static readonly Asn1.ObjectId tcpRtoMin = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 2);
		public static readonly Asn1.ObjectId tcpRtoMax = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 3);
		public static readonly Asn1.ObjectId tcpMaxConn = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 4);
		public static readonly Asn1.ObjectId tcpActiveOpens = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 5);
		public static readonly Asn1.ObjectId tcpPassiveOpens = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 6);
		public static readonly Asn1.ObjectId tcpAttemptFails = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 7);
		public static readonly Asn1.ObjectId tcpEstabResets = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 8);
		public static readonly Asn1.ObjectId tcpCurrEstab = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 9);
		public static readonly Asn1.ObjectId tcpInSegs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 10);
		public static readonly Asn1.ObjectId tcpOutSegs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 11);
		public static readonly Asn1.ObjectId tcpRetransSegs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 12);
		public static readonly Asn1.ObjectId tcpConnTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 13);
		public static readonly Asn1.ObjectId tcpConnEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnTable, 1);
		public static readonly Asn1.Construct.ISet tcpConnLocalAddressSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet tcpConnLocalPortInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.Construct.ISet tcpConnRemAddressSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet tcpConnRemPortInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.ObjectId tcpConnState = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnEntry, 1);
		public static readonly Asn1.ObjectId tcpConnLocalAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnEntry, 2);
		public static readonly Asn1.ObjectId tcpConnLocalPort = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnEntry, 3);
		public static readonly Asn1.ObjectId tcpConnRemAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnEntry, 4);
		public static readonly Asn1.ObjectId tcpConnRemPort = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcpConnEntry, 5);
		public static readonly Asn1.ObjectId tcpInErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 14);
		public static readonly Asn1.ObjectId tcpOutRsts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.tcp, 15);
		public static readonly Asn1.ObjectId udpInDatagrams = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udp, 1);
		public static readonly Asn1.ObjectId udpNoPorts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udp, 2);
		public static readonly Asn1.ObjectId udpInErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udp, 3);
		public static readonly Asn1.ObjectId udpOutDatagrams = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udp, 4);
		public static readonly Asn1.ObjectId udpTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udp, 5);
		public static readonly Asn1.ObjectId udpEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udpTable, 1);
		public static readonly Asn1.Construct.ISet udpLocalAddressSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet udpLocalPortInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 65535);
		public static readonly Asn1.ObjectId udpLocalAddress = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udpEntry, 1);
		public static readonly Asn1.ObjectId udpLocalPort = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.udpEntry, 2);
		public static readonly Asn1.ObjectId egpInMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 1);
		public static readonly Asn1.ObjectId egpInErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 2);
		public static readonly Asn1.ObjectId egpOutMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 3);
		public static readonly Asn1.ObjectId egpOutErrors = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 4);
		public static readonly Asn1.ObjectId egpNeighTable = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 5);
		public static readonly Asn1.ObjectId egpNeighEntry = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighTable, 1);
		public static readonly Asn1.Construct.ISet egpNeighAddrSize = new Asn1.Construct.SingleValue<System.Numerics.BigInteger>(4);
		public static readonly Asn1.Construct.ISet egpNeighInMsgsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighInErrsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighOutMsgsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighOutErrsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighInErrMsgsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighOutErrMsgsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighStateUpsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.Construct.ISet egpNeighStateDownsInSet = new Asn1.Construct.Interval<System.Numerics.BigInteger>(0, 4294967295);
		public static readonly Asn1.ObjectId egpNeighState = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 1);
		public static readonly Asn1.ObjectId egpNeighAddr = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 2);
		public static readonly Asn1.ObjectId egpNeighAs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 3);
		public static readonly Asn1.ObjectId egpNeighInMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 4);
		public static readonly Asn1.ObjectId egpNeighInErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 5);
		public static readonly Asn1.ObjectId egpNeighOutMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 6);
		public static readonly Asn1.ObjectId egpNeighOutErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 7);
		public static readonly Asn1.ObjectId egpNeighInErrMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 8);
		public static readonly Asn1.ObjectId egpNeighOutErrMsgs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 9);
		public static readonly Asn1.ObjectId egpNeighStateUps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 10);
		public static readonly Asn1.ObjectId egpNeighStateDowns = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 11);
		public static readonly Asn1.ObjectId egpNeighIntervalHello = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 12);
		public static readonly Asn1.ObjectId egpNeighIntervalPoll = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 13);
		public static readonly Asn1.ObjectId egpNeighMode = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 14);
		public static readonly Asn1.ObjectId egpNeighEventTrigger = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egpNeighEntry, 15);
		public static readonly Asn1.ObjectId egpAs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.egp, 6);
		public static readonly Asn1.ObjectId snmpInPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 1);
		public static readonly Asn1.ObjectId snmpOutPkts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 2);
		public static readonly Asn1.ObjectId snmpInBadVersions = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 3);
		public static readonly Asn1.ObjectId snmpInBadCommunityNames = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 4);
		public static readonly Asn1.ObjectId snmpInBadCommunityUses = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 5);
		public static readonly Asn1.ObjectId snmpInASNParseErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 6);
		public static readonly Asn1.ObjectId snmpInBadTypes = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 7);
		public static readonly Asn1.ObjectId snmpInTooBigs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 8);
		public static readonly Asn1.ObjectId snmpInNoSuchNames = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 9);
		public static readonly Asn1.ObjectId snmpInBadValues = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 10);
		public static readonly Asn1.ObjectId snmpInReadOnlys = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 11);
		public static readonly Asn1.ObjectId snmpInGenErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 12);
		public static readonly Asn1.ObjectId snmpInTotalReqVars = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 13);
		public static readonly Asn1.ObjectId snmpInTotalSetVars = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 14);
		public static readonly Asn1.ObjectId snmpInGetRequests = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 15);
		public static readonly Asn1.ObjectId snmpInGetNexts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 16);
		public static readonly Asn1.ObjectId snmpInSetRequests = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 17);
		public static readonly Asn1.ObjectId snmpInGetResponses = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 18);
		public static readonly Asn1.ObjectId snmpInTraps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 19);
		public static readonly Asn1.ObjectId snmpOutTooBigs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 20);
		public static readonly Asn1.ObjectId snmpOutNoSuchNames = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 21);
		public static readonly Asn1.ObjectId snmpOutBadValues = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 22);
		public static readonly Asn1.ObjectId snmpOutReadOnlys = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 23);
		public static readonly Asn1.ObjectId snmpOutGenErrs = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 24);
		public static readonly Asn1.ObjectId snmpOutGetRequests = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 25);
		public static readonly Asn1.ObjectId snmpOutGetNexts = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 26);
		public static readonly Asn1.ObjectId snmpOutSetRequests = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 27);
		public static readonly Asn1.ObjectId snmpOutGetResponses = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 28);
		public static readonly Asn1.ObjectId snmpOutTraps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 29);
		public static readonly Asn1.ObjectId snmpEnableAuthTraps = new Asn1.ObjectId(Waher.Things.Snmp.Snmp1.RFC1158_MIB._DefinedValues.snmp, 30);
	}

	public partial class IfEntry : Asn1.Construct
	{
		public System.Numerics.BigInteger ifIndex;
		public byte[] ifDescr;
		public System.Numerics.BigInteger ifType;
		public System.Numerics.BigInteger ifMtu;
		public System.Numerics.BigInteger ifSpeed;
		public byte[] ifPhysAddress;
		public System.Numerics.BigInteger ifAdminStatus;
		public System.Numerics.BigInteger ifOperStatus;
		public System.Numerics.BigInteger ifLastChange;
		public System.Numerics.BigInteger ifInOctets;
		public System.Numerics.BigInteger ifInUcastPkts;
		public System.Numerics.BigInteger ifInNUcastPkts;
		public System.Numerics.BigInteger ifInDiscards;
		public System.Numerics.BigInteger ifInErrors;
		public System.Numerics.BigInteger ifInUnknownProtos;
		public System.Numerics.BigInteger ifOutOctets;
		public System.Numerics.BigInteger ifOutUcastPkts;
		public System.Numerics.BigInteger ifOutNUcastPkts;
		public System.Numerics.BigInteger ifOutDiscards;
		public System.Numerics.BigInteger ifOutErrors;
		public System.Numerics.BigInteger ifOutQLen;
		public Asn1.ObjectId ifSpecific;

		public static IfEntry Create(Asn1F.AsnReader Reader)
		{
			IfEntry _Result = new IfEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.ifIndex = ReadInteger(Reader);
			this.ifDescr = ReadOctetString(Reader);
			this.ifType = ReadInteger(Reader);
			this.ifMtu = ReadInteger(Reader);
			this.ifSpeed = ReadIntegerImplicit(Reader, 4);
			if (!(Asn1.Construct.CheckInSet(this.ifSpeed, _DefinedValues.ifSpeedInSet)))
				throw new Asn1F.AsnContentException("ifSpeed out of bounds.");
			this.ifPhysAddress = ReadOctetString(Reader);
			this.ifAdminStatus = ReadInteger(Reader);
			this.ifOperStatus = ReadInteger(Reader);
			this.ifLastChange = ReadIntegerImplicit(Reader, 8);
			if (!(Asn1.Construct.CheckInSet(this.ifLastChange, _DefinedValues.ifLastChangeInSet)))
				throw new Asn1F.AsnContentException("ifLastChange out of bounds.");
			this.ifInOctets = ReadIntegerImplicit(Reader, 9);
			if (!(Asn1.Construct.CheckInSet(this.ifInOctets, _DefinedValues.ifInOctetsInSet)))
				throw new Asn1F.AsnContentException("ifInOctets out of bounds.");
			this.ifInUcastPkts = ReadIntegerImplicit(Reader, 10);
			if (!(Asn1.Construct.CheckInSet(this.ifInUcastPkts, _DefinedValues.ifInUcastPktsInSet)))
				throw new Asn1F.AsnContentException("ifInUcastPkts out of bounds.");
			this.ifInNUcastPkts = ReadIntegerImplicit(Reader, 11);
			if (!(Asn1.Construct.CheckInSet(this.ifInNUcastPkts, _DefinedValues.ifInNUcastPktsInSet)))
				throw new Asn1F.AsnContentException("ifInNUcastPkts out of bounds.");
			this.ifInDiscards = ReadIntegerImplicit(Reader, 12);
			if (!(Asn1.Construct.CheckInSet(this.ifInDiscards, _DefinedValues.ifInDiscardsInSet)))
				throw new Asn1F.AsnContentException("ifInDiscards out of bounds.");
			this.ifInErrors = ReadIntegerImplicit(Reader, 13);
			if (!(Asn1.Construct.CheckInSet(this.ifInErrors, _DefinedValues.ifInErrorsInSet)))
				throw new Asn1F.AsnContentException("ifInErrors out of bounds.");
			this.ifInUnknownProtos = ReadIntegerImplicit(Reader, 14);
			if (!(Asn1.Construct.CheckInSet(this.ifInUnknownProtos, _DefinedValues.ifInUnknownProtosInSet)))
				throw new Asn1F.AsnContentException("ifInUnknownProtos out of bounds.");
			this.ifOutOctets = ReadIntegerImplicit(Reader, 15);
			if (!(Asn1.Construct.CheckInSet(this.ifOutOctets, _DefinedValues.ifOutOctetsInSet)))
				throw new Asn1F.AsnContentException("ifOutOctets out of bounds.");
			this.ifOutUcastPkts = ReadIntegerImplicit(Reader, 16);
			if (!(Asn1.Construct.CheckInSet(this.ifOutUcastPkts, _DefinedValues.ifOutUcastPktsInSet)))
				throw new Asn1F.AsnContentException("ifOutUcastPkts out of bounds.");
			this.ifOutNUcastPkts = ReadIntegerImplicit(Reader, 17);
			if (!(Asn1.Construct.CheckInSet(this.ifOutNUcastPkts, _DefinedValues.ifOutNUcastPktsInSet)))
				throw new Asn1F.AsnContentException("ifOutNUcastPkts out of bounds.");
			this.ifOutDiscards = ReadIntegerImplicit(Reader, 18);
			if (!(Asn1.Construct.CheckInSet(this.ifOutDiscards, _DefinedValues.ifOutDiscardsInSet)))
				throw new Asn1F.AsnContentException("ifOutDiscards out of bounds.");
			this.ifOutErrors = ReadIntegerImplicit(Reader, 19);
			if (!(Asn1.Construct.CheckInSet(this.ifOutErrors, _DefinedValues.ifOutErrorsInSet)))
				throw new Asn1F.AsnContentException("ifOutErrors out of bounds.");
			this.ifOutQLen = ReadIntegerImplicit(Reader, 20);
			if (!(Asn1.Construct.CheckInSet(this.ifOutQLen, _DefinedValues.ifOutQLenInSet)))
				throw new Asn1F.AsnContentException("ifOutQLen out of bounds.");
			this.ifSpecific = ReadObjectId(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IfEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IfEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.ifIndex);
				WriteOctetString(Writer, this.ifDescr);
				WriteInteger(Writer, this.ifType);
				WriteInteger(Writer, this.ifMtu);
				if (!(Asn1.Construct.CheckInSet(this.ifSpeed, _DefinedValues.ifSpeedInSet)))
					throw new Asn1F.AsnContentException("ifSpeed out of bounds.");
				WriteIntegerImplicit(Writer, 4, this.ifSpeed);
				WriteOctetString(Writer, this.ifPhysAddress);
				WriteInteger(Writer, this.ifAdminStatus);
				WriteInteger(Writer, this.ifOperStatus);
				if (!(Asn1.Construct.CheckInSet(this.ifLastChange, _DefinedValues.ifLastChangeInSet)))
					throw new Asn1F.AsnContentException("ifLastChange out of bounds.");
				WriteIntegerImplicit(Writer, 8, this.ifLastChange);
				if (!(Asn1.Construct.CheckInSet(this.ifInOctets, _DefinedValues.ifInOctetsInSet)))
					throw new Asn1F.AsnContentException("ifInOctets out of bounds.");
				WriteIntegerImplicit(Writer, 9, this.ifInOctets);
				if (!(Asn1.Construct.CheckInSet(this.ifInUcastPkts, _DefinedValues.ifInUcastPktsInSet)))
					throw new Asn1F.AsnContentException("ifInUcastPkts out of bounds.");
				WriteIntegerImplicit(Writer, 10, this.ifInUcastPkts);
				if (!(Asn1.Construct.CheckInSet(this.ifInNUcastPkts, _DefinedValues.ifInNUcastPktsInSet)))
					throw new Asn1F.AsnContentException("ifInNUcastPkts out of bounds.");
				WriteIntegerImplicit(Writer, 11, this.ifInNUcastPkts);
				if (!(Asn1.Construct.CheckInSet(this.ifInDiscards, _DefinedValues.ifInDiscardsInSet)))
					throw new Asn1F.AsnContentException("ifInDiscards out of bounds.");
				WriteIntegerImplicit(Writer, 12, this.ifInDiscards);
				if (!(Asn1.Construct.CheckInSet(this.ifInErrors, _DefinedValues.ifInErrorsInSet)))
					throw new Asn1F.AsnContentException("ifInErrors out of bounds.");
				WriteIntegerImplicit(Writer, 13, this.ifInErrors);
				if (!(Asn1.Construct.CheckInSet(this.ifInUnknownProtos, _DefinedValues.ifInUnknownProtosInSet)))
					throw new Asn1F.AsnContentException("ifInUnknownProtos out of bounds.");
				WriteIntegerImplicit(Writer, 14, this.ifInUnknownProtos);
				if (!(Asn1.Construct.CheckInSet(this.ifOutOctets, _DefinedValues.ifOutOctetsInSet)))
					throw new Asn1F.AsnContentException("ifOutOctets out of bounds.");
				WriteIntegerImplicit(Writer, 15, this.ifOutOctets);
				if (!(Asn1.Construct.CheckInSet(this.ifOutUcastPkts, _DefinedValues.ifOutUcastPktsInSet)))
					throw new Asn1F.AsnContentException("ifOutUcastPkts out of bounds.");
				WriteIntegerImplicit(Writer, 16, this.ifOutUcastPkts);
				if (!(Asn1.Construct.CheckInSet(this.ifOutNUcastPkts, _DefinedValues.ifOutNUcastPktsInSet)))
					throw new Asn1F.AsnContentException("ifOutNUcastPkts out of bounds.");
				WriteIntegerImplicit(Writer, 17, this.ifOutNUcastPkts);
				if (!(Asn1.Construct.CheckInSet(this.ifOutDiscards, _DefinedValues.ifOutDiscardsInSet)))
					throw new Asn1F.AsnContentException("ifOutDiscards out of bounds.");
				WriteIntegerImplicit(Writer, 18, this.ifOutDiscards);
				if (!(Asn1.Construct.CheckInSet(this.ifOutErrors, _DefinedValues.ifOutErrorsInSet)))
					throw new Asn1F.AsnContentException("ifOutErrors out of bounds.");
				WriteIntegerImplicit(Writer, 19, this.ifOutErrors);
				if (!(Asn1.Construct.CheckInSet(this.ifOutQLen, _DefinedValues.ifOutQLenInSet)))
					throw new Asn1F.AsnContentException("ifOutQLen out of bounds.");
				WriteIntegerImplicit(Writer, 20, this.ifOutQLen);
				WriteObjectId(Writer, this.ifSpecific);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IfEntry _Typed)) return false;
			if (!AreEqual(this.ifIndex, _Typed.ifIndex)) return false;
			if (!AreEqual(this.ifDescr, _Typed.ifDescr)) return false;
			if (!AreEqual(this.ifType, _Typed.ifType)) return false;
			if (!AreEqual(this.ifMtu, _Typed.ifMtu)) return false;
			if (!AreEqual(this.ifSpeed, _Typed.ifSpeed)) return false;
			if (!AreEqual(this.ifPhysAddress, _Typed.ifPhysAddress)) return false;
			if (!AreEqual(this.ifAdminStatus, _Typed.ifAdminStatus)) return false;
			if (!AreEqual(this.ifOperStatus, _Typed.ifOperStatus)) return false;
			if (!AreEqual(this.ifLastChange, _Typed.ifLastChange)) return false;
			if (!AreEqual(this.ifInOctets, _Typed.ifInOctets)) return false;
			if (!AreEqual(this.ifInUcastPkts, _Typed.ifInUcastPkts)) return false;
			if (!AreEqual(this.ifInNUcastPkts, _Typed.ifInNUcastPkts)) return false;
			if (!AreEqual(this.ifInDiscards, _Typed.ifInDiscards)) return false;
			if (!AreEqual(this.ifInErrors, _Typed.ifInErrors)) return false;
			if (!AreEqual(this.ifInUnknownProtos, _Typed.ifInUnknownProtos)) return false;
			if (!AreEqual(this.ifOutOctets, _Typed.ifOutOctets)) return false;
			if (!AreEqual(this.ifOutUcastPkts, _Typed.ifOutUcastPkts)) return false;
			if (!AreEqual(this.ifOutNUcastPkts, _Typed.ifOutNUcastPkts)) return false;
			if (!AreEqual(this.ifOutDiscards, _Typed.ifOutDiscards)) return false;
			if (!AreEqual(this.ifOutErrors, _Typed.ifOutErrors)) return false;
			if (!AreEqual(this.ifOutQLen, _Typed.ifOutQLen)) return false;
			if (!AreEqual(this.ifSpecific, _Typed.ifSpecific)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifIndex);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifDescr);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifMtu);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifSpeed);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifPhysAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifAdminStatus);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOperStatus);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifLastChange);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInOctets);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInUcastPkts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInNUcastPkts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInDiscards);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInErrors);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifInUnknownProtos);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutOctets);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutUcastPkts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutNUcastPkts);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutDiscards);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutErrors);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifOutQLen);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ifSpecific);
			return _Result;
		}
	}

	public partial class AtEntry : Asn1.Construct
	{
		public System.Numerics.BigInteger atIfIndex;
		public byte[] atPhysAddress;
		public NetworkAddress atNetAddress;

		public static AtEntry Create(Asn1F.AsnReader Reader)
		{
			AtEntry _Result = new AtEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.atIfIndex = ReadInteger(Reader);
			this.atPhysAddress = ReadOctetString(Reader);
			this.atNetAddress = NetworkAddress.Create(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out AtEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new AtEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.atIfIndex);
				WriteOctetString(Writer, this.atPhysAddress);
				this.atNetAddress.Write(Writer);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is AtEntry _Typed)) return false;
			if (!AreEqual(this.atIfIndex, _Typed.atIfIndex)) return false;
			if (!AreEqual(this.atPhysAddress, _Typed.atPhysAddress)) return false;
			if (!AreEqual(this.atNetAddress, _Typed.atNetAddress)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.atIfIndex);
			_Result ^= _Result << 5 ^ CalcHashCode(this.atPhysAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.atNetAddress);
			return _Result;
		}
	}

	public partial class IpAddrEntry : Asn1.Construct
	{
		public byte[] ipAdEntAddr;
		public System.Numerics.BigInteger ipAdEntIfIndex;
		public byte[] ipAdEntNetMask;
		public System.Numerics.BigInteger ipAdEntBcastAddr;
		public System.Numerics.BigInteger ipAdEntReasmMaxSize;

		public static IpAddrEntry Create(Asn1F.AsnReader Reader)
		{
			IpAddrEntry _Result = new IpAddrEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.ipAdEntAddr = ReadOctetStringImplicit(Reader, 0);
			if (!(Asn1.Construct.CheckSize(this.ipAdEntAddr, _DefinedValues.ipAdEntAddrSize)))
				throw new Asn1F.AsnContentException("ipAdEntAddr out of bounds.");
			this.ipAdEntIfIndex = ReadInteger(Reader);
			this.ipAdEntNetMask = ReadOctetStringImplicit(Reader, 2);
			if (!(Asn1.Construct.CheckSize(this.ipAdEntNetMask, _DefinedValues.ipAdEntNetMaskSize)))
				throw new Asn1F.AsnContentException("ipAdEntNetMask out of bounds.");
			this.ipAdEntBcastAddr = ReadInteger(Reader);
			this.ipAdEntReasmMaxSize = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.ipAdEntReasmMaxSize, _DefinedValues.ipAdEntReasmMaxSizeInSet)))
				throw new Asn1F.AsnContentException("ipAdEntReasmMaxSize out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IpAddrEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IpAddrEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.ipAdEntAddr, _DefinedValues.ipAdEntAddrSize)))
					throw new Asn1F.AsnContentException("ipAdEntAddr out of bounds.");
				WriteOctetStringImplicit(Writer, 0, this.ipAdEntAddr);
				WriteInteger(Writer, this.ipAdEntIfIndex);
				if (!(Asn1.Construct.CheckSize(this.ipAdEntNetMask, _DefinedValues.ipAdEntNetMaskSize)))
					throw new Asn1F.AsnContentException("ipAdEntNetMask out of bounds.");
				WriteOctetStringImplicit(Writer, 2, this.ipAdEntNetMask);
				WriteInteger(Writer, this.ipAdEntBcastAddr);
				if (!(Asn1.Construct.CheckInSet(this.ipAdEntReasmMaxSize, _DefinedValues.ipAdEntReasmMaxSizeInSet)))
					throw new Asn1F.AsnContentException("ipAdEntReasmMaxSize out of bounds.");
				WriteInteger(Writer, this.ipAdEntReasmMaxSize);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IpAddrEntry _Typed)) return false;
			if (!AreEqual(this.ipAdEntAddr, _Typed.ipAdEntAddr)) return false;
			if (!AreEqual(this.ipAdEntIfIndex, _Typed.ipAdEntIfIndex)) return false;
			if (!AreEqual(this.ipAdEntNetMask, _Typed.ipAdEntNetMask)) return false;
			if (!AreEqual(this.ipAdEntBcastAddr, _Typed.ipAdEntBcastAddr)) return false;
			if (!AreEqual(this.ipAdEntReasmMaxSize, _Typed.ipAdEntReasmMaxSize)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAdEntAddr);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAdEntIfIndex);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAdEntNetMask);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAdEntBcastAddr);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipAdEntReasmMaxSize);
			return _Result;
		}
	}

	public partial class IpRouteEntry : Asn1.Construct
	{
		public byte[] ipRouteDest;
		public System.Numerics.BigInteger ipRouteIfIndex;
		public System.Numerics.BigInteger ipRouteMetric1;
		public System.Numerics.BigInteger ipRouteMetric2;
		public System.Numerics.BigInteger ipRouteMetric3;
		public System.Numerics.BigInteger ipRouteMetric4;
		public byte[] ipRouteNextHop;
		public System.Numerics.BigInteger ipRouteType;
		public System.Numerics.BigInteger ipRouteProto;
		public System.Numerics.BigInteger ipRouteAge;
		public byte[] ipRouteMask;

		public static IpRouteEntry Create(Asn1F.AsnReader Reader)
		{
			IpRouteEntry _Result = new IpRouteEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.ipRouteDest = ReadOctetStringImplicit(Reader, 0);
			if (!(Asn1.Construct.CheckSize(this.ipRouteDest, _DefinedValues.ipRouteDestSize)))
				throw new Asn1F.AsnContentException("ipRouteDest out of bounds.");
			this.ipRouteIfIndex = ReadInteger(Reader);
			this.ipRouteMetric1 = ReadInteger(Reader);
			this.ipRouteMetric2 = ReadInteger(Reader);
			this.ipRouteMetric3 = ReadInteger(Reader);
			this.ipRouteMetric4 = ReadInteger(Reader);
			this.ipRouteNextHop = ReadOctetStringImplicit(Reader, 6);
			if (!(Asn1.Construct.CheckSize(this.ipRouteNextHop, _DefinedValues.ipRouteNextHopSize)))
				throw new Asn1F.AsnContentException("ipRouteNextHop out of bounds.");
			this.ipRouteType = ReadInteger(Reader);
			this.ipRouteProto = ReadInteger(Reader);
			this.ipRouteAge = ReadInteger(Reader);
			this.ipRouteMask = ReadOctetStringImplicit(Reader, 10);
			if (!(Asn1.Construct.CheckSize(this.ipRouteMask, _DefinedValues.ipRouteMaskSize)))
				throw new Asn1F.AsnContentException("ipRouteMask out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IpRouteEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IpRouteEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.ipRouteDest, _DefinedValues.ipRouteDestSize)))
					throw new Asn1F.AsnContentException("ipRouteDest out of bounds.");
				WriteOctetStringImplicit(Writer, 0, this.ipRouteDest);
				WriteInteger(Writer, this.ipRouteIfIndex);
				WriteInteger(Writer, this.ipRouteMetric1);
				WriteInteger(Writer, this.ipRouteMetric2);
				WriteInteger(Writer, this.ipRouteMetric3);
				WriteInteger(Writer, this.ipRouteMetric4);
				if (!(Asn1.Construct.CheckSize(this.ipRouteNextHop, _DefinedValues.ipRouteNextHopSize)))
					throw new Asn1F.AsnContentException("ipRouteNextHop out of bounds.");
				WriteOctetStringImplicit(Writer, 6, this.ipRouteNextHop);
				WriteInteger(Writer, this.ipRouteType);
				WriteInteger(Writer, this.ipRouteProto);
				WriteInteger(Writer, this.ipRouteAge);
				if (!(Asn1.Construct.CheckSize(this.ipRouteMask, _DefinedValues.ipRouteMaskSize)))
					throw new Asn1F.AsnContentException("ipRouteMask out of bounds.");
				WriteOctetStringImplicit(Writer, 10, this.ipRouteMask);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IpRouteEntry _Typed)) return false;
			if (!AreEqual(this.ipRouteDest, _Typed.ipRouteDest)) return false;
			if (!AreEqual(this.ipRouteIfIndex, _Typed.ipRouteIfIndex)) return false;
			if (!AreEqual(this.ipRouteMetric1, _Typed.ipRouteMetric1)) return false;
			if (!AreEqual(this.ipRouteMetric2, _Typed.ipRouteMetric2)) return false;
			if (!AreEqual(this.ipRouteMetric3, _Typed.ipRouteMetric3)) return false;
			if (!AreEqual(this.ipRouteMetric4, _Typed.ipRouteMetric4)) return false;
			if (!AreEqual(this.ipRouteNextHop, _Typed.ipRouteNextHop)) return false;
			if (!AreEqual(this.ipRouteType, _Typed.ipRouteType)) return false;
			if (!AreEqual(this.ipRouteProto, _Typed.ipRouteProto)) return false;
			if (!AreEqual(this.ipRouteAge, _Typed.ipRouteAge)) return false;
			if (!AreEqual(this.ipRouteMask, _Typed.ipRouteMask)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteDest);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteIfIndex);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteMetric1);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteMetric2);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteMetric3);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteMetric4);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteNextHop);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteType);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteProto);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteAge);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipRouteMask);
			return _Result;
		}
	}

	public partial class IpNetToMediaEntry : Asn1.Construct
	{
		public System.Numerics.BigInteger ipNetToMediaIfIndex;
		public byte[] ipNetToMediaPhysAddress;
		public byte[] ipNetToMediaNetAddress;
		public System.Numerics.BigInteger ipNetoToMediaType;

		public static IpNetToMediaEntry Create(Asn1F.AsnReader Reader)
		{
			IpNetToMediaEntry _Result = new IpNetToMediaEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.ipNetToMediaIfIndex = ReadInteger(Reader);
			this.ipNetToMediaPhysAddress = ReadOctetString(Reader);
			this.ipNetToMediaNetAddress = ReadOctetStringImplicit(Reader, 2);
			if (!(Asn1.Construct.CheckSize(this.ipNetToMediaNetAddress, _DefinedValues.ipNetToMediaNetAddressSize)))
				throw new Asn1F.AsnContentException("ipNetToMediaNetAddress out of bounds.");
			this.ipNetoToMediaType = ReadInteger(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out IpNetToMediaEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new IpNetToMediaEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.ipNetToMediaIfIndex);
				WriteOctetString(Writer, this.ipNetToMediaPhysAddress);
				if (!(Asn1.Construct.CheckSize(this.ipNetToMediaNetAddress, _DefinedValues.ipNetToMediaNetAddressSize)))
					throw new Asn1F.AsnContentException("ipNetToMediaNetAddress out of bounds.");
				WriteOctetStringImplicit(Writer, 2, this.ipNetToMediaNetAddress);
				WriteInteger(Writer, this.ipNetoToMediaType);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is IpNetToMediaEntry _Typed)) return false;
			if (!AreEqual(this.ipNetToMediaIfIndex, _Typed.ipNetToMediaIfIndex)) return false;
			if (!AreEqual(this.ipNetToMediaPhysAddress, _Typed.ipNetToMediaPhysAddress)) return false;
			if (!AreEqual(this.ipNetToMediaNetAddress, _Typed.ipNetToMediaNetAddress)) return false;
			if (!AreEqual(this.ipNetoToMediaType, _Typed.ipNetoToMediaType)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipNetToMediaIfIndex);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipNetToMediaPhysAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipNetToMediaNetAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.ipNetoToMediaType);
			return _Result;
		}
	}

	public partial class TcpConnEntry : Asn1.Construct
	{
		public System.Numerics.BigInteger tcpConnState;
		public byte[] tcpConnLocalAddress;
		public System.Numerics.BigInteger tcpConnLocalPort;
		public byte[] tcpConnRemAddress;
		public System.Numerics.BigInteger tcpConnRemPort;

		public static TcpConnEntry Create(Asn1F.AsnReader Reader)
		{
			TcpConnEntry _Result = new TcpConnEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.tcpConnState = ReadInteger(Reader);
			this.tcpConnLocalAddress = ReadOctetStringImplicit(Reader, 1);
			if (!(Asn1.Construct.CheckSize(this.tcpConnLocalAddress, _DefinedValues.tcpConnLocalAddressSize)))
				throw new Asn1F.AsnContentException("tcpConnLocalAddress out of bounds.");
			this.tcpConnLocalPort = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.tcpConnLocalPort, _DefinedValues.tcpConnLocalPortInSet)))
				throw new Asn1F.AsnContentException("tcpConnLocalPort out of bounds.");
			this.tcpConnRemAddress = ReadOctetStringImplicit(Reader, 3);
			if (!(Asn1.Construct.CheckSize(this.tcpConnRemAddress, _DefinedValues.tcpConnRemAddressSize)))
				throw new Asn1F.AsnContentException("tcpConnRemAddress out of bounds.");
			this.tcpConnRemPort = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.tcpConnRemPort, _DefinedValues.tcpConnRemPortInSet)))
				throw new Asn1F.AsnContentException("tcpConnRemPort out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out TcpConnEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new TcpConnEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.tcpConnState);
				if (!(Asn1.Construct.CheckSize(this.tcpConnLocalAddress, _DefinedValues.tcpConnLocalAddressSize)))
					throw new Asn1F.AsnContentException("tcpConnLocalAddress out of bounds.");
				WriteOctetStringImplicit(Writer, 1, this.tcpConnLocalAddress);
				if (!(Asn1.Construct.CheckInSet(this.tcpConnLocalPort, _DefinedValues.tcpConnLocalPortInSet)))
					throw new Asn1F.AsnContentException("tcpConnLocalPort out of bounds.");
				WriteInteger(Writer, this.tcpConnLocalPort);
				if (!(Asn1.Construct.CheckSize(this.tcpConnRemAddress, _DefinedValues.tcpConnRemAddressSize)))
					throw new Asn1F.AsnContentException("tcpConnRemAddress out of bounds.");
				WriteOctetStringImplicit(Writer, 3, this.tcpConnRemAddress);
				if (!(Asn1.Construct.CheckInSet(this.tcpConnRemPort, _DefinedValues.tcpConnRemPortInSet)))
					throw new Asn1F.AsnContentException("tcpConnRemPort out of bounds.");
				WriteInteger(Writer, this.tcpConnRemPort);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is TcpConnEntry _Typed)) return false;
			if (!AreEqual(this.tcpConnState, _Typed.tcpConnState)) return false;
			if (!AreEqual(this.tcpConnLocalAddress, _Typed.tcpConnLocalAddress)) return false;
			if (!AreEqual(this.tcpConnLocalPort, _Typed.tcpConnLocalPort)) return false;
			if (!AreEqual(this.tcpConnRemAddress, _Typed.tcpConnRemAddress)) return false;
			if (!AreEqual(this.tcpConnRemPort, _Typed.tcpConnRemPort)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.tcpConnState);
			_Result ^= _Result << 5 ^ CalcHashCode(this.tcpConnLocalAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.tcpConnLocalPort);
			_Result ^= _Result << 5 ^ CalcHashCode(this.tcpConnRemAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.tcpConnRemPort);
			return _Result;
		}
	}

	public partial class UdpEntry : Asn1.Construct
	{
		public byte[] udpLocalAddress;
		public System.Numerics.BigInteger udpLocalPort;

		public static UdpEntry Create(Asn1F.AsnReader Reader)
		{
			UdpEntry _Result = new UdpEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.udpLocalAddress = ReadOctetStringImplicit(Reader, 0);
			if (!(Asn1.Construct.CheckSize(this.udpLocalAddress, _DefinedValues.udpLocalAddressSize)))
				throw new Asn1F.AsnContentException("udpLocalAddress out of bounds.");
			this.udpLocalPort = ReadInteger(Reader);
			if (!(Asn1.Construct.CheckInSet(this.udpLocalPort, _DefinedValues.udpLocalPortInSet)))
				throw new Asn1F.AsnContentException("udpLocalPort out of bounds.");
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out UdpEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new UdpEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				if (!(Asn1.Construct.CheckSize(this.udpLocalAddress, _DefinedValues.udpLocalAddressSize)))
					throw new Asn1F.AsnContentException("udpLocalAddress out of bounds.");
				WriteOctetStringImplicit(Writer, 0, this.udpLocalAddress);
				if (!(Asn1.Construct.CheckInSet(this.udpLocalPort, _DefinedValues.udpLocalPortInSet)))
					throw new Asn1F.AsnContentException("udpLocalPort out of bounds.");
				WriteInteger(Writer, this.udpLocalPort);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is UdpEntry _Typed)) return false;
			if (!AreEqual(this.udpLocalAddress, _Typed.udpLocalAddress)) return false;
			if (!AreEqual(this.udpLocalPort, _Typed.udpLocalPort)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.udpLocalAddress);
			_Result ^= _Result << 5 ^ CalcHashCode(this.udpLocalPort);
			return _Result;
		}
	}

	public partial class EgpNeighEntry : Asn1.Construct
	{
		public System.Numerics.BigInteger egpNeighState;
		public byte[] egpNeighAddr;
		public System.Numerics.BigInteger egpNeighAs;
		public System.Numerics.BigInteger egpNeighInMsgs;
		public System.Numerics.BigInteger egpNeighInErrs;
		public System.Numerics.BigInteger egpNeighOutMsgs;
		public System.Numerics.BigInteger egpNeighOutErrs;
		public System.Numerics.BigInteger egpNeighInErrMsgs;
		public System.Numerics.BigInteger egpNeighOutErrMsgs;
		public System.Numerics.BigInteger egpNeighStateUps;
		public System.Numerics.BigInteger egpNeighStateDowns;
		public System.Numerics.BigInteger egpNeighIntervalHello;
		public System.Numerics.BigInteger egpNeighIntervalPoll;
		public System.Numerics.BigInteger egpNeighMode;
		public System.Numerics.BigInteger egpNeighEventTrigger;

		public static EgpNeighEntry Create(Asn1F.AsnReader Reader)
		{
			EgpNeighEntry _Result = new EgpNeighEntry();
			_Result.Read(Reader);
			return _Result;
		}

		public override void Read(Asn1F.AsnReader Reader0)
		{
			Asn1F.AsnReader Reader = Reader0.ReadSequence();
			this.egpNeighState = ReadInteger(Reader);
			this.egpNeighAddr = ReadOctetStringImplicit(Reader, 1);
			if (!(Asn1.Construct.CheckSize(this.egpNeighAddr, _DefinedValues.egpNeighAddrSize)))
				throw new Asn1F.AsnContentException("egpNeighAddr out of bounds.");
			this.egpNeighAs = ReadInteger(Reader);
			this.egpNeighInMsgs = ReadIntegerImplicit(Reader, 3);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighInMsgs, _DefinedValues.egpNeighInMsgsInSet)))
				throw new Asn1F.AsnContentException("egpNeighInMsgs out of bounds.");
			this.egpNeighInErrs = ReadIntegerImplicit(Reader, 4);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighInErrs, _DefinedValues.egpNeighInErrsInSet)))
				throw new Asn1F.AsnContentException("egpNeighInErrs out of bounds.");
			this.egpNeighOutMsgs = ReadIntegerImplicit(Reader, 5);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighOutMsgs, _DefinedValues.egpNeighOutMsgsInSet)))
				throw new Asn1F.AsnContentException("egpNeighOutMsgs out of bounds.");
			this.egpNeighOutErrs = ReadIntegerImplicit(Reader, 6);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighOutErrs, _DefinedValues.egpNeighOutErrsInSet)))
				throw new Asn1F.AsnContentException("egpNeighOutErrs out of bounds.");
			this.egpNeighInErrMsgs = ReadIntegerImplicit(Reader, 7);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighInErrMsgs, _DefinedValues.egpNeighInErrMsgsInSet)))
				throw new Asn1F.AsnContentException("egpNeighInErrMsgs out of bounds.");
			this.egpNeighOutErrMsgs = ReadIntegerImplicit(Reader, 8);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighOutErrMsgs, _DefinedValues.egpNeighOutErrMsgsInSet)))
				throw new Asn1F.AsnContentException("egpNeighOutErrMsgs out of bounds.");
			this.egpNeighStateUps = ReadIntegerImplicit(Reader, 9);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighStateUps, _DefinedValues.egpNeighStateUpsInSet)))
				throw new Asn1F.AsnContentException("egpNeighStateUps out of bounds.");
			this.egpNeighStateDowns = ReadIntegerImplicit(Reader, 10);
			if (!(Asn1.Construct.CheckInSet(this.egpNeighStateDowns, _DefinedValues.egpNeighStateDownsInSet)))
				throw new Asn1F.AsnContentException("egpNeighStateDowns out of bounds.");
			this.egpNeighIntervalHello = ReadInteger(Reader);
			this.egpNeighIntervalPoll = ReadInteger(Reader);
			this.egpNeighMode = ReadInteger(Reader);
			this.egpNeighEventTrigger = ReadInteger(Reader);
		}

		public static bool TryCreate(Asn1F.AsnReader Reader0, out EgpNeighEntry _Result)
		{
			Asn1F.AsnReader Reader = TryReadInnerVector(Reader0);
			if (Reader is null)
			{
				_Result = null;
				return false;
			}

			_Result = new EgpNeighEntry();
			_Result.Read(Reader);
			return true;
		}

		public override void Write(Asn1F.AsnWriter Writer)
		{
			using (Writer.PushSequence())
			{
				WriteInteger(Writer, this.egpNeighState);
				if (!(Asn1.Construct.CheckSize(this.egpNeighAddr, _DefinedValues.egpNeighAddrSize)))
					throw new Asn1F.AsnContentException("egpNeighAddr out of bounds.");
				WriteOctetStringImplicit(Writer, 1, this.egpNeighAddr);
				WriteInteger(Writer, this.egpNeighAs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighInMsgs, _DefinedValues.egpNeighInMsgsInSet)))
					throw new Asn1F.AsnContentException("egpNeighInMsgs out of bounds.");
				WriteIntegerImplicit(Writer, 3, this.egpNeighInMsgs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighInErrs, _DefinedValues.egpNeighInErrsInSet)))
					throw new Asn1F.AsnContentException("egpNeighInErrs out of bounds.");
				WriteIntegerImplicit(Writer, 4, this.egpNeighInErrs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighOutMsgs, _DefinedValues.egpNeighOutMsgsInSet)))
					throw new Asn1F.AsnContentException("egpNeighOutMsgs out of bounds.");
				WriteIntegerImplicit(Writer, 5, this.egpNeighOutMsgs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighOutErrs, _DefinedValues.egpNeighOutErrsInSet)))
					throw new Asn1F.AsnContentException("egpNeighOutErrs out of bounds.");
				WriteIntegerImplicit(Writer, 6, this.egpNeighOutErrs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighInErrMsgs, _DefinedValues.egpNeighInErrMsgsInSet)))
					throw new Asn1F.AsnContentException("egpNeighInErrMsgs out of bounds.");
				WriteIntegerImplicit(Writer, 7, this.egpNeighInErrMsgs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighOutErrMsgs, _DefinedValues.egpNeighOutErrMsgsInSet)))
					throw new Asn1F.AsnContentException("egpNeighOutErrMsgs out of bounds.");
				WriteIntegerImplicit(Writer, 8, this.egpNeighOutErrMsgs);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighStateUps, _DefinedValues.egpNeighStateUpsInSet)))
					throw new Asn1F.AsnContentException("egpNeighStateUps out of bounds.");
				WriteIntegerImplicit(Writer, 9, this.egpNeighStateUps);
				if (!(Asn1.Construct.CheckInSet(this.egpNeighStateDowns, _DefinedValues.egpNeighStateDownsInSet)))
					throw new Asn1F.AsnContentException("egpNeighStateDowns out of bounds.");
				WriteIntegerImplicit(Writer, 10, this.egpNeighStateDowns);
				WriteInteger(Writer, this.egpNeighIntervalHello);
				WriteInteger(Writer, this.egpNeighIntervalPoll);
				WriteInteger(Writer, this.egpNeighMode);
				WriteInteger(Writer, this.egpNeighEventTrigger);
			}
		}

		public override bool Equals(object Object)
		{
			if (!(Object is EgpNeighEntry _Typed)) return false;
			if (!AreEqual(this.egpNeighState, _Typed.egpNeighState)) return false;
			if (!AreEqual(this.egpNeighAddr, _Typed.egpNeighAddr)) return false;
			if (!AreEqual(this.egpNeighAs, _Typed.egpNeighAs)) return false;
			if (!AreEqual(this.egpNeighInMsgs, _Typed.egpNeighInMsgs)) return false;
			if (!AreEqual(this.egpNeighInErrs, _Typed.egpNeighInErrs)) return false;
			if (!AreEqual(this.egpNeighOutMsgs, _Typed.egpNeighOutMsgs)) return false;
			if (!AreEqual(this.egpNeighOutErrs, _Typed.egpNeighOutErrs)) return false;
			if (!AreEqual(this.egpNeighInErrMsgs, _Typed.egpNeighInErrMsgs)) return false;
			if (!AreEqual(this.egpNeighOutErrMsgs, _Typed.egpNeighOutErrMsgs)) return false;
			if (!AreEqual(this.egpNeighStateUps, _Typed.egpNeighStateUps)) return false;
			if (!AreEqual(this.egpNeighStateDowns, _Typed.egpNeighStateDowns)) return false;
			if (!AreEqual(this.egpNeighIntervalHello, _Typed.egpNeighIntervalHello)) return false;
			if (!AreEqual(this.egpNeighIntervalPoll, _Typed.egpNeighIntervalPoll)) return false;
			if (!AreEqual(this.egpNeighMode, _Typed.egpNeighMode)) return false;
			if (!AreEqual(this.egpNeighEventTrigger, _Typed.egpNeighEventTrigger)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			int _Result = 0;
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighState);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighAddr);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighAs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighInMsgs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighInErrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighOutMsgs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighOutErrs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighInErrMsgs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighOutErrMsgs);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighStateUps);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighStateDowns);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighIntervalHello);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighIntervalPoll);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighMode);
			_Result ^= _Result << 5 ^ CalcHashCode(this.egpNeighEventTrigger);
			return _Result;
		}
	}

}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
