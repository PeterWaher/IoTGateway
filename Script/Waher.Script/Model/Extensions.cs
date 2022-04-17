using System.Collections.Generic;
using Waher.Script.Exceptions;

namespace Waher.Script.Model
{
	/// <summary>
	/// Extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Sets the parent node on a set of nodes.
		/// </summary>
		/// <param name="Nodes">Set of nodes, possibly null or empty.</param>
		/// <param name="Parent">Parent Node</param>
		/// <exception cref="ScriptException">If the parent is already set, and you try to set it to another parent node.</exception>
		public static void SetParent(this IEnumerable<ScriptNode> Nodes, ScriptNode Parent)
		{
			if (!(Nodes is null))
			{
				foreach (ScriptNode N in Nodes)
					N?.SetParent(Parent);
			}
		}

		/// <summary>
		/// Calls the <see cref="ScriptNode.ForAllChildNodes(ScriptNodeEventHandler, object, bool)"/> method for all nodes in an array.
		/// </summary>
		/// <param name="Nodes">Script node array</param>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Order">Order to traverse the nodes.</param>
		/// <returns>If the process was completed.</returns>
		public static bool ForAllChildNodes(this ScriptNode[] Nodes, ScriptNodeEventHandler Callback, object State, SearchMethod Order)
		{
			if (!(Nodes is null))
			{
				int i, c = Nodes.Length;

				for (i = 0; i < c; i++)
				{
					if (!(Nodes[i]?.ForAllChildNodes(Callback, State, Order) ?? true))
						return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Calls the callback method for all nodes in an array.
		/// </summary>
		/// <param name="Nodes">Script node array</param>
		/// <param name="Callback">Callback method to call.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="IncludeChildren">If children should also be included. Corresponds to 
		/// <see cref="SearchMethod.TreeOrder"/>.</param>
		/// <returns>If the process was completed.</returns>
		public static bool ForAll(this ScriptNode[] Nodes, ScriptNodeEventHandler Callback, ScriptNode Parent, object State, bool IncludeChildren)
		{
			if (!(Nodes is null))
			{
				int i, c = Nodes.Length;
				ScriptNode Node;

				for (i = 0; i < c; i++)
				{
					Node = Nodes[i];
					if (!(Node is null))
					{
						if (!Callback(Node, out ScriptNode NewNode, State))
							return false;

						if (!(NewNode is null))
						{
							Nodes[i] = NewNode;
							NewNode.SetParent(Parent);
							Node = NewNode;
						}

						if (IncludeChildren && !Node.ForAllChildNodes(Callback, State, SearchMethod.TreeOrder))
							return false;
					}
				}
			}

			return true;
		}
	}
}
