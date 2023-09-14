using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.DisplayableParameters;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Represents an external node.
	/// </summary>
	public class ExternalNode : INode, ISensor
	{
		private readonly string nodeId;
		private readonly string sourceId;
		private readonly string paritionId;
		private readonly string jid;

		/// <summary>
		/// Represents an external node.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="SourceId">Source ID</param>
		/// <param name="ParitionId">Partition ID</param>
		/// <param name="Jid">JID of external host.</param>
		public ExternalNode(string NodeId, string SourceId, string ParitionId, string Jid)
		{
			this.nodeId = NodeId;
			this.sourceId = SourceId;
			this.paritionId = ParitionId;
			this.jid = Jid;
		}

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public string LocalId => this.nodeId;

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		public string LogId
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.nodeId);

				if (!string.IsNullOrEmpty(this.sourceId) || !string.IsNullOrEmpty(this.paritionId))
				{
					sb.Append('/');
					sb.Append(this.sourceId);

					if (!string.IsNullOrEmpty(this.paritionId))
					{
						sb.Append('/');
						sb.Append(this.paritionId);
					}
				}

				if (!string.IsNullOrEmpty(this.jid))
				{
					sb.Append('@');
					sb.Append(this.jid);
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren => false;

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public bool ChildrenOrdered => false;

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public bool IsReadable => true;

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		public bool IsControllable => true;

		/// <summary>
		/// If the node has registered commands or not.
		/// </summary>
		public bool HasCommands => false;

		/// <summary>
		/// Parent Node, or null if a root node.
		/// </summary>
		public INode Parent => null;

		/// <summary>
		/// When the node was last updated.
		/// </summary>
		public DateTime LastChanged => DateTime.MinValue;

		/// <summary>
		/// Current overall state of the node.
		/// </summary>
		public NodeState State => NodeState.None;

		/// <summary>
		/// Child nodes. If no child nodes are available, null is returned.
		/// </summary>
		public Task<IEnumerable<INode>> ChildNodes => Task.FromResult<IEnumerable<INode>>(null);

		/// <summary>
		/// ID of node.
		/// </summary>
		public string NodeId => this.nodeId;

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		public string SourceId => this.sourceId;

		/// <summary>
		/// Optional partition in which the Node ID is unique.
		/// </summary>
		public string Partition => this.paritionId;

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Adds a new child to the node.
		/// </summary>
		/// <param name="Child">New child to add.</param>
		public Task AddAsync(INode Child)
		{
			throw new NotSupportedException("This node cannot accept child nodes.");
		}

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		public Task<bool> CanAddAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node can be destroyed to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be destroyed to by the caller.</returns>
		public Task<bool> CanDestroyAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node can be edited by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be edited by the caller.</returns>
		public Task<bool> CanEditAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public Task DestroyAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			return Task.FromResult<IEnumerable<Parameter>>(new Parameter[0]);
		}

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Set of messages.</returns>
		public Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller)
		{
			return Task.FromResult<IEnumerable<Message>>(new Message[0]);
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public Task<string> GetTypeNameAsync(Language Language)
		{
			return Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Tries to move the node down.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved down.</returns>
		public Task<bool> MoveDownAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Tries to move the node up.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved up.</returns>
		public Task<bool> MoveUpAsync(RequestOrigin Caller)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Removes a child from the node.
		/// </summary>
		/// <param name="Child">Child to remove.</param>
		/// <returns>If the Child node was found and removed.</returns>
		public Task<bool> RemoveAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Updates the node (in persisted storage).
		/// </summary>
		public Task UpdateAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public Task<IEnumerable<ICommand>> Commands => Task.FromResult<IEnumerable<ICommand>>(null);

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public Task StartReadout(ISensorReadout Request)
		{
			RosterItem Item = Gateway.XmppClient[this.jid];

			if (Item is null ||
				(Item.State != SubscriptionState.Both &&
				Item.State != SubscriptionState.To))
			{
				Gateway.XmppClient.RequestPresenceSubscription(this.jid);
				Request.ReportErrors(true, new ThingError(this, "No presence subscription approved by " + this.jid + ". New subscription request sent."));
				return Task.CompletedTask;
			}

			if (!Item.HasLastPresence || !Item.LastPresence.IsOnline)
			{
				Request.ReportErrors(true, new ThingError(this, "Sensor host not online: " + this.jid));
				return Task.CompletedTask;
			}

			INode[] Nodes;

			if (string.IsNullOrEmpty(this.nodeId))
				Nodes = null;
			else
				Nodes = new INode[] { this };

			SensorDataClientRequest SensorDataRequest = Gateway.SensorClient.RequestReadout(Item.LastPresenceFullJid,
				Nodes, Request.Types, Request.FieldNames, Request.From, Request.To, Request.When, 
				Request.ServiceToken, Request.DeviceToken, Request.UserToken);

			SensorDataRequest.OnFieldsReceived += (sender, Fields) =>
			{
				Request.ReportFields(false, Fields);
				return Task.CompletedTask;
			};

			SensorDataRequest.OnErrorsReceived += (sender, Errors) =>
			{
				Request.ReportErrors(false, Errors);
				return Task.CompletedTask;
			};

			SensorDataRequest.OnStateChanged += (sender, State) =>
			{
				switch (State)
				{
					case SensorDataReadoutState.Cancelled:
						Request.ReportErrors(true, new ThingError(this, "Readout was cancelled."));
						break;

					case SensorDataReadoutState.Done:
						Request.ReportFields(true);
						break;

					case SensorDataReadoutState.Failure:
						Request.ReportErrors(true, new ThingError(this, "Readout failed."));
						break;
				}

				return Task.CompletedTask;
			};

			return Task.CompletedTask;
		}
	}
}
