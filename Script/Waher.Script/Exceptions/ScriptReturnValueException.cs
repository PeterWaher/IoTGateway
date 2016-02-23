using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Exceptions
{
	/// <summary>
	/// Script runtime exception.
	/// </summary>
	public class ScriptReturnValueException : ScriptException 
	{
		private IElement returnValue;

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
		public IElement ReturnValue
		{
			get { return this.returnValue; }
		}

	}
}
