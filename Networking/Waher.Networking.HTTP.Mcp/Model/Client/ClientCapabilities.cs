using System.Collections.Generic;

namespace Waher.Networking.HTTP.Mcp.Model.Client
{
	/// <summary>
	/// Capabilities of the client.
	/// </summary>
	public class ClientCapabilities
	{
		/// <summary>
		/// Experimental, non-standard capabilities that the client supports.
		/// </summary>
		public Dictionary<string, object>? Experimental { get; internal set; }

		/// <summary>
		/// Present if the client supports listing roots.
		/// </summary>
		public RootsCapabilities? Roots { get; internal set; }

		/// <summary>
		/// Present if the client supports sampling from an LLM.
		/// </summary>
		public SamplingCapabilities? Sampling { get; internal set; }

		/// <summary>
		/// Present if the client supports elicitation from the server.
		/// </summary>
		public ElicitationCapabilities? Elicitation { get; internal set; }

		/// <summary>
		/// Present if the client supports task-augmented requests.
		/// </summary>
		public TasksCapabilities? Tasks { get; internal set; }

		/// <summary>
		/// Tries to parse a generic structure into a typed structure.
		/// </summary>
		/// <param name="Generic">Generic representation.</param>
		/// <param name="Typed">Typed prepsentation.</param>
		/// <returns>If successful.</returns>
		public static bool TryParse(Dictionary<string, object> Generic,
			out ClientCapabilities Typed)
		{
			ClientCapabilities Result = new ClientCapabilities();

			if (Generic.TryGetValue("experimental", out object? Obj) &&
				Obj is Dictionary<string, object> Experimental)
			{
				Result.Experimental = Experimental;
			}

			if (Generic.TryGetValue("roots", out Obj) &&
				Obj is Dictionary<string, object> Roots &&
				RootsCapabilities.TryParse(Roots, out RootsCapabilities RootsParsed))
			{
				Result.Roots = RootsParsed;
			}

			if (Generic.TryGetValue("sampling", out Obj) &&
				Obj is Dictionary<string, object> Sampling &&
				SamplingCapabilities.TryParse(Sampling, out SamplingCapabilities SamplingParsed))
			{
				Result.Sampling = SamplingParsed;
			}

			if (Generic.TryGetValue("elicitation", out Obj) &&
				Obj is Dictionary<string, object> Elicitation &&
				ElicitationCapabilities.TryParse(Elicitation, out ElicitationCapabilities ElicitationParsed))
			{
				Result.Elicitation = ElicitationParsed;
			}

			if (Generic.TryGetValue("tasks", out Obj) &&
				Obj is Dictionary<string, object> Tasks &&
				TasksCapabilities.TryParse(Tasks, out TasksCapabilities TasksParsed))
			{
				Result.Tasks = TasksParsed;
			}

			Typed = Result;
			return true;
		}
	}
}
