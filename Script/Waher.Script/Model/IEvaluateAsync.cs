using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Model
{
	/// <summary>
	/// Interface for script nodes with asynchronous evaluation
	/// </summary>
	public interface IEvaluateAsync
	{
		/// <summary>
		/// Evaluates the node asynchronously, using the variables provided in 
		/// the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		Task<IElement> EvaluateAsync(Variables Variables);
	}
}
