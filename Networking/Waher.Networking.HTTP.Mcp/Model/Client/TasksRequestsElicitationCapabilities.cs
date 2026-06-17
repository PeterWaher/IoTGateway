using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Task Requests Elicitation capabilities structure
	/// </summary>
	public class TasksRequestsElicitationCapabilities
	{
		/// <summary>
		/// Whether the client supports task-augmented elicitation/create requests.
		/// </summary>
		public object? Create { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out TasksRequestsElicitationCapabilities Typed)
		{
			TasksRequestsElicitationCapabilities Result = new TasksRequestsElicitationCapabilities();

			if (Generic.TryGetValue("create", out object? Obj))
				Result.Create = Obj;

			Typed = Result;
			return true;
		}
	}
}
