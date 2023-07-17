using System;

namespace Waher.Things.Semantic.Ontologies
{
	/// <summary>
	/// IoT Concentrator Ontology
	/// </summary>
	public static class IoTConcentrator
	{
		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:
		/// </summary>
		public const string Namespace = "urn:ieee:iot:concentrator:1.0:";

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:DataSource
		/// </summary>
		public static readonly Uri DataSource = new Uri(Namespace + "DataSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:Node
		/// </summary>
		public static readonly Uri Node = new Uri(Namespace + "Node");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:hasChildSource
		/// </summary>
		public static readonly Uri HasChildSource = new Uri(Namespace + "hasChildSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:childSource
		/// </summary>
		public static readonly Uri ChildSource = new Uri(Namespace + "childSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:rootNode
		/// </summary>
		public static readonly Uri RootNode = new Uri(Namespace + "rootNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:childNode
		/// </summary>
		public static readonly Uri ChildNode = new Uri(Namespace + "childNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:parentNode
		/// </summary>
		public static readonly Uri ParentNode = new Uri(Namespace + "parentNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:nodeId
		/// </summary>
		public static readonly Uri NodeId = new Uri(Namespace + "nodeId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sourceId
		/// </summary>
		public static readonly Uri SourceId = new Uri(Namespace + "sourceId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:partition
		/// </summary>
		public static readonly Uri Partition = new Uri(Namespace + "partition");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:logId
		/// </summary>
		public static readonly Uri LogId = new Uri(Namespace + "logId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:localId
		/// </summary>
		public static readonly Uri LocalId = new Uri(Namespace + "localId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:hasCommands
		/// </summary>
		public static readonly Uri HasCommands = new Uri(Namespace + "hasCommands");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:isControllable
		/// </summary>
		public static readonly Uri IsControllable = new Uri(Namespace + "isControllable");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:isReadable
		/// </summary>
		public static readonly Uri IsReadable = new Uri(Namespace + "isReadable");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:state
		/// </summary>
		public static readonly Uri State = new Uri(Namespace + "state");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:nodeState
		/// </summary>
		public static readonly Uri NodeState = new Uri(Namespace + "nodeState");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:typeName
		/// </summary>
		public static readonly Uri TypeName = new Uri(Namespace + "typeName");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commands
		/// </summary>
		public static readonly Uri Commands = new Uri(Namespace + "commands");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:Command
		/// </summary>
		public static readonly Uri Command = new Uri(Namespace + "Command");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commandId
		/// </summary>
		public static readonly Uri CommandId = new Uri(Namespace + "commandId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commandType
		/// </summary>
		public static readonly Uri CommandType = new Uri(Namespace + "commandType");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sortCategory
		/// </summary>
		public static readonly Uri SortCategory = new Uri(Namespace + "sortCategory");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sortKey
		/// </summary>
		public static readonly Uri SortKey = new Uri(Namespace + "sortKey");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:success
		/// </summary>
		public static readonly Uri Success = new Uri(Namespace + "success");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:failure
		/// </summary>
		public static readonly Uri Failure = new Uri(Namespace + "failure");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:confirmation
		/// </summary>
		public static readonly Uri Confirmation = new Uri(Namespace + "confirmation");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:dispParam
		/// </summary>
		public static readonly Uri DisplayableParameters = new Uri(Namespace + "dispParam");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:parameterId
		/// </summary>
		public static readonly Uri ParameterId = new Uri(Namespace + "parameterId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:messages
		/// </summary>
		public static readonly Uri Messages = new Uri(Namespace + "messages");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:eventId
		/// </summary>
		public static readonly Uri EventId = new Uri(Namespace + "eventId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:body
		/// </summary>
		public static readonly Uri Body = new Uri(Namespace + "body");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:messageType
		/// </summary>
		public static readonly Uri MessageType = new Uri(Namespace + "messageType");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:operations
		/// </summary>
		public static readonly Uri Operations = new Uri(Namespace + "operations");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:edit
		/// </summary>
		public static readonly Uri Edit = new Uri(Namespace + "edit");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:read
		/// </summary>
		public static readonly Uri Read = new Uri(Namespace + "read");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:control
		/// </summary>
		public static readonly Uri Control = new Uri(Namespace + "control");
	}
}
