using System;
using System.Collections.Generic;
using Waher.Persistence;
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
	public class CreateJwt : FunctionOneVariable
	{
        internal static readonly JwtFactory factory = new JwtFactory();

        /// <summary>
        /// Creates a Java Web Token (JWT)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        /// <param name="Expression">Expression containing script.</param>
        public CreateJwt(ScriptNode Argument, int Start, int Length, Expression Expression)
            : base(Argument, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "CreateJwt"; }
        }

        /// <summary>
        /// Default Argument names
        /// </summary>
        public override string[] DefaultArgumentNames => new string[] { "Claims" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
        {
            object Obj = Argument.AssociatedObjectValue;
            IEnumerable<KeyValuePair<string, object>> Claims;

            if (Obj is IEnumerable<KeyValuePair<string, IElement>> GenObj)
            {
                LinkedList<KeyValuePair<string, object>> List = new LinkedList<KeyValuePair<string, object>>();

                foreach (KeyValuePair<string, IElement> P in GenObj)
                    List.AddLast(new KeyValuePair<string, object>(P.Key, P.Value.AssociatedObjectValue));

                Claims = List;
            }
            else if (Obj is IEnumerable<KeyValuePair<string, object>> GenObj2)
                Claims = GenObj2;
            else
                Claims = Database.Generalize(Obj).Result;

            return new StringValue(factory.Create(Claims));
        }
    }
}
