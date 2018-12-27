using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;

namespace Waher.Script.Model
{
	/// <summary>
	/// Represents a variable reference.
	/// </summary>
	public abstract class ScriptLeafNodeVariableReference : ScriptLeafNode
	{
		/// <summary>
		/// Name of variable being referenced by the node.
		/// </summary>
		protected readonly string variableName;

		/// <summary>
		/// Represents a variable reference.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ScriptLeafNodeVariableReference(string VariableName, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Variable Name.
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is ScriptLeafNodeVariableReference O &&
				this.variableName.Equals(O.variableName) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.variableName.GetHashCode();
			return Result;
		}

	}
}
