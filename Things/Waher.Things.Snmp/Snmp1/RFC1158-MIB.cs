using System;
using System.Text;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Asn1;
using Waher.Things.Snmp.Snmp1.RFC1155_SMI;

namespace Waher.Things.Snmp.Snmp1.RFC1158_MIB
{
	public static partial class Values
	{
		public static readonly ObjectId mib_2 = new ObjectId(RFC1155_SMI.Values.mgmt, 1);
		public static readonly ObjectId system = new ObjectId(mib_2, 1);
		public static readonly ObjectId interfaces = new ObjectId(mib_2, 2);
		public static readonly ObjectId at = new ObjectId(mib_2, 3);
		public static readonly ObjectId ip = new ObjectId(mib_2, 4);
		public static readonly ObjectId icmp = new ObjectId(mib_2, 5);
		public static readonly ObjectId tcp = new ObjectId(mib_2, 6);
		public static readonly ObjectId udp = new ObjectId(mib_2, 7);
		public static readonly ObjectId egp = new ObjectId(mib_2, 8);
		public static readonly ObjectId transmission = new ObjectId(mib_2, 10);
		public static readonly ObjectId snmp = new ObjectId(mib_2, 11);
		public static readonly ObjectId sysDescr = new ObjectId(system, 1);
		public static readonly ObjectId sysObjectID = new ObjectId(system, 2);
		public static readonly ObjectId sysUpTime = new ObjectId(system, 3);
		public static readonly ObjectId sysContact = new ObjectId(system, 4);
		public static readonly ObjectId sysName = new ObjectId(system, 5);
		public static readonly ObjectId sysLocation = new ObjectId(system, 6);
		public static readonly ObjectId sysServices = new ObjectId(system, 7);
		public static readonly ObjectId ifNumber = new ObjectId(interfaces, 1);
		public static readonly ObjectId ifTable = new ObjectId(interfaces, 2);
		public static readonly ObjectId ifEntry = new ObjectId(ifTable, 1);
		public static readonly ObjectId ifIndex = new ObjectId(ifEntry, 1);
		public static readonly ObjectId ifDescr = new ObjectId(ifEntry, 2);
		public static readonly ObjectId ifType = new ObjectId(ifEntry, 3);
		public static readonly ObjectId ifMtu = new ObjectId(ifEntry, 4);
		public static readonly ObjectId ifSpeed = new ObjectId(ifEntry, 5);
		public static readonly ObjectId ifPhysAddress = new ObjectId(ifEntry, 6);
		public static readonly ObjectId ifAdminStatus = new ObjectId(ifEntry, 7);
		public static readonly ObjectId ifOperStatus = new ObjectId(ifEntry, 8);
		public static readonly ObjectId ifLastChange = new ObjectId(ifEntry, 9);
		public static readonly ObjectId ifInOctets = new ObjectId(ifEntry, 10);
		public static readonly ObjectId ifInUcastPkts = new ObjectId(ifEntry, 11);
		public static readonly ObjectId ifInNUcastPkts = new ObjectId(ifEntry, 12);
		public static readonly ObjectId ifInDiscards = new ObjectId(ifEntry, 13);
		public static readonly ObjectId ifInErrors = new ObjectId(ifEntry, 14);
		public static readonly ObjectId ifInUnknownProtos = new ObjectId(ifEntry, 15);
		public static readonly ObjectId ifOutOctets = new ObjectId(ifEntry, 16);
		public static readonly ObjectId ifOutUcastPkts = new ObjectId(ifEntry, 17);
		public static readonly ObjectId ifOutNUcastPkts = new ObjectId(ifEntry, 18);
		public static readonly ObjectId ifOutDiscards = new ObjectId(ifEntry, 19);
		public static readonly ObjectId ifOutErrors = new ObjectId(ifEntry, 20);
		public static readonly ObjectId ifOutQLen = new ObjectId(ifEntry, 21);
		public static readonly ObjectId ifSpecific = new ObjectId(ifEntry, 22);
		public static readonly ObjectId nullSpecific = new ObjectId(0, 0);
		public static readonly ObjectId atTable = new ObjectId(at, 1);
		public static readonly ObjectId atEntry = new ObjectId(atTable, 1);
		public static readonly ObjectId atIfIndex = new ObjectId(atEntry, 1);
		public static readonly ObjectId atPhysAddress = new ObjectId(atEntry, 2);
		public static readonly ObjectId atNetAddress = new ObjectId(atEntry, 3);
		public static readonly ObjectId ipForwarding = new ObjectId(ip, 1);
		public static readonly ObjectId ipDefaultTTL = new ObjectId(ip, 2);
		public static readonly ObjectId ipInReceives = new ObjectId(ip, 3);
		public static readonly ObjectId ipInHdrErrors = new ObjectId(ip, 4);
		public static readonly ObjectId ipInAddrErrors = new ObjectId(ip, 5);
		public static readonly ObjectId ipForwDatagrams = new ObjectId(ip, 6);
		public static readonly ObjectId ipInUnknownProtos = new ObjectId(ip, 7);
		public static readonly ObjectId ipInDiscards = new ObjectId(ip, 8);
		public static readonly ObjectId ipInDelivers = new ObjectId(ip, 9);
		public static readonly ObjectId ipOutRequests = new ObjectId(ip, 10);
		public static readonly ObjectId ipOutDiscards = new ObjectId(ip, 11);
		public static readonly ObjectId ipOutNoRoutes = new ObjectId(ip, 12);
		public static readonly ObjectId ipReasmTimeout = new ObjectId(ip, 13);
		public static readonly ObjectId ipReasmReqds = new ObjectId(ip, 14);
		public static readonly ObjectId ipReasmOKs = new ObjectId(ip, 15);
		public static readonly ObjectId ipReasmFails = new ObjectId(ip, 16);
		public static readonly ObjectId ipFragOKs = new ObjectId(ip, 17);
		public static readonly ObjectId ipFragFails = new ObjectId(ip, 18);
		public static readonly ObjectId ipFragCreates = new ObjectId(ip, 19);
		public static readonly ObjectId ipAddrTable = new ObjectId(ip, 20);
		public static readonly ObjectId ipAddrEntry = new ObjectId(ipAddrTable, 1);
		public static readonly ObjectId ipAdEntAddr = new ObjectId(ipAddrEntry, 1);
		public static readonly ObjectId ipAdEntIfIndex = new ObjectId(ipAddrEntry, 2);
		public static readonly ObjectId ipAdEntNetMask = new ObjectId(ipAddrEntry, 3);
		public static readonly ObjectId ipAdEntBcastAddr = new ObjectId(ipAddrEntry, 4);
		public static readonly ObjectId ipAdEntReasmMaxSiz = new ObjectId(ipAddrEntry, 5);
		public static readonly ObjectId ipRoutingTable = new ObjectId(ip, 21);
		public static readonly ObjectId ipRouteEntry = new ObjectId(ipRoutingTable, 1);
		public static readonly ObjectId ipRouteDest = new ObjectId(ipRouteEntry, 1);
		public static readonly ObjectId ipRouteIfIndex = new ObjectId(ipRouteEntry, 2);
		public static readonly ObjectId ipRouteMetric1 = new ObjectId(ipRouteEntry, 3);
		public static readonly ObjectId ipRouteMetric2 = new ObjectId(ipRouteEntry, 4);
		public static readonly ObjectId ipRouteMetric3 = new ObjectId(ipRouteEntry, 5);
		public static readonly ObjectId ipRouteMetric4 = new ObjectId(ipRouteEntry, 6);
		public static readonly ObjectId ipRouteNextHop = new ObjectId(ipRouteEntry, 7);
		public static readonly ObjectId ipRouteType = new ObjectId(ipRouteEntry, 8);
		public static readonly ObjectId ipRouteProto = new ObjectId(ipRouteEntry, 9);
		public static readonly ObjectId ipRouteAge = new ObjectId(ipRouteEntry, 10);
		public static readonly ObjectId ipRouteMask = new ObjectId(ipRouteEntry, 11);
		public static readonly ObjectId ipNetToMediaTable = new ObjectId(ip, 22);
		public static readonly ObjectId ipNetToMediaEntry = new ObjectId(ipNetToMediaTable, 1);
		public static readonly ObjectId ipNetToMediaIfIndex = new ObjectId(ipNetToMediaEntry, 1);
		public static readonly ObjectId ipNetToMediaPhysAddress = new ObjectId(ipNetToMediaEntry, 2);
		public static readonly ObjectId ipNetToMediaNetAddress = new ObjectId(ipNetToMediaEntry, 3);
		public static readonly ObjectId ipNetToMediaType = new ObjectId(ipNetToMediaEntry, 4);
		public static readonly ObjectId icmpInMsgs = new ObjectId(icmp, 1);
		public static readonly ObjectId icmpInErrors = new ObjectId(icmp, 2);
		public static readonly ObjectId icmpInDestUnreachs = new ObjectId(icmp, 3);
		public static readonly ObjectId icmpInTimeExcds = new ObjectId(icmp, 4);
		public static readonly ObjectId icmpInParmProbs = new ObjectId(icmp, 5);
		public static readonly ObjectId icmpInSrcQuenchs = new ObjectId(icmp, 6);
		public static readonly ObjectId icmpInRedirects = new ObjectId(icmp, 7);
		public static readonly ObjectId icmpInEchos = new ObjectId(icmp, 8);
		public static readonly ObjectId icmpInEchoReps = new ObjectId(icmp, 9);
		public static readonly ObjectId icmpInTimestamps = new ObjectId(icmp, 10);
		public static readonly ObjectId icmpInTimestampReps = new ObjectId(icmp, 11);
		public static readonly ObjectId icmpInAddrMasks = new ObjectId(icmp, 12);
		public static readonly ObjectId icmpInAddrMaskReps = new ObjectId(icmp, 13);
		public static readonly ObjectId icmpOutMsgs = new ObjectId(icmp, 14);
		public static readonly ObjectId icmpOutErrors = new ObjectId(icmp, 15);
		public static readonly ObjectId icmpOutDestUnreachs = new ObjectId(icmp, 16);
		public static readonly ObjectId icmpOutTimeExcds = new ObjectId(icmp, 17);
		public static readonly ObjectId icmpOutParmProbs = new ObjectId(icmp, 18);
		public static readonly ObjectId icmpOutSrcQuenchs = new ObjectId(icmp, 19);
		public static readonly ObjectId icmpOutRedirects = new ObjectId(icmp, 20);
		public static readonly ObjectId icmpOutEchos = new ObjectId(icmp, 21);
		public static readonly ObjectId icmpOutEchoReps = new ObjectId(icmp, 22);
		public static readonly ObjectId icmpOutTimestamps = new ObjectId(icmp, 23);
		public static readonly ObjectId icmpOutTimestampReps = new ObjectId(icmp, 24);
		public static readonly ObjectId icmpOutAddrMasks = new ObjectId(icmp, 25);
		public static readonly ObjectId icmpOutAddrMaskReps = new ObjectId(icmp, 26);
		public static readonly ObjectId tcpRtoAlgorithm = new ObjectId(tcp, 1);
		public static readonly ObjectId tcpRtoMin = new ObjectId(tcp, 2);
		public static readonly ObjectId tcpRtoMax = new ObjectId(tcp, 3);
		public static readonly ObjectId tcpMaxConn = new ObjectId(tcp, 4);
		public static readonly ObjectId tcpActiveOpens = new ObjectId(tcp, 5);
		public static readonly ObjectId tcpPassiveOpens = new ObjectId(tcp, 6);
		public static readonly ObjectId tcpAttemptFails = new ObjectId(tcp, 7);
		public static readonly ObjectId tcpEstabResets = new ObjectId(tcp, 8);
		public static readonly ObjectId tcpCurrEstab = new ObjectId(tcp, 9);
		public static readonly ObjectId tcpInSegs = new ObjectId(tcp, 10);
		public static readonly ObjectId tcpOutSegs = new ObjectId(tcp, 11);
		public static readonly ObjectId tcpRetransSegs = new ObjectId(tcp, 12);
		public static readonly ObjectId tcpConnTable = new ObjectId(tcp, 13);
		public static readonly ObjectId tcpConnEntry = new ObjectId(tcpConnTable, 1);
		public static readonly ObjectId tcpConnState = new ObjectId(tcpConnEntry, 1);
		public static readonly ObjectId tcpConnLocalAddress = new ObjectId(tcpConnEntry, 2);
		public static readonly ObjectId tcpConnLocalPort = new ObjectId(tcpConnEntry, 3);
		public static readonly ObjectId tcpConnRemAddress = new ObjectId(tcpConnEntry, 4);
		public static readonly ObjectId tcpConnRemPort = new ObjectId(tcpConnEntry, 5);
		public static readonly ObjectId tcpInErrs = new ObjectId(tcp, 14);
		public static readonly ObjectId tcpOutRsts = new ObjectId(tcp, 15);
		public static readonly ObjectId udpInDatagrams = new ObjectId(udp, 1);
		public static readonly ObjectId udpNoPorts = new ObjectId(udp, 2);
		public static readonly ObjectId udpInErrors = new ObjectId(udp, 3);
		public static readonly ObjectId udpOutDatagrams = new ObjectId(udp, 4);
		public static readonly ObjectId udpTable = new ObjectId(udp, 5);
		public static readonly ObjectId udpEntry = new ObjectId(udpTable, 1);
		public static readonly ObjectId udpLocalAddress = new ObjectId(udpEntry, 1);
		public static readonly ObjectId udpLocalPort = new ObjectId(udpEntry, 2);
		public static readonly ObjectId egpInMsgs = new ObjectId(egp, 1);
		public static readonly ObjectId egpInErrors = new ObjectId(egp, 2);
		public static readonly ObjectId egpOutMsgs = new ObjectId(egp, 3);
		public static readonly ObjectId egpOutErrors = new ObjectId(egp, 4);
		public static readonly ObjectId egpNeighTable = new ObjectId(egp, 5);
		public static readonly ObjectId egpNeighEntry = new ObjectId(egpNeighTable, 1);
		public static readonly ObjectId egpNeighState = new ObjectId(egpNeighEntry, 1);
		public static readonly ObjectId egpNeighAddr = new ObjectId(egpNeighEntry, 2);
		public static readonly ObjectId egpNeighAs = new ObjectId(egpNeighEntry, 3);
		public static readonly ObjectId egpNeighInMsgs = new ObjectId(egpNeighEntry, 4);
		public static readonly ObjectId egpNeighInErrs = new ObjectId(egpNeighEntry, 5);
		public static readonly ObjectId egpNeighOutMsgs = new ObjectId(egpNeighEntry, 6);
		public static readonly ObjectId egpNeighOutErrs = new ObjectId(egpNeighEntry, 7);
		public static readonly ObjectId egpNeighInErrMsgs = new ObjectId(egpNeighEntry, 8);
		public static readonly ObjectId egpNeighOutErrMsgs = new ObjectId(egpNeighEntry, 9);
		public static readonly ObjectId egpNeighStateUps = new ObjectId(egpNeighEntry, 10);
		public static readonly ObjectId egpNeighStateDowns = new ObjectId(egpNeighEntry, 11);
		public static readonly ObjectId egpNeighIntervalHello = new ObjectId(egpNeighEntry, 12);
		public static readonly ObjectId egpNeighIntervalPoll = new ObjectId(egpNeighEntry, 13);
		public static readonly ObjectId egpNeighMode = new ObjectId(egpNeighEntry, 14);
		public static readonly ObjectId egpNeighEventTrigger = new ObjectId(egpNeighEntry, 15);
		public static readonly ObjectId egpAs = new ObjectId(egp, 6);
		public static readonly ObjectId snmpInPkts = new ObjectId(snmp, 1);
		public static readonly ObjectId snmpOutPkts = new ObjectId(snmp, 2);
		public static readonly ObjectId snmpInBadVersions = new ObjectId(snmp, 3);
		public static readonly ObjectId snmpInBadCommunityNames = new ObjectId(snmp, 4);
		public static readonly ObjectId snmpInBadCommunityUses = new ObjectId(snmp, 5);
		public static readonly ObjectId snmpInASNParseErrs = new ObjectId(snmp, 6);
		public static readonly ObjectId snmpInBadTypes = new ObjectId(snmp, 7);
		public static readonly ObjectId snmpInTooBigs = new ObjectId(snmp, 8);
		public static readonly ObjectId snmpInNoSuchNames = new ObjectId(snmp, 9);
		public static readonly ObjectId snmpInBadValues = new ObjectId(snmp, 10);
		public static readonly ObjectId snmpInReadOnlys = new ObjectId(snmp, 11);
		public static readonly ObjectId snmpInGenErrs = new ObjectId(snmp, 12);
		public static readonly ObjectId snmpInTotalReqVars = new ObjectId(snmp, 13);
		public static readonly ObjectId snmpInTotalSetVars = new ObjectId(snmp, 14);
		public static readonly ObjectId snmpInGetRequests = new ObjectId(snmp, 15);
		public static readonly ObjectId snmpInGetNexts = new ObjectId(snmp, 16);
		public static readonly ObjectId snmpInSetRequests = new ObjectId(snmp, 17);
		public static readonly ObjectId snmpInGetResponses = new ObjectId(snmp, 18);
		public static readonly ObjectId snmpInTraps = new ObjectId(snmp, 19);
		public static readonly ObjectId snmpOutTooBigs = new ObjectId(snmp, 20);
		public static readonly ObjectId snmpOutNoSuchNames = new ObjectId(snmp, 21);
		public static readonly ObjectId snmpOutBadValues = new ObjectId(snmp, 22);
		public static readonly ObjectId snmpOutReadOnlys = new ObjectId(snmp, 23);
		public static readonly ObjectId snmpOutGenErrs = new ObjectId(snmp, 24);
		public static readonly ObjectId snmpOutGetRequests = new ObjectId(snmp, 25);
		public static readonly ObjectId snmpOutGetNexts = new ObjectId(snmp, 26);
		public static readonly ObjectId snmpOutSetRequests = new ObjectId(snmp, 27);
		public static readonly ObjectId snmpOutGetResponses = new ObjectId(snmp, 28);
		public static readonly ObjectId snmpOutTraps = new ObjectId(snmp, 29);
		public static readonly ObjectId snmpEnableAuthTraps = new ObjectId(snmp, 30);
	}

