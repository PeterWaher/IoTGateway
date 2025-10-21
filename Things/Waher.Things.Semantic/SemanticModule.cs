using System;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Things.Semantic.Sources;

namespace Waher.Things.Semantic
{
	/// <summary>
	/// Module for semantic things.
	/// </summary>
	public class SemanticModule : IModule
	{
		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			Gateway.Root.FileNotFound += this.CheckDynamicGraphs;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			Gateway.Root.FileNotFound -= this.CheckDynamicGraphs;
			return Task.CompletedTask;
		}

		private async Task CheckDynamicGraphs(object Sender, FileNotFoundEventArgs e)
		{
			if (e.Exception is null)
				return; // Already processed

			e.Exception = null;
			try
			{
				HttpRequest Request = e.Request;
				RequestOrigin Caller;

				if (Request.User is IRequestOrigin Origin)
					Caller = await Origin.GetOrigin();
				else
					Caller = RequestOrigin.Empty;

				IDynamicGraph Graph = await DataSourceGraph.FindDynamicGraph(e.Request.SubPath, Caller);
				if (Graph is null)
					return;

				InMemorySemanticCube Result = new InMemorySemanticCube();
				Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.

				await Graph.GenerateGraph(Result, Language, Caller);
				await e.Response.Return(Result);
			}
			catch (Exception ex)
			{
				await e.Response.SendResponse(ex);
			}
		}
	}
}
