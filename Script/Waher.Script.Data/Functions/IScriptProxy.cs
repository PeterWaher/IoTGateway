using Waher.Script.Model;

namespace Waher.Script.Data.Functions
{
	/// <summary>
	/// Abstract base class for script proxies used by callback functions.
	/// </summary>
	public interface IScriptProxy
	{
		/// <summary>
		/// Lambda expression to call.
		/// </summary>
		ILambdaExpression Lambda { get; }

		/// <summary>
		/// Untyped callback function.
		/// </summary>
		/// <returns>Callback function.</returns>
		object GetCallbackFunctionUntyped();
	}
}
