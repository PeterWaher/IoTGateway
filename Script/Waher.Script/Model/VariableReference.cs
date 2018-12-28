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
	public sealed class VariableReference : ScriptLeafNodeVariableReference, IDifferentiable
	{
		private readonly bool onlyVariables;

		/// <summary>
		/// Represents a variable reference.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VariableReference(string VariableName, int Start, int Length, Expression Expression)
			: base(VariableName, Start, Length, Expression)
		{
			this.onlyVariables = false;
		}

		/// <summary>
		/// Represents a variable reference.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="OnlyVariables">If only values of variables should be returned (true), or if constants and namespaces should
		/// also be included in the scope of the reference (false).</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VariableReference(string VariableName, bool OnlyVariables, int Start, int Length, Expression Expression)
			: base(VariableName, Start, Length, Expression)
		{
			this.onlyVariables = OnlyVariables;
		}

		/// <summary>
		/// If only values of variables should be returned (true), or if constants and namespaces should
		/// also be included in the scope of the reference (false).
		/// </summary>
		public bool OnlyVariables
		{
			get { return this.onlyVariables; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			if (Variables.TryGetVariable(this.variableName, out Variable v))
				return v.ValueElement;

			if (!this.onlyVariables)
			{
				if (Expression.TryGetConstant(this.variableName, out IElement ValueElement))
					return ValueElement;

				if (Types.IsRootNamespace(this.variableName))
					return new Namespace(this.variableName);

				ValueElement = Expression.GetFunctionLambdaDefinition(this.variableName, this.Start, this.Length, this.Expression);
				if (!(ValueElement is null))
					return ValueElement;

				if (Types.TryGetQualifiedNames(this.variableName, out string[] QualifiedNames))
				{
					if (QualifiedNames.Length == 1)
					{
						Type T = Types.GetType(QualifiedNames[0]);

						if (!(T is null))
							return new TypeValue(T);
						else
							return new Namespace(QualifiedNames[0]);
					}
					else
					{
						int i, c = QualifiedNames.Length;
						IElement[] Elements = new IElement[c];

						for (i = 0; i < c; i++)
							Elements[i] = new StringValue(QualifiedNames[i]);

						return new ObjectVector(Elements);
					}
				}
			}

			throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);
		}

		/// <summary>
		/// Performs a pattern match operation.
		/// </summary>
		/// <param name="CheckAgainst">Value to check against.</param>
		/// <param name="AlreadyFound">Variables already identified.</param>
		/// <returns>Pattern match result</returns>
		public override PatternMatchResult PatternMatch(IElement CheckAgainst, Dictionary<string, IElement> AlreadyFound)
		{
			if (AlreadyFound.TryGetValue(this.variableName, out IElement E) && !E.Equals(CheckAgainst))
				return PatternMatchResult.NoMatch;

			AlreadyFound[this.variableName] = CheckAgainst;

			return PatternMatchResult.Match;
		}

		/// <summary>
		/// Differentiates a script node, if possible.
		/// </summary>
		/// <param name="VariableName">Name of variable to differentiate on.</param>
		/// <param name="Variables">Collection of variables.</param>
		/// <returns>Differentiated node.</returns>
		public ScriptNode Differentiate(string VariableName, Variables Variables)
		{
			if (VariableName == this.variableName)
				return new ConstantElement(DoubleNumber.OneElement, this.Start, this.Length, this.Expression);
			else
				return new ConstantElement(DoubleNumber.ZeroElement, this.Start, this.Length, this.Expression);
		}

		/// <summary>
		/// Default variable name, if any, null otherwise.
		/// </summary>
		public string DefaultVariableName => this.variableName;

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is VariableReference O &&
				this.onlyVariables.Equals(O.onlyVariables) &&
				base.Equals(obj);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.onlyVariables.GetHashCode();
			return Result;
		}

	}
}
