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
		public static readonly Uri Namespace = new Uri("urn:ieee:iot:concentrator:1.0:");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:DataSource
		/// </summary>
		public static readonly Uri DataSource = new Uri(Namespace, "DataSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:Node
		/// </summary>
		public static readonly Uri Node = new Uri(Namespace, "Node");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:hasChildSource
		/// </summary>
		public static readonly Uri HasChildSource = new Uri(Namespace, "hasChildSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:childSource
		/// </summary>
		public static readonly Uri ChildSource = new Uri(Namespace, "childSource");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:rootNode
		/// </summary>
		public static readonly Uri RootNode = new Uri(Namespace, "rootNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:childNode
		/// </summary>
		public static readonly Uri ChildNode = new Uri(Namespace, "childNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:parentNode
		/// </summary>
		public static readonly Uri ParentNode = new Uri(Namespace, "parentNode");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:nodeId
		/// </summary>
		public static readonly Uri NodeId = new Uri(Namespace, "nodeId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sourceId
		/// </summary>
		public static readonly Uri SourceId = new Uri(Namespace, "sourceId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:partition
		/// </summary>
		public static readonly Uri Partition = new Uri(Namespace, "partition");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:logId
		/// </summary>
		public static readonly Uri LogId = new Uri(Namespace, "logId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:localId
		/// </summary>
		public static readonly Uri LocalId = new Uri(Namespace, "localId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:hasCommands
		/// </summary>
		public static readonly Uri HasCommands = new Uri(Namespace, "hasCommands");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:isControllable
		/// </summary>
		public static readonly Uri IsControllable = new Uri(Namespace, "isControllable");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:isReadable
		/// </summary>
		public static readonly Uri IsReadable = new Uri(Namespace, "isReadable");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:state
		/// </summary>
		public static readonly Uri State = new Uri(Namespace, "state");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:nodeState
		/// </summary>
		public static readonly Uri NodeState = new Uri(Namespace, "nodeState");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:typeName
		/// </summary>
		public static readonly Uri TypeName = new Uri(Namespace, "typeName");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commands
		/// </summary>
		public static readonly Uri Commands = new Uri(Namespace, "commands");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:Command
		/// </summary>
		public static readonly Uri Command = new Uri(Namespace, "Command");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commandId
		/// </summary>
		public static readonly Uri CommandId = new Uri(Namespace, "commandId");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:commandType
		/// </summary>
		public static readonly Uri CommandType = new Uri(Namespace, "commandType");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sortCategory
		/// </summary>
		public static readonly Uri SortCategory = new Uri(Namespace, "sortCategory");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:sortKey
		/// </summary>
		public static readonly Uri SortKey = new Uri(Namespace, "sortKey");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:success
		/// </summary>
		public static readonly Uri Success = new Uri(Namespace, "success");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:failure
		/// </summary>
		public static readonly Uri Failure = new Uri(Namespace, "failure");

		/// <summary>
		/// urn:ieee:iot:concentrator:1.0:confirmation
		/// </summary>
		public static readonly Uri Confirmation = new Uri(Namespace, "confirmation");
	}
}
