using System;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Processors.Metering.NodeTypes.Errors
{
	/// <summary>
	/// Abstract base class for decision tree statement leaf nodes.
	/// </summary>
	public abstract class DecisionTreeLeafStatement : DecisionTreeStatement
	{
		/// <summary>
		/// Abstract base class for decision tree statement leaf nodes.
		/// </summary>
		public DecisionTreeLeafStatement()
			: base()
		{
			this.NodeId = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}
	}
}
