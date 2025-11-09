using System;
using System.Threading.Tasks;
using Waher.Things.Attributes;

namespace Waher.Processors.Metering.NodeTypes
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
		[Header(29, "Comparison Operator:", 10)]
		[Page(21, "Processor", 0)]
		[ToolTip(30, "What comparison operator to use.")]
		[Required]
		[Option(ComparisonOperator.Equal, 31, "Equal to (=)")]
		[Option(ComparisonOperator.NotEqual, 32, "Not equal to (<>)")]
		[Option(ComparisonOperator.LesserThan, 33, "Lesser than (<)")]
		[Option(ComparisonOperator.LesserThanOrEqual, 34, "Lesser than or equal to (<=)")]
		[Option(ComparisonOperator.GreaterThan, 35, "Greater than (>)")]
		[Option(ComparisonOperator.GreaterThanOrEqual, 36, "Greater than or equal to (>=)")]
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
