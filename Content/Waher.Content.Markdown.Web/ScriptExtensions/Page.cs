using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Networking.HTTP.ScriptExtensions
{
	/// <summary>
	/// Page-local variables.
	/// </summary>
	public class Page : IConstant
	{
		/// <summary>
		/// Page-local variables.
		/// </summary>
		public Page()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName
		{
			get { return "Page"; }
		}

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!Variables.TryGetVariable(" PageVariables ", out Variable v))
				return null;

			IElement Result = v.ValueElement;

			if (!Variables.TryGetVariable(" LastPage ", out v) ||
				!(v.ValueObject is string Url) ||
				!Variables.TryGetVariable("Request", out v) ||
				!(v.ValueObject is HttpRequest Request) ||
				string.Compare(Url, Request.Header.ResourcePart, true) != 0)
			{
				return null;
			}

			return Result;
		}

	}
}
