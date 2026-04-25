using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;

namespace Waher.Things.Http
{
	/// <summary>
	/// Node representing the local web server.
	/// </summary>
	public class LocalWebServerNode : MeteringNode
	{
		/// <summary>
		/// Node representing a connection to an MQTT broker.
		/// </summary>
		public LocalWebServerNode()
			: base()
		{
		}

		/// <summary>
		/// Type name representing data.
		/// </summary>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(LocalWebServerNode), 1, "Local Web Server");
		}

		/// <summary>
		/// TODO
		/// </summary>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root);
		}
	}
}
