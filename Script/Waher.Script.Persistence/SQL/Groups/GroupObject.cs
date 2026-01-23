using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Groups
{
	/// <summary>
	/// Represents a collection of objects grouped together useing a GROUP BY construct.
	/// </summary>
	public class GroupObject
	{
		private readonly Variables variables;
		private readonly Dictionary<string, object> groupedValues = new Dictionary<string, object>();
		private readonly int groupCount;

		/// <summary>
		/// Represents a collection of objects grouped together useing a GROUP BY construct.
		/// </summary>
		/// <param name="GroupByValues">Grouped values that are constant in the group.</param>
		/// <param name="GroupNames">Names of the grouped values.</param>
		/// <param name="Variables">Current set of variables.</param>
		public GroupObject(object[] GroupByValues, ScriptNode[] GroupNames, Variables Variables)
		{
			this.variables = Variables;

			int i;

			this.groupCount = GroupNames.Length;
			if (GroupByValues.Length != this.groupCount)
				throw new ArgumentException("Length mismatch.", nameof(GroupByValues));

			string Name;
			ScriptNode Node;
			IElement E;

			for (i = 0; i < this.groupCount; i++)
			{
				Node = GroupNames[i];
				if (Node is null)
					continue;

				if (Node is VariableReference Ref)
					Name = Ref.VariableName;
				else
				{
					E = Node.Evaluate(Variables);	// TODO: Async
					Name = E.AssociatedObjectValue?.ToString();
				}

				if (!string.IsNullOrEmpty(Name))
					this.groupedValues[Name] = GroupByValues[i];
			}
		}

		/// <summary>
		/// Access to grouped properties.
		/// </summary>
		/// <param name="Name">Named property</param>
		/// <returns>Property value.</returns>
		public object this[string Name]
		{
			get
			{
				if (this.groupedValues.TryGetValue(Name, out object Value))
					return Value;
				else if (this.variables.TryGetVariable(Name, out Variable v))
					return v.ValueObject;
				else
					return null;
			}

			set
			{
				this.groupedValues[Name] = value;
			}
		}
	}
}
