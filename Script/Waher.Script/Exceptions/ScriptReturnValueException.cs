using System;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime exception.
	/// </summary>
	public class ScriptReturnValueException : ScriptException 
	{
		private readonly IElement returnValue;

		/// <summary>
		/// Script runtime exception.
		/// </summary>
        /// <param name="ReturnValue">Return value.</param>
		public ScriptReturnValueException(IElement ReturnValue)
			: base(string.Empty)
		{
            this.returnValue = ReturnValue;
		}

		/// <summary>
		/// Return value.
		/// </summary>
		public IElement ReturnValue => this.returnValue;

	}
}
