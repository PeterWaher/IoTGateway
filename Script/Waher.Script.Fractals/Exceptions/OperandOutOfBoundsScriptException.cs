using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Fractals.Exceptions
{
	/// <summary>
	/// Exception thrown if an image with invalid size is requested.
	/// </summary>
	public class FractalImageSizeScriptException : ScriptRuntimeException
	{
		/// <summary>
		/// Exception thrown if an image with invalid size is requested.
		/// </summary>
		/// <param name="Node">Script node where syntax error was detected.</param>
		public FractalImageSizeScriptException(ScriptNode Node)
			: base("Image size must be within 1x1 to 5000x5000", Node)
		{
		}
	}
}
