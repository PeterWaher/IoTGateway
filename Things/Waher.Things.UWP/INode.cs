using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things
{
	/// <summary>
	/// State of a node.
	/// </summary>
	public enum NodeState
	{
		/// <summary>
		/// No messages, warnings or errors.
		/// </summary>
		None,

		/// <summary>
		/// Informational messages available.
		/// </summary>
		Information,

		/// <summary>
		/// Signed warnings availale.
		/// </summary>
		WarningSigned,

		/// <summary>
		/// Unsigned warnings availale.
		/// </summary>
		WarningUnsigned,

		/// <summary>
		/// Signed errors availale.
		/// </summary>
		ErrorSigned,

		/// <summary>
		/// Unsigned errors availale.
		/// </summary>
		ErrorUnsigned
	}

	/// <summary>
	/// Interface for nodes that are published through the concentrator interface.
	/// </summary>
	public interface INode : IThingReference
	{
		/// <summary>
		/// If provided, an ID for the node, but unique locally between siblings. Can be null, if Local ID equal to Node ID.
		/// </summary>
		string LocalId
		{
			get;
		}

		/// <summary>
		/// If provided, an ID for the node, as it would appear or be used in system logs. Can be null, if Log ID equal to Node ID.
		/// </summary>
		string LogId
		{
			get;
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use. Can be null.</param>
		Task<string> GetTypeNameAsync(Language Language);

		/// <summary>
		/// If the source has any child sources.
		/// </summary>
		bool HasChildren
		{
			get;
		}

		/// <summary>
		/// If the children of the node have an intrinsic order (true), or if the order is not important (false).
		/// </summary>
		bool ChildrenOrdered
		{
			get;
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		bool IsReadable
		{
			get;
		}

		/// <summary>
		/// If the node can be controlled.
		/// </summary>
		bool IsControllable
		{
			get;
		}

		/// <summary>
		/// If the node has registered commands or not.
		/// </summary>
		bool HasCommands
		{
			get;
		}

		/// <summary>
		/// Parent Node, or null if a root node.
		/// </summary>
		IThingReference Parent
		{
			get;
		}

		/// <summary>
		/// When the source was last updated.
		/// </summary>
		DateTime LastChanged
		{
			get;
		}

		/// <summary>
		/// Current overall state of the node.
		/// </summary>
		NodeState State
		{
			get;
		}

		/// <summary>
		/// Child nodes. If no child nodes are available, null is returned.
		/// </summary>
		Task<IEnumerable<INode>> ChildNodes
		{
			get;
		}

		/// <summary>
		/// If the node is visible to the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node is visible to the caller.</returns>
		Task<bool> CanViewAsync(RequestOrigin Caller);

		/// <summary>
		/// If the node can be edited by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be edited by the caller.</returns>
		Task<bool> CanEditAsync(RequestOrigin Caller);

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		Task<bool> CanAddAsync(RequestOrigin Caller);

		/// <summary>
		/// If the node can be added to by the caller.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node can be added to by the caller.</returns>
		Task<bool> CanDestroyAsync(RequestOrigin Caller);

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <returns>Set of displayable parameters.</returns>
		Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(RequestOrigin Caller);

		/// <summary>
		/// Gets messages logged on the node.
		/// </summary>
		/// <returns>Set of messages.</returns>
		Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller);

		/// <summary>
		/// Tries to move the node up.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved up.</returns>
		Task<bool> MoveUpAsync(RequestOrigin Caller);

		/// <summary>
		/// Tries to move the node down.
		/// </summary>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>If the node was moved down.</returns>
		Task<bool> MoveDownAsync(RequestOrigin Caller);

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		Task<bool> AcceptsParent(INode Parent);

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		Task<bool> AcceptsChild(INode Child);

		/// <summary>
		/// Adds a new child to the node.
		/// </summary>
		/// <param name="Child">New child to add.</param>
		Task Add(INode Child);

		/// <summary>
		/// Removes a child from the node.
		/// </summary>
		/// <param name="Child">Child to remove.</param>
		Task Remove(INode Child);

		/// <summary>
		/// Destroys the node.
		/// </summary>
		Task Destroy();

		/// <summary>
		/// Sets the parent property of the node.
		/// </summary>
		/// <param name="Parent">New parent.</param>
		Task SetParent(INode Parent);

		/// <summary>
		/// Available command objects. If no commands are available, null is returned.
		/// </summary>
		Task<IEnumerable<ICommand>> Commands
		{
			get;
		}
	}
}
