using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Elicitation capabilities structure
	/// </summary>
	public class ElicitationCapabilities
	{
		/// <summary>
		/// Form
		/// </summary>
		public object? Form { get; internal set; }

		/// <summary>
		/// URL
		/// </summary>
		public object? Url { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out ElicitationCapabilities Typed)
		{
			ElicitationCapabilities Result = new ElicitationCapabilities();

			if (Generic.TryGetValue("form", out object? Obj))
				Result.Form = Obj;

			if (Generic.TryGetValue("url", out Obj))
				Result.Url = Obj;

			Typed = Result;
			return true;
		}
	}
}
