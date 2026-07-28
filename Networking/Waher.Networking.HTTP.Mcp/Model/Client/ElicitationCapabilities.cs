using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Elicitation capabilities structure
	/// </summary>
	public class ElicitationCapabilities
	{
		/// <summary>
		/// Form
		/// </summary>
		public bool Form { get; internal set; }

		/// <summary>
		/// URL
		/// </summary>
		public bool Url { get; internal set; }

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
				Result.Form = !(Obj is null);

			if (Generic.TryGetValue("url", out Obj))
				Result.Url = !(Obj is null);

			if (!Result.Form && !Result.Url)
				Result.Form = true;

			Typed = Result;
			return true;
		}
	}
}
