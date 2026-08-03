using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;
using Waher.Security.CallStack;

namespace Waher.Mcp.Xmpp
{
	/// <summary>
	/// Service module for the XMPP MCP Server service.
	/// </summary>
	public class XmppMcpServerModule : IModule
	{
		/// <summary>
		/// Service module for the XMPP MCP Server service.
		/// </summary>
		public XmppMcpServerModule()
		{
		}

		/// <summary>
		/// Starts the module.
		/// </summary>
		public Task Start()
		{
			ClientCredentials.SetAllowedSources(approvedSources);
			return Task.CompletedTask;
		}

		internal static readonly Regex FromSaveUnsavedRegex = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]((<SaveUnsaved>\w*[.]\w*)|(SaveUnsavedLocked))",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex FromUpdateObjectRegex = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]((<UpdateObject>\w*[.]\w*)|(UpdateObjectLocked))",
			RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly ICallStackCheck[] approvedSources = Assert.Convert(new object[]
		{
			typeof(XmppMcpServer),
			FromSaveUnsavedRegex,
			FromUpdateObjectRegex
		});

		/// <summary>
		/// Stops the module.
		/// </summary>
		public Task Stop()
		{
			return Task.CompletedTask;
		}
	}
}
