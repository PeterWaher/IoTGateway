using Waher.Content.Semantic;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SPARQL
{
	/// <summary>
	/// Type of query triple
	/// </summary>
	public enum QueryTripleType
	{
		/// <summary>
		/// Constant
		/// </summary>
		Constant = 0,

		/// <summary>
		/// Subject is a variable
		/// </summary>
		SubjectVariable = 1,

		/// <summary>
		/// Predicate is a variable
		/// </summary>
		PredicateVariable = 2,

		/// <summary>
		/// Subject and predicate are variables
		/// </summary>
		SubjectPredicateVariables = 3,

		/// <summary>
		/// Object is a variable
		/// </summary>
		ObjectVariable = 4,

		/// <summary>
		/// Subject and object are variables
		/// </summary>
		SubjectObjectVariable = 5,

		/// <summary>
		/// Predicate and object are variables
		/// </summary>
		PredicateObjectVariable = 6,

		/// <summary>
		/// Subject, predicate and object are variables
		/// </summary>
		SubjectPredicateObjectVariable = 7
	}

	/// <summary>
	/// Semantic query triple
	/// </summary>
	public class SemanticQueryTriple : ISemanticTriple
	{
		/// <summary>
		/// Semantic query triple
		/// </summary>
		/// <param name="Triple">Triple</param>
		public SemanticQueryTriple(ISemanticTriple Triple)
			: this(Triple.Subject, Triple.Predicate, Triple.Object)
		{
		}

		/// <summary>
		/// Semantic query triple
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public SemanticQueryTriple(ISemanticElement Subject, ISemanticElement Predicate,
			ISemanticElement Object)
		{
			this.Subject = Subject;
			this.Predicate = Predicate;
			this.Object = Object;

			int i = 0;

			if (Subject is SemanticScriptElement ScriptElement &&
				ScriptElement.Node is VariableReference VarRef)
			{
				this.SubjectIsVariable = true;
				this.SubjectVariable = VarRef.VariableName;
				i = 1;
			}
			else
			{
				this.SubjectIsVariable = false;
				this.SubjectVariable = null;
			}

			if (Predicate is SemanticScriptElement ScriptElement2 &&
				ScriptElement2.Node is VariableReference VarRef2)
			{
				this.PredicateIsVariable = true;
				this.PredicateVariable = VarRef2.VariableName;
				i += 2;
			}
			else
			{
				this.PredicateIsVariable = false;
				this.PredicateVariable = null;
			}

			if (Object is SemanticScriptElement ScriptElement3 &&
				ScriptElement3.Node is VariableReference VarRef3)
			{
				this.ObjectIsVariable = true;
				this.ObjectVariable = VarRef3.VariableName;
				i += 4;
			}
			else
			{
				this.ObjectIsVariable = false;
				this.ObjectVariable = null;
			}

			this.Type = (QueryTripleType)i;
		}

		/// <summary>
		/// Subject element
		/// </summary>
		public ISemanticElement Subject { get; }

		/// <summary>
		/// Predicate element
		/// </summary>
		public ISemanticElement Predicate { get; }

		/// <summary>
		/// Object element
		/// </summary>
		public ISemanticElement Object { get; }

		/// <summary>
		/// If the Subject element is a variable reference.
		/// </summary>
		public bool SubjectIsVariable { get; }

		/// <summary>
		/// If the Predicate element is a variable reference.
		/// </summary>
		public bool PredicateIsVariable { get; }

		/// <summary>
		/// If the Object element is a variable reference.
		/// </summary>
		public bool ObjectIsVariable { get; }

		/// <summary>
		/// Subject element variable name, if any
		/// </summary>
		public string SubjectVariable { get; }

		/// <summary>
		/// Predicate element variable name, if any
		/// </summary>
		public string PredicateVariable { get; }

		/// <summary>
		/// Object element variable name, if any
		/// </summary>
		public string ObjectVariable { get; }

		/// <summary>
		/// Type of triple
		/// </summary>
		public QueryTripleType Type { get; }

		/// <summary>
		/// Access to elements: 0=Subject, 1=Predicate, 2=Object.
		/// </summary>
		/// <param name="Index">Axis index.</param>
		/// <returns>Semantic element.</returns>
		public ISemanticElement this[int Index]
		{
			get
			{
				switch (Index)
				{
					case 0: return this.Subject;
					case 1: return this.Predicate;
					case 2: return this.Object;
					default: return null;
				}
			}
		}

		/// <summary>
		/// Gets a variable name, given the axis index: 0=Subject, 1=Predicate, 2=Object.
		/// </summary>
		/// <param name="Index">Axis index.</param>
		/// <returns>Variable name.</returns>
		public string VariableName(int Index)
		{
			switch (Index)
			{
				case 0: return this.SubjectVariable;
				case 1: return this.PredicateVariable;
				case 2: return this.ObjectVariable;
				default: return null;
			}
		}
	}
}
