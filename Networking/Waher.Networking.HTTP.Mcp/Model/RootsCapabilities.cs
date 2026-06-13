using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Root capabilities structure
	/// </summary>
	public class RootsCapabilities
	{
		/// <summary>
		/// Whether the client supports notifications for changes to the roots list.
		/// </summary>
		public bool? ListChanged { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic, 
			out RootsCapabilities Typed)
		{
			RootsCapabilities Result = new RootsCapabilities();

			if (Generic.TryGetValue("listChanged", out object? Obj) &&
				Obj is bool ListChanged)
			{
				Result.ListChanged = ListChanged;
			}

			Typed = Result;
			return true;
		}
	}
}
