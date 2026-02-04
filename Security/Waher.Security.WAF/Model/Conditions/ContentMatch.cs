using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.HTTP.Interfaces;
using Waher.Runtime.IO;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Checks for a match against the Content of the request.
	/// </summary>
	public class ContentMatch : WafCondition
	{
		/// <summary>
		/// Checks for a match against the Content of the request.
		/// </summary>
		public ContentMatch()
			: base()
		{
		}

		/// <summary>
		/// Checks for a match against the Content of the request.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public ContentMatch(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(ContentMatch);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new ContentMatch(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			if (!State.Request.HasData)
				return null;

			if (State.ContentAsString is null)
			{
				long Pos = State.Request.DataStream.Position;
				byte[] Bin = await State.Request.DataStream.ReadAllAsync();
				State.Request.DataStream.Position = Pos;

				Encoding Encoding = State.Request.Header.ContentType?.Encoding ?? Encoding.UTF8;
				State.ContentAsString = Encoding.GetString(Bin);
			}
			
			return await this.Review(State, State.ContentAsString);
		}
	}
}
