using System;
using System.Threading.Tasks;
using Waher.Things.Attributes;

namespace Waher.Processors.Metering.NodeTypes.Fields.Comparisons
{
	/// <summary>
	/// Abstract base class of comparison nodes.
	/// </summary>
	public abstract class ComparisonNode : ConditionNode
	{
		/// <summary>
		/// Abstract base class of comparison nodes.
		/// </summary>
		public ComparisonNode()
			: base()
		{
		}

		/// <summary>
		/// ID of node.
		/// </summary>
		[Header(25, "Comparison Operator:", 20)]
		[Page(21, "Processor", 0)]
		[ToolTip(26, "What comparison operator to use.")]
		[Required]
		[Option(ComparisonOperator.Equal, 27, "Equal to (=)")]
		[Option(ComparisonOperator.NotEqual, 28, "Not equal to (<>)")]
		[Option(ComparisonOperator.LesserThan, 29, "Lesser than (<)")]
		[Option(ComparisonOperator.LesserThanOrEqual, 30, "Lesser than or equal to (<=)")]
		[Option(ComparisonOperator.GreaterThan, 31, "Greater than (>)")]
		[Option(ComparisonOperator.GreaterThanOrEqual, 32, "Greater than or equal to (>=)")]
		public ComparisonOperator Operator { get; set; }

		/// <summary>
		/// Checks a value against the limit.
		/// </summary>
		/// <param name="Value">Value reported.</param>
		/// <param name="Limit">Limit value to check against.</param>
		/// <returns>If the condition applies.</returns>
		public Task<bool> CompareTo(IComparable Value, IComparable Limit)
		{
			int i = Value.CompareTo(Limit);

			return this.Operator switch
			{
				ComparisonOperator.Equal => Task.FromResult(i == 0),
				ComparisonOperator.NotEqual => Task.FromResult(i != 0),
				ComparisonOperator.LesserThan => Task.FromResult(i < 0),
				ComparisonOperator.LesserThanOrEqual => Task.FromResult(i <= 0),
				ComparisonOperator.GreaterThan => Task.FromResult(i > 0),
				ComparisonOperator.GreaterThanOrEqual => Task.FromResult(i >= 0),
				_ => Task.FromResult(false),
			};
		}
	}
}
