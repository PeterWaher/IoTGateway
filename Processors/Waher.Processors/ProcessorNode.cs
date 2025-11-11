using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Processors.Commands;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Runtime.Threading;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SourceEvents;

namespace Waher.Processors
{
    /// <summary>
    /// Base class for all processor nodes.
    /// </summary>
    [CollectionName("Processors")]
	[TypeName(TypeNameSerialization.FullName)]
	[ArchivingTime]
	[Index("NodeId")]
	[Index("ParentId", "NodeId")]
	public abstract class ProcessorNode : IProcessorNode
	{
		private Guid objectId = Guid.Empty;
		private Guid parentId = Guid.Empty;
		private ProcessorNode parent = null;
		private string nodeId = string.Empty;
		private string oldId = null;
		private NodeState state = NodeState.None;
		private List<ProcessorNode> children = null;
		private bool childrenLoaded = false;
		private readonly object synchObject = new object();
		private DateTime created = DateTime.Now;
		private DateTime updated = DateTime.MinValue;
		private ThingReference thingReference = null;

		/// <summary>
		/// Base class for all processor nodes.
		/// </summary>
		public ProcessorNode()
		{
		}

		/// <summary>
		/// Converts a ProcessorNode to a ThingReference object.
		/// </summary>
		/// <param name="Node">Metering node.</param>
		public static implicit operator ThingReference(ProcessorNode Node)
		{
			Node.thingReference ??= new ThingReference(Node.nodeId, Node.SourceId, Node.Partition);

			return Node.thingReference;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is IThingReference Ref))
				return false;
			else
				return this.nodeId == Ref.NodeId && this.SourceId == Ref.SourceId && this.Partition == Ref.Partition;
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.nodeId.GetHashCode() ^
				this.SourceId.GetHashCode() ^
				this.Partition.GetHashCode();
		}

		/// <summary>
		/// Object ID in persistence layer.
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Object ID of parent node in persistence layer.
		/// </summary>
		public Guid ParentId
		{
			get => this.parentId;
			set => this.parentId = value;
		}

		/// <summary>
		/// When node was created.
		/// </summary>
		public DateTime Created
		{
			get => this.created;
			set => this.created = value;
		}

		/// <summary>
		/// When node was last updated. If it has not been updated, value will be <see cref="DateTime.MinValue"/>.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get => this.updated;
			set => this.updated = value;
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(14, "ID:", 0)]
		[Page(15, "Processor", 0)]
		[ToolTip(16, "Processor identity in the collection of processors.")]
		[Required]
		public string NodeId
		{
			get => this.nodeId;
			set
			{
				this.nodeId = value;
				this.thingReference = null;

				if (this.oldId is null && !string.IsNullOrEmpty(value))
					this.oldId = value;
			}
		}

		/// <summary>
		/// Previous ID of node (during update).
		/// </summary>
		protected string OldId => this.oldId;

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		[IgnoreMember]
		public string SourceId => ProcessorSource.SourceID;

		/// <summary>
		/// Optional partition in which the Node ID is unique.
		/// </summary>
		[IgnoreMember]
		public string Partition => string.Empty;

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			Language Language = Translator.GetDefaultLanguageAsync().Result;

			sb.Append(this.nodeId);
			sb.Append(" (");
			sb.Append(this.GetTypeNameAsync(Language).Result);
			sb.Append(")");

			foreach (Parameter P in this.GetDisplayableParametersAsync(Language, RequestOrigin.Empty).Result)
			{
				sb.Append(", ");
				sb.Append(P.Name);
				sb.Append("=");
				sb.Append(P.StringValue);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Logs an error message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public virtual Task LogErrorAsync(string Body)
		{
			return this.LogMessageAsync(MessageType.Error, string.Empty, Body);
		}

		/// <summary>
		/// Logs an error message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public virtual Task LogErrorAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(MessageType.Error, EventId, Body);
		}

		/// <summary>
		/// Logs an warning message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public virtual Task LogWarningAsync(string Body)
		{
			return this.LogMessageAsync(MessageType.Warning, string.Empty, Body);
		}

		/// <summary>
		/// Logs an warning message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public virtual Task LogWarningAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(MessageType.Warning, EventId, Body);
		}

		/// <summary>
		/// Logs an informational message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public virtual Task LogInformationAsync(string Body)
		{
			return this.LogMessageAsync(MessageType.Information, string.Empty, Body);
		}

		/// <summary>
		/// Logs an informational message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public virtual Task LogInformationAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(MessageType.Information, EventId, Body);
		}

		/// <summary>
		/// Logs a message on the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		/// <param name="Body">Message body.</param>
		public virtual Task LogMessageAsync(MessageType Type, string Body)
		{
			return this.LogMessageAsync(Type, string.Empty, Body);
		}

		/// <summary>
		/// Logs a message on the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public virtual async Task LogMessageAsync(MessageType Type, string EventId, string Body)
		{
			if (this.objectId == Guid.Empty)
				return;

			bool Updated = false;

			foreach (ProcessorMessage Message in await Database.Find<ProcessorMessage>(new FilterAnd(
				new FilterFieldEqualTo("NodeId", this.objectId),
				new FilterFieldEqualTo("Type", Type),
				new FilterFieldEqualTo("EventId", EventId),
				new FilterFieldEqualTo("Body", Body))))
			{
				Message.Updated = DateTime.Now;
				Message.Count++;

				await Database.Update(Message);
				Updated = true;

				break;
			}

			if (!Updated)
			{
				ProcessorMessage Msg = new ProcessorMessage(this.objectId, DateTime.Now, Type, EventId, Body)
				{
					NodeId = this.objectId
				};

				await Database.Insert(Msg);
			}

			switch (Type)
			{
				case MessageType.Error:
					if (this.state < NodeState.ErrorUnsigned)
					{
						this.state = NodeState.ErrorUnsigned;
						await Database.Update(this);
						await this.RaiseUpdate();
					}
					break;

				case MessageType.Warning:
					if (this.state < NodeState.WarningUnsigned)
					{
						this.state = NodeState.WarningUnsigned;
						await Database.Update(this);
						await this.RaiseUpdate();
					}
					break;

				case MessageType.Information:
					if (this.state < NodeState.Information)
					{
						this.state = NodeState.Information;
						await Database.Update(this);
						await this.RaiseUpdate();
					}
					break;
			}

			switch (Type)
			{
				case MessageType.Information:
					Log.Informational(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;

				case MessageType.Warning:
					Log.Warning(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;

				case MessageType.Error:
					Log.Error(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;
			}

			await this.NodeStateChanged();
		}

		internal async Task NodeStateChanged()
		{
			await ProcessorSource.NewEvent(new NodeStatusChanged()
			{
				Messages = await this.GetMessageArrayAsync(RequestOrigin.Empty),
				State = this.state,
				NodeId = this.NodeId,
				Partition = this.Partition,
				SourceId = this.SourceId,
				Timestamp = DateTime.Now
			});
		}

		/// <summary>
		/// Removes error messages with an empty event ID from the node.
		/// </summary>
		public virtual Task<bool> RemoveErrorAsync()
		{
			return this.RemoveMessageAsync(MessageType.Error, string.Empty);
		}

		/// <summary>
		/// Removes error messages with a given event ID from the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		public virtual Task<bool> RemoveErrorAsync(string EventId)
		{
			return this.RemoveMessageAsync(MessageType.Error, EventId);
		}

		/// <summary>
		/// Removes warning messages with an empty event ID from the node.
		/// </summary>
		public virtual Task<bool> RemoveWarningAsync()
		{
			return this.RemoveMessageAsync(MessageType.Warning, string.Empty);
		}

		/// <summary>
		/// Removes warning messages with a given event ID from the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		public virtual Task<bool> RemoveWarningAsync(string EventId)
		{
			return this.RemoveMessageAsync(MessageType.Warning, EventId);
		}

		/// <summary>
		/// Removes warning messages with an empty event ID from the node.
		/// </summary>
		public virtual Task<bool> RemoveInformationAsync()
		{
			return this.RemoveMessageAsync(MessageType.Information, string.Empty);
		}

		/// <summary>
		/// Removes an informational message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		public virtual Task<bool> RemoveInformationAsync(string EventId)
		{
			return this.RemoveMessageAsync(MessageType.Information, EventId);
		}

		/// <summary>
		/// Removes messages with empty event IDs from the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		public virtual Task<bool> RemoveMessageAsync(MessageType Type)
		{
			return this.RemoveMessageAsync(Type, string.Empty);
		}

		/// <summary>
		/// Logs a message on the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		public virtual async Task<bool> RemoveMessageAsync(MessageType Type, string EventId)
		{
			if (this.objectId == Guid.Empty)
				return false;

			bool Removed = false;

			foreach (ProcessorMessage Message in await Database.FindDelete<ProcessorMessage>(new FilterAnd(
				new FilterFieldEqualTo("NodeId", this.objectId),
				new FilterFieldEqualTo("Type", Type),
				new FilterFieldEqualTo("EventId", EventId))))
			{
				Removed = true;

				switch (Type)
				{
					case MessageType.Error:
						Log.Informational("Error removed: " + Message.Body, this.nodeId, string.Empty, string.Empty, EventLevel.Minor);
						break;

					case MessageType.Warning:
						Log.Informational("Warning removed: " + Message.Body, this.nodeId, string.Empty, string.Empty, EventLevel.Minor);
						break;
				}
			}

			if (Removed)
			{
				bool ErrorsFound = false;
				bool WarningsFound = false;
				bool InformationFound = false;

				foreach (ProcessorMessage Message in await Database.Find<ProcessorMessage>(new FilterFieldEqualTo("NodeId", this.objectId)))
				{
					switch (Type)
					{
						case MessageType.Error:
							ErrorsFound = true;
							break;

						case MessageType.Warning:
							WarningsFound = true;
							break;

						case MessageType.Information:
							InformationFound = true;
							break;
					}
				}

				NodeState NewStateSigned;
				NodeState NewStateUnsigned;

				if (ErrorsFound)
				{
					NewStateSigned = NodeState.ErrorSigned;
					NewStateUnsigned = NodeState.ErrorUnsigned;
				}
				else if (WarningsFound)
				{
					NewStateSigned = NodeState.WarningSigned;
					NewStateUnsigned = NodeState.WarningUnsigned;
				}
				else if (InformationFound)
				{
					NewStateSigned = NodeState.Information;
					NewStateUnsigned = NodeState.Information;
				}
				else 
				{
					NewStateSigned = NodeState.None;
					NewStateUnsigned = NodeState.None;
				}

				switch (this.state)
				{
					case NodeState.ErrorSigned:
					case NodeState.WarningSigned:
						if (this.state != NewStateSigned)
						{
							this.state = NewStateSigned;
							await Database.Update(this);
							await this.RaiseUpdate();
						}
						break;

					default:
						if (this.state != NewStateUnsigned)
						{
							this.state = NewStateUnsigned;
							await Database.Update(this);
							await this.RaiseUpdate();
						}
						break;
				}

				await this.NodeStateChanged();
			}

			return Removed;
		}

		/// <summary>
		/// Event raised when node has been updated.
		/// </summary>
		public event EventHandlerAsync OnUpdate = null;

		internal Task RaiseUpdate()
		{
			return this.OnUpdate.Raise(this, EventArgs.Empty);
		}

		/// <summary>
		/// Gets a Processor ID, based on <paramref name="ProcessorId"/> that is not already available in the database.
		/// </summary>
		/// <param name="ProcessorId">Desired Processor ID</param>
		/// <returns>A Processor ID that does not exist in the database.</returns>
		public static async Task<string> GetUniqueNodeId(string ProcessorId)
		{
			using Semaphore Semaphore = await Semaphores.BeginWrite("Processors." + ProcessorId);
			string Suffix = string.Empty;
			string s;
			int i = 1;

			while (true)
			{
				if (await Database.FindFirstIgnoreRest<ProcessorNode>(
					new FilterFieldEqualTo("NodeId", s = ProcessorId + Suffix)) is null)
				{
					return s;
				}

				i++;
				Suffix = " (" + i.ToString() + ")";
			}
		}

		#region INode

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		[IgnoreMember]
		public virtual string LocalId => this.NodeId;

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		[IgnoreMember]
		public virtual string LogId => this.NodeId;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public abstract Task<string> GetTypeNameAsync(Language Language);

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		[IgnoreMember]
		public bool HasChildren
		{
			get
			{
				if (!this.childrenLoaded)
					this.LoadChildren().Wait();

				return !(this.children is null) && this.children.Count > 0;
			}
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public virtual bool ChildrenOrdered => false;

		/// <summary>
		/// If the node can be read.
		/// </summary>
		[IgnoreMember]
		public virtual bool IsReadable => this is ISensor;

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		[IgnoreMember]
		public virtual bool IsControllable => this is IActuator;

		/// <summary>
		/// If the node has registered commands or not.
		/// </summary>
		[IgnoreMember]
		public virtual bool HasCommands => true;

		/// <summary>
		/// Parent Node, or null if a root node.
		/// </summary>
		[IgnoreMember]
		[Obsolete("Use the asynchronous GetParent() method instead.")]
		public INode Parent => this.GetParent().Result;

		/// <summary>
		/// Gets the parent of the node.
		/// </summary>
		/// <returns>Parent instance.</returns>
		/// <exception cref="Exception">If parent is not found.</exception>
		public async Task<INode> GetParent()
		{
			if (!(this.parent is null))
				return this.parent;

			if (this.parentId == Guid.Empty)
				return null;

			this.parent = await this.LoadParent();
			if (this.parent is null)
				throw new Exception("Parent not found.");

			return this.parent;
		}

		/// <summary>
		/// Tries to get an ancestor node of a given type, if one exists.
		/// </summary>
		/// <returns>Ancestor Node of given type, if found, null otherwise.</returns>
		public async Task<T> GetAncestor<T>()
			where T : INode
		{
			INode Loop = await this.GetParent();

			while (!(Loop is null))
			{
				if (Loop is T Ancestor)
					return Ancestor;
				else if (Loop is ProcessorNode ProcessorNode)
					Loop = await ProcessorNode.GetParent();
				else
					Loop = Loop.Parent;
			}

			return default;
		}

		/// <summary>
		/// When the node was last updated.
		/// </summary>
		[IgnoreMember]
		public DateTime LastChanged
		{
			get
			{
				if (this.updated == DateTime.MinValue)
					return this.created;
				else
					return this.updated;
			}
		}

		/// <summary>
		/// Current overall state of the node.
		/// </summary>
		[DefaultValue(NodeState.None)]
		public NodeState State
		{
			get => this.state;
			set => this.state = value;
		}

		/// <summary>
		/// Child nodes. If no child nodes are available, null is returned.
		/// </summary>
		[IgnoreMember]
		public Task<IEnumerable<INode>> ChildNodes
		{
			get
			{
				return this.GetChildNodes();
			}
		}

		private async Task<IEnumerable<INode>> GetChildNodes()
		{
			if (!this.childrenLoaded)
				await this.LoadChildren();

			lock (this.synchObject)
			{
				if (this.children is null)
					return Array.Empty<INode>();
				else
					return this.children.ToArray();
			}
		}

		private async Task LoadChildren()
		{
			IEnumerable<ProcessorNode> Children = await Database.Find<ProcessorNode>(new FilterFieldEqualTo("ParentId", this.objectId));
			LinkedList<ProcessorNode> Children2 = new LinkedList<ProcessorNode>();

			foreach (ProcessorNode Node in Children)
				Children2.AddLast(ProcessorSource.RegisterNode(Node));

			lock (this.synchObject)
			{
				this.children = null;

				foreach (ProcessorNode Child in Children2)
				{
					this.children ??= new List<ProcessorNode>();
					this.children.Add(Child);
					
					Child.parent = this;
				}

				this.SortChildrenAfterLoadLocked(this.children);
				this.childrenLoaded = true;
			}
		}

		/// <summary>
		/// Method that allows the node to sort its children, after they have been loaded.
		/// </summary>
		/// <param name="Children">Loaded children.</param>
		protected virtual void SortChildrenAfterLoadLocked(List<ProcessorNode> Children)
		{
			// Do nothing by default.
		}

		internal async Task<ProcessorNode> LoadParent()
		{
			if (!(this.parent is null))
				return this.parent;

			if (this.parentId == Guid.Empty)
				return null;

			this.parent = await Database.LoadObject<ProcessorNode>(this.parentId);
			ProcessorSource.RegisterNode(this.parent);

			return this.parent;
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		public virtual Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult(Caller.HasPrivilege("Source." + ProcessorSource.SourceID + ".Node.View"));
		}

		/// <summary>
		/// If the node can be edited by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be edited by the caller.</returns>
		public virtual Task<bool> CanEditAsync(RequestOrigin Caller)
		{
			return Task.FromResult(Caller.HasPrivilege("Source." + ProcessorSource.SourceID + ".Node.Edit"));
		}

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		public virtual Task<bool> CanAddAsync(RequestOrigin Caller)
		{
			return Task.FromResult(Caller.HasPrivilege("Source." + ProcessorSource.SourceID + ".Node.Add"));
		}

		/// <summary>
		/// If the node can be destroyed to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be destroyed to by the caller.</returns>
		public virtual Task<bool> CanDestroyAsync(RequestOrigin Caller)
		{
			return Task.FromResult(Caller.HasPrivilege("Source." + ProcessorSource.SourceID + ".Node.Destroy"));
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public virtual async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			Namespace Namespace = await Language.GetNamespaceAsync(typeof(ProcessorNode).Namespace)
				?? await Language.CreateNamespaceAsync(typeof(ProcessorNode).Namespace);

			LinkedList<Parameter> Result = new LinkedList<Parameter>();
			Result.AddLast(new StringParameter("NodeId", await Namespace.GetStringAsync(1, "Processor ID"), this.nodeId));
			Result.AddLast(new StringParameter("Type", await Namespace.GetStringAsync(4, "Type"), await this.GetTypeNameAsync(Language)));

			if (!(this.parent is null))
				Result.AddLast(new StringParameter("ParentId", await Namespace.GetStringAsync(2, "Parent ID"), this.parent.nodeId));

			if (!this.childrenLoaded)
				await this.LoadChildren();

			if (!(this.children is null))
			{
				int i;

				lock (this.synchObject)
				{
					i = this.children.Count;
				}

				Result.AddLast(new Int32Parameter("NrChildren", await Namespace.GetStringAsync(3, "#Children"), i));
			}

			string s = this.state switch
			{
				NodeState.Information => await Namespace.GetStringAsync(8, "Information"),
				NodeState.WarningUnsigned => await Namespace.GetStringAsync(9, "Unsigned Warning"),
				NodeState.WarningSigned => await Namespace.GetStringAsync(10, "Warning"),
				NodeState.ErrorUnsigned => await Namespace.GetStringAsync(11, "Unsigned Error"),
				NodeState.ErrorSigned => await Namespace.GetStringAsync(12, "Error"),
				_ => null,
			};

			if (!string.IsNullOrEmpty(s))
				Result.AddLast(new StringParameter("State", await Namespace.GetStringAsync(5, "State"), s));

			Result.AddLast(new DateTimeParameter("Created", await Namespace.GetStringAsync(6, "Created"), this.created));

			if (this.updated != DateTime.MinValue)
				Result.AddLast(new DateTimeParameter("Updated", await Namespace.GetStringAsync(7, "Updated"), this.updated));

			return Result;
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async Task<Parameter[]> GetDisplayableParameterAraryAsync(Language Language, RequestOrigin Caller)
		{
			ChunkedList<Parameter> Result = new ChunkedList<Parameter>();
			Result.AddRange(await this.GetDisplayableParametersAsync(Language, Caller));
			return Result.ToArray();
		}

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Set of messages.</returns>
		public virtual async Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller)
		{
			IEnumerable<ProcessorMessage> Messages = await Database.Find<ProcessorMessage>(
				new FilterFieldEqualTo("NodeId", this.objectId), "Created");
			LinkedList<Message> Result = new LinkedList<Message>();

			foreach (ProcessorMessage Msg in Messages)
				Result.AddLast(new Message(Msg.Created, Msg.Type, Msg.EventId, Msg.Body));  // TODO: Include Updated & Count also.

			return Result;
		}

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Array of messages.</returns>
		public async Task<Message[]> GetMessageArrayAsync(RequestOrigin Caller)
		{
			List<Message> Result = new List<Message>();

			foreach (Message Msg in await this.GetMessagesAsync(Caller))
				Result.Add(Msg);

			return Result.ToArray();
		}

		/// <summary>
		/// Tries to move the node up.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved up.</returns>
		public virtual async Task<bool> MoveUpAsync(RequestOrigin Caller)
		{
			if (!(await this.GetParent() is ProcessorNode Parent))
				return false;
			else
				return await Parent.MoveUpAsync(this, Caller);
		}

		/// <summary>
		/// Tries to move the node down.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved down.</returns>
		public virtual async Task<bool> MoveDownAsync(RequestOrigin Caller)
		{
			if (!(await this.GetParent() is ProcessorNode Parent))
				return false;
			else
				return await Parent.MoveDownAsync(this, Caller);
		}

		/// <summary>
		/// Tries to move the child node up.
		/// </summary>
		/// <param name="Child">Child node to move.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the child node was moved up.</returns>
		public virtual async Task<bool> MoveUpAsync(ProcessorNode Child, RequestOrigin Caller)
		{
			if (!this.ChildrenOrdered || this.children is null)
				return false;

			if (!await this.CanEditAsync(Caller) || !await Child.CanEditAsync(Caller))
				return false;

			lock (this.children)
			{
				int i = this.children.IndexOf(Child);
				if (i <= 0)
					return false;

				this.children.RemoveAt(i);
				this.children.Insert(i - 1, Child);
			}

			await ProcessorSource.NewEvent(new NodeMovedUp()
			{
				NodeId = Child.NodeId,
				Partition = Child.Partition,
				SourceId = Child.SourceId,
				Timestamp = DateTime.Now
			});

			return true;
		}

		/// <summary>
		/// Tries to move the child node down.
		/// </summary>
		/// <param name="Child">Child node to move.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the child node was moved down.</returns>
		public virtual async Task<bool> MoveDownAsync(ProcessorNode Child, RequestOrigin Caller)
		{
			if (!this.ChildrenOrdered || this.children is null)
				return false;

			if (!await this.CanEditAsync(Caller) || !await Child.CanEditAsync(Caller))
				return false;

			lock (this.children)
			{
				int c = this.children.Count;
				int i = this.children.IndexOf(Child);
				if (i < 0 || i + 1 >= c)
					return false;

				this.children.RemoveAt(i);
				this.children.Insert(i + 1, Child);
			}

			await ProcessorSource.NewEvent(new NodeMovedDown()
			{
				NodeId = Child.NodeId,
				Partition = Child.Partition,
				SourceId = Child.SourceId,
				Timestamp = DateTime.Now
			});

			return true;
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public abstract Task<bool> AcceptsParentAsync(INode Parent);

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public abstract Task<bool> AcceptsChildAsync(INode Child);

		/// <summary>
		/// Adds a new child to the node.
		/// </summary>
		/// <param name="Child">New child to add.</param>
		public virtual async Task AddAsync(INode Child)
		{
			if (!(Child is ProcessorNode Node))
				throw new Exception("Child must be a processor node.");

			if (this.objectId == Guid.Empty)
				throw new Exception("Parent node must be persisted before you can add nodes to it.");

			if (!this.childrenLoaded)
				await this.LoadChildren();

			Node.parentId = this.objectId;

			ProcessorNode After = null;
			int c;

			lock (this.synchObject)
			{
				if (this.children is null)
					this.children = new List<ProcessorNode>();
				else if ((c = this.children.Count) > 0)
					After = this.children[c - 1];

				this.children.Add(Node);
				Node.parent = this;
			}

			if (Node.objectId == Guid.Empty)
			{
				await Database.Insert(Node);
				ProcessorSource.RegisterNode(Node);

				Language Language = await Translator.GetDefaultLanguageAsync();
				NodeAdded Event = new NodeAdded()
				{
					Parameters = await Node.GetDisplayableParameterAraryAsync(Language, RequestOrigin.Empty),
					NodeType = Node.GetType().FullName,
					Sniffable = false,
					DisplayName = await Node.GetTypeNameAsync(Language),
					HasChildren = Node.HasChildren,
					ChildrenOrdered = Node.ChildrenOrdered,
					IsReadable = Node.IsReadable,
					IsControllable = Node.IsControllable,
					HasCommands = Node.HasCommands,
					ParentId = this.NodeId,
					ParentPartition = this.Partition,
					Updated = Node.Updated,
					State = Node.State,
					NodeId = Node.NodeId,
					Partition = Node.Partition,
					LogId = NodeAdded.EmptyIfSame(Node.LogId, Node.NodeId),
					LocalId = NodeAdded.EmptyIfSame(Node.LocalId, Node.NodeId),
					SourceId = Node.SourceId,
					Timestamp = DateTime.Now
				};

				if (this.ChildrenOrdered && !(After is null))
				{
					Event.AfterNodeId = After.nodeId;
					Event.AfterPartition = After.Partition;
				}

				await ProcessorSource.NewEvent(Event);
			}
			else
				await Node.NodeUpdated();

			await this.RaiseUpdate();
		}

		/// <summary>
		/// Persists changes to the node, and generates a node updated event.
		/// </summary>
		protected virtual async Task NodeUpdated()
		{
			this.updated = DateTime.Now;
			await Database.Update(this);

			await ProcessorSource.NewEvent(new NodeUpdated()
			{
				Parameters = await this.GetDisplayableParameterAraryAsync(await Translator.GetDefaultLanguageAsync(), RequestOrigin.Empty),
				HasChildren = this.HasChildren,
				ChildrenOrdered = this.ChildrenOrdered,
				IsReadable = this.IsReadable,
				IsControllable = this.IsControllable,
				HasCommands = this.HasCommands,
				ParentId = this.NodeId,
				ParentPartition = this.Partition,
				Updated = this.Updated,
				State = this.State,
				NodeId = this.NodeId,
				OldId = this.oldId,
				Partition = this.Partition,
				LogId = NodeAdded.EmptyIfSame(this.LogId, this.NodeId),
				LocalId = NodeAdded.EmptyIfSame(this.LocalId, this.NodeId),
				SourceId = this.SourceId,
				Timestamp = DateTime.Now
			});

			if (this.oldId != this.nodeId)
			{
				ProcessorSource.RegisterNewNodeId(this, this.oldId);
				this.oldId = this.nodeId;
			}
		}

		/// <summary>
		/// Updates the node (in persisted storage).
		/// </summary>
		public virtual async Task UpdateAsync()
		{
			if (this.objectId != Guid.Empty)
				await this.NodeUpdated();

			await this.RaiseUpdate();
		}

		/// <summary>
		/// Removes a child from the node.
		/// </summary>
		/// <param name="Child">Child to remove.</param>
		/// <returns>If the Child node was found and removed.</returns>
		public virtual async Task<bool> RemoveAsync(INode Child)
		{
			if (!(Child is ProcessorNode Node))
				throw new Exception("Child must be a processor node.");

			if (!this.childrenLoaded)
				await this.LoadChildren();

			int i;

			lock (this.synchObject)
			{
				if (!(this.children is null))
				{
					i = this.children.IndexOf(Node);
					if (i >= 0)
					{
						this.children.RemoveAt(i);
						if (i == 0 && this.children.Count == 0)
							this.children = null;
					}
				}
				else
					i = -1;
			}

			Node.parentId = Guid.Empty;
			Node.parent = null;

			if (Node.objectId != Guid.Empty)
			{
				await Database.Update(Child);
				await this.RaiseUpdate();

				await ProcessorSource.NewEvent(new NodeRemoved()
				{
					NodeId = Node.NodeId,
					Partition = Node.Partition,
					SourceId = Node.SourceId,
					Timestamp = DateTime.Now
				});
			}

			return i >= 0;
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public async virtual Task DestroyAsync()
		{
			if (!(await this.GetParent() is null))
			{
				if (this.parent.childrenLoaded)
				{
					lock (this.parent.synchObject)
					{
						if (!(this.parent.children is null))
						{
							this.parent.children.Remove(this);
							if (this.parent.children.Count == 0)
								this.parent.children = null;
						}
					}
				}
			}

			if (!this.childrenLoaded)
				await this.LoadChildren();

			if (!(this.children is null))
			{
				foreach (ProcessorNode Child in this.children)
				{
					Child.parent = null;
					Child.parentId = Guid.Empty;

					await Child.DestroyAsync();
				}

				this.children = null;
			}

			if (this.objectId != Guid.Empty)
			{
				await Database.Delete(this);
				this.objectId = Guid.Empty;
			}

			ProcessorSource.UnregisterNode(this);
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		[IgnoreMember]
		public virtual Task<IEnumerable<ICommand>> Commands
		{
			get
			{
				return Task.FromResult<IEnumerable<ICommand>>(new ICommand[]
				{
					new ClearMessages(this),
					new LogMessage(this)
				});
			}
		}

		#endregion
	}
}
