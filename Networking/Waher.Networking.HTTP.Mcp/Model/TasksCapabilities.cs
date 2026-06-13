using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp
{
	/// <summary>
	/// Tasks capabilities structure
	/// </summary>
	public class TasksCapabilities
	{
		/// <summary>
		/// Whether this client supports tasks/list.
		/// </summary>
		public object? List { get; internal set; }

		/// <summary>
		/// Whether this client supports tasks/cancel.
		/// </summary>
		public object? Cancel { get; internal set; }

		/// <summary>
		/// Specifies which request types can be augmented with tasks.
		/// </summary>
		public TasksRequestsCapabilities? Requests { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out TasksCapabilities Typed)
		{
			TasksCapabilities Result = new TasksCapabilities();
		
			if (Generic.TryGetValue("list", out object? Obj))
				Result.List = Obj;

			if (Generic.TryGetValue("cancel", out Obj))
				Result.Cancel = Obj;

			if (Generic.TryGetValue("requests", out Obj) &&
				Obj is Dictionary<string, object> Requests &&
				TasksRequestsCapabilities.TryParse(Requests, out TasksRequestsCapabilities RequestsParsed))
			{
				Result.Requests = RequestsParsed;
			}

			Typed = Result;
			return true;
		}
	}
}
