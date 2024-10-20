using System;
using Waher.Script.Model;

namespace Waher.Script.Data.Functions
{
	/// <summary>
	/// Abstract base class for script proxies used by callback functions.
	/// </summary>
	public abstract class ScriptProxy<T> : IScriptProxy
		where T : Delegate
	{
		private readonly ILambdaExpression lambda;
		private readonly Variables variables;

		/// <summary>
		/// Abstract base class for script proxies used by callback functions.
		/// </summary>
		/// <param name="Lambda">Lambda expression to call.</param>
		/// <param name="Variables">Variables collection.</param>
		public ScriptProxy(ILambdaExpression Lambda, Variables Variables)
		{
			this.lambda = Lambda;
			this.variables = Variables;
		}

		/// <summary>
		/// Lambda expression to call.
		/// </summary>
		public ILambdaExpression Lambda => this.lambda;

		/// <summary>
		/// Variables collection.
		/// </summary>
		public Variables Variables => this.variables;

		/// <summary>
		/// Gets the callback function of the specific type.
		/// </summary>
		/// <returns>Callback function.</returns>
		public abstract T GetCallbackFunction();

		/// <summary>
		/// Untyped callback function.
		/// </summary>
		/// <returns>Callbacl function.</returns>
		public object GetCallbackFunctionUntyped() => this.GetCallbackFunction();
	}
}
