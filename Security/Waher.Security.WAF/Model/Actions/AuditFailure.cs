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
	/// Registers an audit failure for an endpoint. Repetitive audit failures within a 
	/// short time may cause the endpoint to be first temporarily blocked, then permanently
	/// blocked, depending on the configuration of the login auditor.
	/// </summary>
	public class AuditFailure : WafAuditAction
	{
		/// <summary>
		/// Registers an audit failure for an endpoint. Repetitive audit failures within a 
		/// short time may cause the endpoint to be first temporarily blocked, then permanently
		/// blocked, depending on the configuration of the login auditor.
		/// </summary>
		public AuditFailure()
			: base()
		{
		}

		/// <summary>
		/// Registers an audit failure for an endpoint. Repetitive audit failures within a 
		/// short time may cause the endpoint to be first temporarily blocked, then permanently
		/// blocked, depending on the configuration of the login auditor.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public AuditFailure(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(AuditFailure);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new AuditFailure(Xml, Parent, Document);

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

			LoginAuditor.Fail(Message, UserName, 
				State.Request.RemoteEndPoint.RemovePortNumber(), 
				Protocol, Tags);

			return null;
		}
	}
}
