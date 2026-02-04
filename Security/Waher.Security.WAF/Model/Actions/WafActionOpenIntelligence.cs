using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml.Attributes;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Collections;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.Security.WAF.Model.Actions
{
	/// <summary>
	/// Abstract base class for Web Application Firewall Open Intelligence actions.
	/// </summary>
	public abstract class WafActionOpenIntelligence : WafActionWithTags
	{
		private readonly DurationAttribute duration;
		private readonly StringAttribute vector;
		private readonly StringAttribute protocol;
		private readonly StringAttribute classification;
		private readonly StringAttribute code;
		private readonly StringAttribute message;

		/// <summary>
		/// Abstract base class for Web Application Firewall Open Intelligence actions.
		/// </summary>
		public WafActionOpenIntelligence()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall Open Intelligence actions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public WafActionOpenIntelligence(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.duration = new DurationAttribute(Xml, "duration");
			this.vector = new StringAttribute(Xml, "vector");
			this.protocol = new StringAttribute(Xml, "protocol");
			this.classification = new StringAttribute(Xml, "classification");
			this.code = new StringAttribute(Xml, "code");
			this.message = new StringAttribute(Xml, "message");
		}

		/// <summary>
		/// Evaluates an object reference from the attributes of the action.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Corresponding information object.</returns>
		public async Task<GenericObject> EvaluateObject(ProcessingState State)
		{
			Variables Variables = State.Variables;
			DateTime UtcNow = DateTime.UtcNow;
			DateTime Expires = UtcNow + await this.duration.EvaluateAsync(Variables, Duration.Zero);

			KeyValuePair<string, object>[] Properties = new KeyValuePair<string, object>[]
			{
				new KeyValuePair<string, object>("Domain", new CaseInsensitiveString(State.Request.Host)),
				new KeyValuePair<string, object>("RemoteEndpoint", new CaseInsensitiveString(State.Request.RemoteEndPoint.RemovePortNumber())),
				new KeyValuePair<string, object>("Timestamp", UtcNow),
				new KeyValuePair<string, object>("Expires", Expires),
				new KeyValuePair<string, object>("Vector", await this.vector.EvaluateAsync(Variables,string.Empty)),
				new KeyValuePair<string, object>("Protocol", await this.protocol.EvaluateAsync(Variables,string.Empty)),
				new KeyValuePair<string, object>("Classification", await this.classification.EvaluateAsync(Variables, string.Empty)),
				new KeyValuePair<string, object>("Code", await this.code.EvaluateAsync(Variables, string.Empty)),
				new KeyValuePair<string, object>("Message", await this.message.EvaluateAsync(Variables, string.Empty)),
				new KeyValuePair<string, object>("AgentJid", new CaseInsensitiveString(State.Request.Host))
			};

			GenericObject Result = new GenericObject("OpenIntelligence",
				"Waher.Service.IoTBroker.WebServices.Agent.Intelligence.Information",
				Guid.NewGuid(), Properties);

			ChunkedList<GenericObject> Tags = new ChunkedList<GenericObject>();

			foreach (KeyValuePair<string, object> P in await this.EvaluateTags(State))
			{
				Tags.Add(new GenericObject(string.Empty, string.Empty, Guid.Empty,
					new KeyValuePair<string, object>[]
					{
						new KeyValuePair<string, object>("Name", P.Key),
						new KeyValuePair<string, object>("Value", P.Value)
					}));
			}

			Result["Tags"] = Tags.ToArray();

			return Result;
		}

		/// <summary>
		/// Finds any Open Intelligence records matching the parameters of the action.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Array of matching Open Intelligence records.</returns>
		public async Task<GenericObject[]> FindObjects(ProcessingState State)
		{
			Variables Variables = State.Variables;
			DateTime UtcNow = DateTime.UtcNow;
			DateTime From = UtcNow - await this.duration.EvaluateAsync(Variables, Duration.Zero);
			StringBuilder sb = new StringBuilder();
			string RemoteEndpoint = State.Request.RemoteEndPoint.RemovePortNumber();
			string s;

			sb.AppendLine(RemoteEndpoint);

			ChunkedList<Filter> Filters = new ChunkedList<Filter>()
			{
				new FilterFieldEqualTo("RemoteEndpoint", new CaseInsensitiveString(RemoteEndpoint)),
				new FilterFieldGreaterOrEqualTo("Timestamp", From)
			};

			s = await this.vector.EvaluateAsync(Variables, string.Empty);
			sb.AppendLine(s);
			if (!string.IsNullOrEmpty(s))
				Filters.Add(new FilterFieldEqualTo("Vector", s));

			s = await this.protocol.EvaluateAsync(Variables, string.Empty);
			sb.AppendLine(s);
			if (!string.IsNullOrEmpty(s))
				Filters.Add(new FilterFieldEqualTo("Protocol", s));

			s = await this.classification.EvaluateAsync(Variables, string.Empty);
			sb.AppendLine(s);
			if (!string.IsNullOrEmpty(s))
				Filters.Add(new FilterFieldEqualTo("Classification", s));

			s = await this.code.EvaluateAsync(Variables, string.Empty);
			sb.AppendLine(s);
			if (!string.IsNullOrEmpty(s))
				Filters.Add(new FilterFieldEqualTo("Code", s));

			s = await this.message.EvaluateAsync(Variables, string.Empty);
			sb.AppendLine(s);
			if (!string.IsNullOrEmpty(s))
				Filters.Add(new FilterFieldEqualTo("Message", s));

			string Key = sb.ToString();

			if (State.TryGetCachedObject(Key, out GenericObject[] Result))
				return Result;

			ChunkedList<GenericObject> Results = new ChunkedList<GenericObject>();
			KeyValuePair<string, object>[] Tags = await this.EvaluateTags(State);
			IEnumerable<GenericObject> Objects = await Database.Find<GenericObject>(
				new FilterAnd(Filters.ToArray()));

			if (Tags.Length > 0)
			{
				foreach (GenericObject Obj in Objects)
				{
					if (!Obj.TryGetFieldValue("Tags", out object TagsObj) ||
						!(TagsObj is Array TagsArray))
					{
						continue;
					}

					Dictionary<string, object> ObjTags = new Dictionary<string, object>();

					foreach (object Item in TagsArray)
					{
						if (Item is GenericObject ItemObj &&
							ItemObj.TryGetFieldValue("Name", out object NameObj) &&
							!(NameObj is null) &&
							ItemObj.TryGetFieldValue("Value", out object ValueObj))
						{
							ObjTags[NameObj.ToString()] = ValueObj;
						}
					}

					bool Match = true;

					foreach (KeyValuePair<string, object> P in Tags)
					{
						if (!ObjTags.TryGetValue(P.Key, out object Value))
						{
							Match = false;
							break;
						}

						if (P.Value is null)
						{
							if (!(Value is null))
							{
								Match = false;
								break;
							}
						}
						else if (!P.Value.Equals(Value))
						{
							Match = false;
							break;
						}
					}

					if (!Match)
						continue;

					Results.Add(Obj);
				}
			}
			else
				Results.AddRange(Objects);

			Result = Results.ToArray();
			State.AddToCache(Key, Result, fiveMinutes);

			return Result;
		}
	}
}
