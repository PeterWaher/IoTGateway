using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Sampling capabilities structure
	/// </summary>
	public class SamplingCapabilities
	{
		/// <summary>
		/// Whether the client supports context inclusion via includeContext parameter.
		/// If not declared, servers SHOULD only use `includeContext: "none"` (or omit it).
		/// </summary>
		public object? Context { get; internal set; }

		/// <summary>
		/// Whether the client supports tool use via tools and toolChoice parameters.
		/// </summary>
		public object? Tools { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out SamplingCapabilities Typed)
		{
			SamplingCapabilities Result = new SamplingCapabilities();

			if (Generic.TryGetValue("context", out object? Obj))
				Result.Context = Obj;

			if (Generic.TryGetValue("tools", out Obj))
				Result.Tools = Obj;

			Typed = Result;
			return true;
		}
	}
}
