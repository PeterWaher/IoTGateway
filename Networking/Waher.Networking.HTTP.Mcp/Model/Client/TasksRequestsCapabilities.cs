using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Tasks Requests capabilities structure
	/// </summary>
	public class TasksRequestsCapabilities
	{
		/// <summary>
		/// Task support for sampling-related requests.
		/// </summary>
		public TasksRequestsSamplingCapabilities? Sampling { get; internal set; }

		/// <summary>
		/// Task support for elicitation-related requests.
		/// </summary>
		public TasksRequestsElicitationCapabilities? Elicitation { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out TasksRequestsCapabilities Typed)
		{
			TasksRequestsCapabilities Result = new TasksRequestsCapabilities();

			if (Generic.TryGetValue("requests", out object? Obj) &&
				Obj is Dictionary<string, object> Requests)
			{
				if (Requests.TryGetValue("sampling", out Obj) &&
					Obj is Dictionary<string, object> TasksRequestsSampling &&
					TasksRequestsSamplingCapabilities.TryParse(TasksRequestsSampling, out TasksRequestsSamplingCapabilities TasksRequestsSamplingParsed))
				{
					Result.Sampling = TasksRequestsSamplingParsed;
				}

				if (Requests.TryGetValue("elicitation", out Obj) &&
					Obj is Dictionary<string, object> TasksRequestsElicitation &&
					TasksRequestsElicitationCapabilities.TryParse(TasksRequestsElicitation, out TasksRequestsElicitationCapabilities TasksRequestsElicitationParsed))
				{
					Result.Elicitation = TasksRequestsElicitationParsed;
				}
			}

			Typed = Result;
			return true;
		}
	}
}