	public class IfEntry
	{
		public Int64 ifIndex;
		public Array<Byte> ifDescr;
		public Int64 ifType;
		public Int64 ifMtu;
		public Int64 ifSpeed;
		public Array<Byte> ifPhysAddress;
		public Int64 ifAdminStatus;
		public Int64 ifOperStatus;
		public Int64 ifLastChange;
		public Int64 ifInOctets;
		public Int64 ifInUcastPkts;
		public Int64 ifInNUcastPkts;
		public Int64 ifInDiscards;
		public Int64 ifInErrors;
		public Int64 ifInUnknownProtos;
		public Int64 ifOutOctets;
		public Int64 ifOutUcastPkts;
		public Int64 ifOutNUcastPkts;
		public Int64 ifOutDiscards;
		public Int64 ifOutErrors;
		public Int64 ifOutQLen;
		public ObjectId ifSpecific;
	}

	public class AtEntry
	{
		public Int64 atIfIndex;
		public Array<Byte> atPhysAddress;
		public NetworkAddress atNetAddress;
	}

	public class IpAddrEntry
	{
		public Array<Byte> ipAdEntAddr;
		public Int64 ipAdEntIfIndex;
		public Array<Byte> ipAdEntNetMask;
		public Int64 ipAdEntBcastAddr;
		public Int64 ipAdEntReasmMaxSize;
	}

