using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Things.DisplayableParameters;

namespace Waher.Reports
{
	/// <summary>
	/// Abstract base class for report nodes.
	/// </summary>
	public abstract class ReportNode : INode
	{
		private List<ReportNode> children = null;
		private readonly string nodeId;
		private readonly ReportNode parent;
		private readonly object syncObj = new object();

		/// <summary>
		/// Abstract base class for report nodes.
		/// </summary>
		/// <param name="NodeId">Node ID</param>
		/// <param name="Parent">Parent Node, or null if a root node.</param>
		public ReportNode(string NodeId, ReportNode Parent)
		{
			this.nodeId = NodeId;
			this.parent = Parent;

			this.parent?.AddChild(this);
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		public string NodeId => this.nodeId;

		/// <summary>
		/// Optional ID of source containing node.
		/// </summary>
		public string SourceId => ReportsDataSource.SourceID;

		/// <summary>
		/// Optional partition in which the Node ID is unique.
		/// </summary>
		public string Partition => string.Empty;

		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		public virtual string LocalId => this.nodeId;

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		public virtual string LogId => this.nodeId;

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		public virtual Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ReportsDataSource), 3, "Report");
		}

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		public bool HasChildren
		{
			get
			{
				lock (this.syncObj)
				{
					return (this.children?.Count ?? 0) > 0;
				}
			}
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		public bool ChildrenOrdered => false;

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public bool IsReadable => false;

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		public bool IsControllable => false;

		/// <summary>
		/// If the node has registered commands or not.
		/// </summary>
		public bool HasCommands => true;

		/// <summary>
		/// Parent Node, or null if a root node.
		/// </summary>
		public INode Parent => this.parent;

		/// <summary>
		/// When the node was last updated.
		/// </summary>
		public DateTime LastChanged => ReportsDataSource.Instance.LastChanged;

		/// <summary>
		/// Current overall state of the node.
		/// </summary>
		public NodeState State => NodeState.None;

		/// <summary>
		/// Child nodes. If no child nodes are available, null is returned.
		/// </summary>
		public Task<IEnumerable<INode>> ChildNodes
		{
			get
			{
				lock (this.syncObj)
				{
					return Task.FromResult<IEnumerable<INode>>(this.children.ToArray());
				}
			}
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		public virtual Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult(true);
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
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public virtual Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			return Task.FromResult<IEnumerable<Parameter>>(Array.Empty<Parameter>());
		}

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Set of messages.</returns>
		public Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller)
		{
			return Task.FromResult<IEnumerable<Message>>(null);
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
		/// Tries to move the node down.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved down.</returns>
		public Task<bool> MoveDownAsync(RequestOrigin Caller)
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
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Adds a new child to the node.
		/// </summary>
		/// <param name="Child">New child to add.</param>
		public Task AddAsync(INode Child)
		{
			if (!(Child is ReportNode ReportNode))
				throw new ArgumentException("Must be a report node.", nameof(Child));

			this.AddChild(ReportNode);

			return Task.CompletedTask;
		}

		private void AddChild(ReportNode ReportNode)
		{
			lock (this.syncObj)
			{
				if (this.children is null)
					this.children = new List<ReportNode>();

				if (!this.children.Contains(ReportNode))
					this.children.Add(ReportNode);
			}
		}

		/// <summary>
		/// Updates the node (in persisted storage).
		/// </summary>
		public Task UpdateAsync() => Task.CompletedTask;

		/// <summary>
		/// Removes a child from the node.
		/// </summary>
		/// <param name="Child">Child to remove.</param>
		/// <returns>If the Child node was found and removed.</returns>
		public Task<bool> RemoveAsync(INode Child)
		{
			if (!(Child is ReportNode ReportNode))
				return Task.FromResult(false);

			lock (this.syncObj)
			{
				if (this.children is null)
					return Task.FromResult(false);

				return Task.FromResult(this.children.Remove(ReportNode));
			}
		}

		/// <summary>
		/// Destroys the node. If it is a child to a parent node, it is removed from the parent first.
		/// </summary>
		public virtual Task DestroyAsync()
		{
			return Task.CompletedTask;
		}

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		public Task<IEnumerable<ICommand>> Commands => this.GetCommands();

		private async Task<IEnumerable<ICommand>> GetCommands()
		{
			ExecuteReport Command = await this.GetExecuteCommand();
			if (Command is null)
				return Array.Empty<ICommand>();
			else
				return new ICommand[] { Command };
		}

		/// <summary>
		/// Gets the command object to execute a report. If null is returned, the report
		/// node is just a group placeholder of sub-reports.
		/// </summary>
		/// <returns>Command object, or null if node is only a placeholder.</returns>
		public abstract Task<ExecuteReport> GetExecuteCommand();

	}
}