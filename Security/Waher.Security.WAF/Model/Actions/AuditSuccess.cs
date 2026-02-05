using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.IO;
using Waher.Security.LoginMonitor;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Registers an audit success for an endpoint.
	/// </summary>
	public class AuditSuccess : WafAuditAction
	{
		/// <summary>
		/// Registers an audit success for an endpoint.
		/// </summary>
		public AuditSuccess()
			: base()
		{
		}

		/// <summary>
		/// Registers an audit success for an endpoint.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public AuditSuccess(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(AuditSuccess);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new AuditSuccess(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			string Message = await this.message.EvaluateAsync(State.Variables, string.Empty);
			string UserName = await this.userName.EvaluateAsync(State.Variables, string.Empty);
			string Protocol = await this.protocol.EvaluateAsync(State.Variables, "HTTP");
			KeyValuePair<string, object>[] Tags = await this.EvaluateTags(State);

			LoginAuditor.Success(Message, UserName, 
				State.Request.RemoteEndPoint.RemovePortNumber(),
				Protocol, Tags);

			return null;
		}
	}
}
