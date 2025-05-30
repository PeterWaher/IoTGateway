using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Security.JWT.Functions
{
	/// <summary>
	/// Creates a Java Web Token (JWT)
	/// </summary>
	public class CreateJwt : FunctionMultiVariate
	{
		private static JwtFactory factory = null;

		/// <summary>
		/// Creates a Java Web Token (JWT)
		/// </summary>
		/// <param name="Claims">Claims.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateJwt(ScriptNode Claims, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Claims }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a Java Web Token (JWT)
		/// </summary>
		/// <param name="Claims">Claims.</param>
		/// <param name="Factory">JWT Factory.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CreateJwt(ScriptNode Claims, ScriptNode Factory, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Claims, Factory }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Scalar }, 
			Start, Length, Expression)
		{
		}

		/// <summary>
		/// Token factory used to create tokens.
		/// </summary>
		public static JwtFactory Factory
		{
			get
			{
				if (factory is null)
				{
					factory = Types.TryGetModuleParameter<JwtFactory>("JWT")
						?? JwtFactory.CreateHmacSha256();
				}
				
				return factory;
			}
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(CreateJwt);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Claims", "Factory" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			object Obj = Arguments[0].AssociatedObjectValue;
			IEnumerable<KeyValuePair<string, object>> Claims;

			if (Obj is IEnumerable<KeyValuePair<string, IElement>> GenObj)
			{
				ChunkedList<KeyValuePair<string, object>> List = new ChunkedList<KeyValuePair<string, object>>();

				foreach (KeyValuePair<string, IElement> P in GenObj)
					List.Add(new KeyValuePair<string, object>(P.Key, P.Value.AssociatedObjectValue));

				Claims = List;
			}
			else if (Obj is IEnumerable<KeyValuePair<string, object>> GenObj2)
				Claims = GenObj2;
			else
				Claims = await Database.Generalize(Obj);

			if (Arguments.Length == 1)
				return new StringValue(Factory.Create(Claims));

			if (!(Arguments[1].AssociatedObjectValue is JwtFactory Factory2))
				throw new ScriptRuntimeException("Expected a JWT Factory as second argument.", this);

			return new StringValue(Factory2.Create(Claims));
		}
	}
}
