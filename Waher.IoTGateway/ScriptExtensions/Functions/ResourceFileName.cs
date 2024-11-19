using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.IoTGateway.ScriptExtensions.Functions
{
	/// <summary>
	/// Returns the file name that corresponds to a local resource, taking into consideration defined web folders.
	/// </summary>
	public class ResourceFileName : FunctionOneScalarVariable
	{
		/// <summary>
		/// Returns the file name that corresponds to a local resource, taking into consideration defined web folders.
		/// </summary>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ResourceFileName(ScriptNode LocalResource, int Start, int Length, Expression Expression)
			: base(LocalResource, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ResourceFileName);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "LocalResource" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (!Gateway.HttpServer.TryGetFileName(Argument, false, out string FileName))
				throw new ScriptRuntimeException("Unable to convert local resource to file name.", this);

			return new StringValue(FileName);
		}
	}
}
