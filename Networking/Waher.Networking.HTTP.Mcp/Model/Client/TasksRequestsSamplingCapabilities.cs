using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Task Requests Sampling capabilities structure
	/// </summary>
	public class TasksRequestsSamplingCapabilities
	{
		/// <summary>
		/// Whether the client supports task-augmented sampling/createMessage requests.
		/// </summary>
		public object? CreateMessage { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out TasksRequestsSamplingCapabilities Typed)
		{
			TasksRequestsSamplingCapabilities Result = new TasksRequestsSamplingCapabilities();

			if (Generic.TryGetValue("createMessage", out object? Obj))
				Result.CreateMessage = Obj;

			Typed = Result;
			return true;
		}
	}
}
