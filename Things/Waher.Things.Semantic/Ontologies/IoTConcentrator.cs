using System;

namespace Waher.Things.Semantic.Ontologies
{
	/// <summary>
	/// IoT Concentrator Ontology
	/// </summary>
	public static class IoTConcentrator
	{
		/// <summary>
		/// urn:nf:iot:concentrator:1.0:
		/// </summary>
		public const string Namespace = "urn:nf:iot:concentrator:1.0:";

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:Broker
		/// </summary>
		public static readonly Uri Broker = new Uri(Namespace + "Broker");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:DataSource
		/// </summary>
		public static readonly Uri DataSource = new Uri(Namespace + "DataSource");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:Node
		/// </summary>
		public static readonly Uri Node = new Uri(Namespace + "Node");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:mainDomain
		/// </summary>
		public static readonly Uri mainDomain = new Uri(Namespace + "mainDomain");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:alternativeDomains
		/// </summary>
		public static readonly Uri alternativeDomains = new Uri(Namespace + "alternativeDomains");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:rootSource
		/// </summary>
		public static readonly Uri rootSource = new Uri(Namespace + "rootSource");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:hasChildSource
		/// </summary>
		public static readonly Uri hasChildSource = new Uri(Namespace + "hasChildSource");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:childSource
		/// </summary>
		public static readonly Uri childSource = new Uri(Namespace + "childSource");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:rootNode
		/// </summary>
		public static readonly Uri rootNode = new Uri(Namespace + "rootNode");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:childNode
		/// </summary>
		public static readonly Uri childNode = new Uri(Namespace + "childNode");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:parentNode
		/// </summary>
		public static readonly Uri parentNode = new Uri(Namespace + "parentNode");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:nodeId
		/// </summary>
		public static readonly Uri nodeId = new Uri(Namespace + "nodeId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:sourceId
		/// </summary>
		public static readonly Uri sourceId = new Uri(Namespace + "sourceId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:partition
		/// </summary>
		public static readonly Uri partition = new Uri(Namespace + "partition");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:logId
		/// </summary>
		public static readonly Uri logId = new Uri(Namespace + "logId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:localId
		/// </summary>
		public static readonly Uri localId = new Uri(Namespace + "localId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:hasCommands
		/// </summary>
		public static readonly Uri hasCommands = new Uri(Namespace + "hasCommands");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:isControllable
		/// </summary>
		public static readonly Uri isControllable = new Uri(Namespace + "isControllable");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:isReadable
		/// </summary>
		public static readonly Uri isReadable = new Uri(Namespace + "isReadable");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:state
		/// </summary>
		public static readonly Uri state = new Uri(Namespace + "state");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:nodeState
		/// </summary>
		public static readonly Uri nodeState = new Uri(Namespace + "nodeState");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:typeName
		/// </summary>
		public static readonly Uri typeName = new Uri(Namespace + "typeName");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:commands
		/// </summary>
		public static readonly Uri commands = new Uri(Namespace + "commands");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:Command
		/// </summary>
		public static readonly Uri Command = new Uri(Namespace + "Command");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:commandId
		/// </summary>
		public static readonly Uri commandId = new Uri(Namespace + "commandId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:commandType
		/// </summary>
		public static readonly Uri commandType = new Uri(Namespace + "commandType");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:sortCategory
		/// </summary>
		public static readonly Uri sortCategory = new Uri(Namespace + "sortCategory");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:sortKey
		/// </summary>
		public static readonly Uri sortKey = new Uri(Namespace + "sortKey");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:success
		/// </summary>
		public static readonly Uri success = new Uri(Namespace + "success");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:failure
		/// </summary>
		public static readonly Uri failure = new Uri(Namespace + "failure");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:confirmation
		/// </summary>
		public static readonly Uri confirmation = new Uri(Namespace + "confirmation");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:dispParam
		/// </summary>
		public static readonly Uri dispParam = new Uri(Namespace + "dispParam");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:parameterId
		/// </summary>
		public static readonly Uri parameterId = new Uri(Namespace + "parameterId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:messages
		/// </summary>
		public static readonly Uri messages = new Uri(Namespace + "messages");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:eventId
		/// </summary>
		public static readonly Uri eventId = new Uri(Namespace + "eventId");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:body
		/// </summary>
		public static readonly Uri body = new Uri(Namespace + "body");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:messageType
		/// </summary>
		public static readonly Uri messageType = new Uri(Namespace + "messageType");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:operations
		/// </summary>
		public static readonly Uri operations = new Uri(Namespace + "operations");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:edit
		/// </summary>
		public static readonly Uri edit = new Uri(Namespace + "edit");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:read
		/// </summary>
		public static readonly Uri read = new Uri(Namespace + "read");

		/// <summary>
		/// urn:nf:iot:concentrator:1.0:control
		/// </summary>
		public static readonly Uri control = new Uri(Namespace + "control");
	}
}
