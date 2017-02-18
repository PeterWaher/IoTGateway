using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Runtime.Language;
using Waher.Networking.XMPP.Concentrator.Parameters;

namespace Waher.Networking.XMPP.Concentrator
{
	public enum NodeState
	{
		None,
		Information,
		WarningSigned,
		WarningUnsigned,
		ErrorSigned,
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
		/// Gets the name of data source.
		/// </summary>
		/// <param name="Language">Language to use. Can be null.</param>
		Task<string> GetNameAsync(Language Language);

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
		IEnumerable<INode> ChildNodes
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
		/// Gets available parameters.
		/// </summary>
		/// <returns>Set of parameters.</returns>
		Task<IEnumerable<Parameter>> GetParametersAsync(RequestOrigin Caller);
	}
}