	public class IpRouteEntry
	{
		public Array<Byte> ipRouteDest;
		public Int64 ipRouteIfIndex;
		public Int64 ipRouteMetric1;
		public Int64 ipRouteMetric2;
		public Int64 ipRouteMetric3;
		public Int64 ipRouteMetric4;
		public Array<Byte> ipRouteNextHop;
		public Int64 ipRouteType;
		public Int64 ipRouteProto;
		public Int64 ipRouteAge;
		public Array<Byte> ipRouteMask;
	}

	public class IpNetToMediaEntry
	{
		public Int64 ipNetToMediaIfIndex;
		public Array<Byte> ipNetToMediaPhysAddress;
		public Array<Byte> ipNetToMediaNetAddress;
		public Int64 ipNetoToMediaType;
	}

	public class TcpConnEntry
	{
		public Int64 tcpConnState;
		public Array<Byte> tcpConnLocalAddress;
		public Int64 tcpConnLocalPort;
		public Array<Byte> tcpConnRemAddress;
		public Int64 tcpConnRemPort;
	}

	public class UdpEntry
	{
		public Array<Byte> udpLocalAddress;
		public Int64 udpLocalPort;
	}

	public class EgpNeighEntry
	{
		public Int64 egpNeighState;
		public Array<Byte> egpNeighAddr;
		public Int64 egpNeighAs;
		public Int64 egpNeighInMsgs;
		public Int64 egpNeighInErrs;
		public Int64 egpNeighOutMsgs;
		public Int64 egpNeighOutErrs;
		public Int64 egpNeighInErrMsgs;
		public Int64 egpNeighOutErrMsgs;
		public Int64 egpNeighStateUps;
		public Int64 egpNeighStateDowns;
		public Int64 egpNeighIntervalHello;
		public Int64 egpNeighIntervalPoll;
		public Int64 egpNeighMode;
		public Int64 egpNeighEventTrigger;
	}

}
