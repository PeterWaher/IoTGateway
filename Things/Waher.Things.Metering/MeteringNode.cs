using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Metering
{
	/// <summary>
	/// Base class for all metering nodes.
	/// </summary>
	[CollectionName("MeteringNodes")]
	[TypeName(TypeNameSerialization.FullName)]
	[Index("NodeId")]
	[Index("ParentId", "NodeId")]
	public abstract class MeteringNode : INode
	{
		private Guid objectId = Guid.Empty;
		private Guid parentId = Guid.Empty;
		private MeteringNode parent = null;
		private string nodeId = string.Empty;
		private NodeState state = NodeState.None;
		private List<MeteringNode> children = null;
		private bool childrenLoaded = false;
		private object synchObject = new object();
		private DateTime created = DateTime.Now;
		private DateTime updated = DateTime.MinValue;

		/// <summary>
		/// Base class for all metering nodes.
		/// </summary>
		public MeteringNode()
		{
		}

		/// <summary>
		/// Object ID in persistence layer.
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Object ID of parent node in persistence layer.
		/// </summary>
		public Guid ParentId
		{
			get { return this.parentId; }
			set { this.parentId = value; }
		}

		/// <summary>
		/// When node was created.
		/// </summary>
		public DateTime Created
		{
			get { return this.created; }
			set { this.created = value; }
		}

		/// <summary>
		/// When node was last updated. If it has not been updated, value will be <see cref="DateTime.MinValue"/>.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get { return this.updated; }
			set { this.updated = value; }
		}

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public virtual string LocalId
		{
			get { return this.NodeId; }
		}

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		public virtual string LogId
		{
			get { return this.NodeId; }
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		public string NodeId
		{
			get { return this.nodeId; }
			set { this.nodeId = value; }
		}

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		public string SourceId
		{
			get { return MeteringTopology.SourceID; }
		}

		/// <summary>
		/// Optional Type of cache in which the Node ID is unique.
		/// </summary>
		public string CacheType
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public abstract Task<string> GetTypeNameAsync(Language Language);

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren
		{
			get
			{
				if (!this.childrenLoaded)
					this.LoadChildren().Wait();

				return this.children != null;
			}
		}

		/// <summary>
		/// Child nodes. If no child nodes are available, null is returned.
		/// </summary>
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
				return this.children.ToArray();
			}
		}

		private async Task LoadChildren()
		{
			IEnumerable<MeteringNode> Children = await Database.Find<MeteringNode>(new FilterFieldEqualTo("ParentId", this.objectId));

			lock (this.synchObject)
			{
				this.children = new List<MeteringNode>();
				this.children.AddRange(Children);

				foreach (MeteringNode Child in this.children)
					Child.parent = this;

				this.SortChildrenAfterLoadLocked();
				this.childrenLoaded = true;
			}
		}

		/// <summary>
		/// Method that allows the node to sort its children, after they have been loaded.
		/// </summary>
		protected virtual void SortChildrenAfterLoadLocked()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public virtual bool ChildrenOrdered
		{
			get { return false; }
		}

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		public bool IsControllable
		{
			get
			{
				return this is IActuator;
			}
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public bool IsReadable
		{
			get
			{
				return this is ISensor;
			}
		}

		/// <summary>
		/// When the node was last updated.
		/// </summary>
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
		/// Parent Node, or null if a root node.
		/// </summary>
		public IThingReference Parent
		{
			get
			{
				if (this.parent != null)
					return this.parent;

				if (this.parentId == Guid.Empty)
					return null;

				Task<MeteringNode> T = this.LoadParent();
				T.Wait();
				this.parent = T.Result;

				if (this.parent == null)
					throw new Exception("Parent not found.");

				return this.parent;
			}
		}

		private async Task<MeteringNode> LoadParent()
		{
			foreach (MeteringNode Node in await Database.Find<MeteringNode>(new FilterFieldEqualTo("ObjectId", this.parentId)))
				return Node;

			return null;
		}

		/// <summary>
		/// Current overall state of the node.
		/// </summary>
		[DefaultValue(NodeState.None)]
		public NodeState State
		{
			get { return this.state; }
			set { this.state = value; }
		}

		/// <summary>
		/// If the node has registered commands or not.
		/// </summary>
		public virtual bool HasCommands
		{
			get { return false; }
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public virtual Task<IEnumerable<ICommand>> Commands
		{
			get
			{
				return Task.FromResult<IEnumerable<ICommand>>(null);
			}
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public abstract Task<bool> AcceptsChildAsync(INode Child);

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public abstract Task<bool> AcceptsParentAsync(INode Parent);

		/// <summary>
		/// Adds a new child to the node.
		/// </summary>
		/// <param name="Child">New child to add.</param>
		public async Task AddAsync(INode Child)
		{
			MeteringNode Node = Child as MeteringNode;
			if (Node == null)
				throw new Exception("Child must be a metering node.");

			if (this.objectId == Guid.Empty)
				throw new Exception("Parent node must be persisted before you can add nodes to it.");

			if (!this.childrenLoaded)
				await this.LoadChildren();

			Node.parentId = this.objectId;
			if (Node.objectId == Guid.Empty)
				await Database.Insert(Node);
			else
			{
				Node.updated = DateTime.Now;
				await Database.Update(Node);
			}

			lock (this.synchObject)
			{
				this.children.Add(Node);
			}

			this.RaiseUpdate();
		}

		/// <summary>
		/// Tries to move the node down.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved down.</returns>
		public Task<bool> MoveDownAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// Tries to move the node up.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved up.</returns>
		public Task<bool> MoveUpAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(false);
		}

		/// <summary>
		/// Removes a child from the node.
		/// </summary>
		/// <param name="Child">Child to remove.</param>
		/// <returns>If the Child node was found and removed.</returns>
		public async Task<bool> RemoveAsync(INode Child)
		{
			MeteringNode Node = Child as MeteringNode;
			if (Node == null)
				throw new Exception("Child must be a metering node.");

			if (!this.childrenLoaded)
				await this.LoadChildren();

			int i;

			lock (this.synchObject)
			{
				i = this.children.IndexOf(Node);
				if (i >= 0)
					this.children.RemoveAt(i);
			}

			Node.parentId = Guid.Empty;
			if (Node.objectId != Guid.Empty)
			{
				await Database.Update(Child);
				this.RaiseUpdate();
			}

			return i >= 0;
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public async virtual Task DestroyAsync()
		{
			if (this.Parent != null)
			{
				if (this.parent.childrenLoaded)
				{
					lock (this.synchObject)
					{
						this.parent.children.Remove(this);
					}
				}
			}

			if (!this.childrenLoaded)
				await this.LoadChildren();

			if (this.children != null)
			{
				foreach (MeteringNode Child in this.children)
				{
					Child.parent = null;
					Child.parentId = Guid.Empty;

					await Child.DestroyAsync();
				}

				this.children = null;
			}

			if (this.objectId != Guid.Empty)
				await Database.Delete(this);
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public virtual async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			Namespace Namespace = await Language.GetNamespaceAsync(typeof(MeteringNode).Namespace);
			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(typeof(MeteringNode).Namespace);

			LinkedList<Parameter> Result = new LinkedList<Parameter>();
			string s;

			Result.AddLast(new StringParameter("NodeId", await Namespace.GetStringAsync(1, "Node ID"), this.nodeId));
			Result.AddLast(new StringParameter("Type", await Namespace.GetStringAsync(4, "Type"), await this.GetTypeNameAsync(Language)));

			if (this.parent != null)
				Result.AddLast(new StringParameter("ParentId", await Namespace.GetStringAsync(2, "Parent ID"), this.parent.nodeId));

			if (!this.childrenLoaded)
				await this.LoadChildren();

			if (this.children != null)
			{
				int i;

				lock (this.synchObject)
				{
					i = this.children.Count;
				}

				Result.AddLast(new Int32Parameter("NrChildren", await Namespace.GetStringAsync(3, "#Children"), i));
			}

			switch (this.state)
			{
				case NodeState.Information:
					s = await Namespace.GetStringAsync(8, "Information");
					break;

				case NodeState.WarningUnsigned:
					s = await Namespace.GetStringAsync(9, "Unsigned Warning");
					break;

				case NodeState.WarningSigned:
					s = await Namespace.GetStringAsync(10, "Warning");
					break;

				case NodeState.ErrorUnsigned:
					s = await Namespace.GetStringAsync(11, "Unsigned Error");
					break;

				case NodeState.ErrorSigned:
					s = await Namespace.GetStringAsync(12, "Error");
					break;

				case NodeState.None:
				default:
					s = null;
					break;
			}

			if (!string.IsNullOrEmpty(s))
				Result.AddLast(new StringParameter("State", await Namespace.GetStringAsync(5, "State"), s));

			Result.AddLast(new DateTimeParameter("Created", await Namespace.GetStringAsync(6, "Created"), this.created));

			if (this.updated != DateTime.MinValue)
				Result.AddLast(new DateTimeParameter("Updated", await Namespace.GetStringAsync(7, "Updated"), this.updated));

			return Result;
		}

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Set of messages.</returns>
		public async Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller)
		{
			IEnumerable<MeteringMessage> Messages = await Database.Find<MeteringMessage>(
				new FilterFieldEqualTo("NodeId", this.objectId), "Timestamp");
			LinkedList<Message> Result = new LinkedList<Message>();

			foreach (MeteringMessage Msg in Messages)
				Result.AddLast(new Message(Msg.Created, Msg.Type, Msg.EventId, Msg.Body));  // TODO: Include Updated & Count also.

			return Result;
		}

		/// <summary>
		/// Logs an error message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public Task LogErrorAsync(string Body)
		{
			return this.LogMessageAsync(NodeState.ErrorUnsigned, string.Empty, Body);
		}

		/// <summary>
		/// Logs an error message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public Task LogErrorAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(NodeState.ErrorUnsigned, EventId, Body);
		}

		/// <summary>
		/// Logs an warning message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public Task LogWarningAsync(string Body)
		{
			return this.LogMessageAsync(NodeState.WarningUnsigned, string.Empty, Body);
		}

		/// <summary>
		/// Logs an warning message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public Task LogWarningAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(NodeState.WarningUnsigned, EventId, Body);
		}

		/// <summary>
		/// Logs an informational message on the node.
		/// </summary>
		/// <param name="Body">Message body.</param>
		public Task LogInformationAsync(string Body)
		{
			return this.LogMessageAsync(NodeState.Information, string.Empty, Body);
		}

		/// <summary>
		/// Logs an informational message on the node.
		/// </summary>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public Task LogInformationAsync(string EventId, string Body)
		{
			return this.LogMessageAsync(NodeState.Information, EventId, Body);
		}

		/// <summary>
		/// Logs a message on the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		/// <param name="Body">Message body.</param>
		public Task LogMessageAsync(NodeState Type, string Body)
		{
			return this.LogMessageAsync(Type, string.Empty, Body);
		}

		/// <summary>
		/// Logs a message on the node.
		/// </summary>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public async Task LogMessageAsync(NodeState Type, string EventId, string Body)
		{
			if (this.objectId == Guid.Empty)
				throw new Exception("You can only log messages on persisted nodes.");

			foreach (MeteringMessage Message in await Database.Find<MeteringMessage>(new FilterAnd(
				new FilterFieldEqualTo("NodeId", this.objectId),
				new FilterFieldEqualTo("Type", Type),
				new FilterFieldEqualTo("EventId", EventId),
				new FilterFieldEqualTo("Body", Body))))
			{
				if ((Type == NodeState.None && Message.Type == NodeState.None) ||
					(Type == NodeState.Information && Message.Type == NodeState.Information) ||
					((Type == NodeState.WarningSigned || Type == NodeState.WarningUnsigned) && (Message.Type == NodeState.WarningSigned || Message.Type == NodeState.WarningUnsigned)) ||
					((Type == NodeState.ErrorSigned || Type == NodeState.ErrorUnsigned) && (Message.Type == NodeState.ErrorSigned || Message.Type == NodeState.ErrorUnsigned)))
				{
					Message.Updated = DateTime.Now;
					Message.Count++;

					if (Type == NodeState.WarningUnsigned || Type == NodeState.ErrorUnsigned)
						Message.Type = Type;

					await Database.Update(Message);

					return;
				}
			}

			MeteringMessage Msg = new MeteringMessage(this.objectId, DateTime.Now, Type, EventId, Body);
			Msg.NodeId = this.objectId;
			await Database.Insert(Msg);

			if (Type > this.state)
			{
				this.state = Type;
				await Database.Update(this);
				this.RaiseUpdate();
			}

			switch (Type)
			{
				case NodeState.Information:
					Log.Informational(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;

				case NodeState.WarningSigned:
				case NodeState.WarningUnsigned:
					Log.Warning(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;

				case NodeState.ErrorSigned:
				case NodeState.ErrorUnsigned:
					Log.Error(Body, this.nodeId, string.Empty, EventId, EventLevel.Minor);
					break;
			}
		}

		/// <summary>
		/// Event raised when node has been updated.
		/// </summary>
		public EventHandler OnUpdate = null;

		private void RaiseUpdate()
		{
			EventHandler h = this.OnUpdate;
			if (h != null)
			{
				try
				{
					h(this, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		public Task<bool> CanAddAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);     // TODO: Check user privileges
		}

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		public Task<bool> CanDestroyAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);     // TODO: Check user privileges
		}

		/// <summary>
		/// If the node can be edited by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be edited by the caller.</returns>
		public Task<bool> CanEditAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);     // TODO: Check user privileges
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);     // TODO: Check user privileges
		}
	}
}
