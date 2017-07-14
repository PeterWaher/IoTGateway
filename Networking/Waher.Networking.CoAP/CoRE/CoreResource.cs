using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.Options;

namespace Waher.Networking.CoAP.CoRE
{
	/// <summary>
	/// Manages a CoRE resource for an CoAP endpoint.
	/// </summary>
	public class CoreResource : CoapResource, ICoapGetMethod
	{
		private CoapEndpoint endpoint;

		/// <summary>
		/// Manages a CoRE resource for an CoAP endpoint.
		/// </summary>
		/// <param name="Endpoint">CoAP endpoint.</param>
		public CoreResource(CoapEndpoint Endpoint)
			: base("/.well-known/core")
		{
			this.endpoint = Endpoint;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get { return false; }
		}

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET
		{
			get { return true; }
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">CoAP Request</param>
		/// <param name="Response">CoAP Response</param>
		/// <exception cref="CoapException">If an error occurred when processing the method.</exception>
		public void GET(CoapMessage Request, CoapResponse Response)
		{
			StringBuilder sb = new StringBuilder();
			CoapResource[] Resources = this.endpoint.GetResources();
			LinkedList<KeyValuePair<string, string>> Filter = null;
			bool First = true;
			string s;
			int i, c;
			bool Include;

			foreach (CoapOption Option in Request.Options)
			{
				if (Option is CoapOptionUriQuery Query)
				{
					if (Filter == null)
						Filter = new LinkedList<KeyValuePair<string, string>>();

					Filter.AddLast(new KeyValuePair<string, string>(Query.Key, Query.KeyValue));
				}
			}

			foreach (CoapResource Resource in Resources)
			{
				if (Filter != null)
				{
					Include = true;

					foreach (KeyValuePair<string, string> Rec in Filter)
					{
						switch (Rec.Key)
						{
							case "href":
								Include = this.Matches(Rec.Value, Resource.Path);
								break;

							case "rt":
								Include = this.Matches(Rec.Value, Resource.ResourceTypes);
								break;

							case "if":
								Include = this.Matches(Rec.Value, Resource.InterfaceDescriptions);
								break;

							case "ct":
								Include = this.Matches(Rec.Value, Resource.ContentFormats);
								break;

							case "obs":
								Include = Resource.Observable;
								break;

							case "title":
								Include = this.Matches(Rec.Value, Resource.Title);
								break;

							case "sz":
								if (!Resource.MaximumSizeEstimate.HasValue)
									Include = false;
								else
									Include = this.Matches(Rec.Value, Resource.MaximumSizeEstimate.Value.ToString());
								break;

							default:
								Include = false;
								break;
						}
					}

					if (!Include)
						continue;
				}

				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append('<');
				sb.Append(Resource.Path);
				sb.Append('>');

				if (Resource.Observable)
					sb.Append(";obs");

				if (!string.IsNullOrEmpty(s = Resource.Title))
				{
					sb.Append(";title=\"");
					sb.Append(s.Replace("\"", "\\\""));
					sb.Append("\"");
				}

				this.Append(sb, "rt", Resource.ResourceTypes);
				this.Append(sb, "if", Resource.InterfaceDescriptions);

				int[] ContentFormats = Resource.ContentFormats;
				if (ContentFormats != null && (c = ContentFormats.Length) > 0)
				{
					sb.Append(";ct=\"");

					for (i = 0; i < c; i++)
					{
						if (i > 0)
							sb.Append(' ');

						sb.Append(ContentFormats[i].ToString());
					}

					sb.Append('"');
				}

				if (Resource.MaximumSizeEstimate.HasValue)
				{
					sb.Append(";sz=");
					sb.Append(Resource.MaximumSizeEstimate.Value.ToString());
				}
			}

			Response.Respond(CoapCode.Content, Encoding.UTF8.GetBytes(sb.ToString()),
				64, new CoapOptionContentFormat(CoreLinkFormat.ContentFormatCode));
		}

		private bool Matches(string Pattern, int[] Values)
		{
			if (Values == null)
				return false;

			foreach (int Value in Values)
			{
				if (this.Matches(Pattern, Value.ToString()))
					return true;
			}

			return false;
		}

		private bool Matches(string Pattern, string[] Values)
		{
			if (Values == null)
				return false;

			foreach (string Value in Values)
			{
				if (this.Matches(Pattern, Value))
					return true;
			}

			return false;
		}

		private bool Matches(string Pattern, string Value)
		{
			if (Value == null)
				return false;

			if (Pattern.EndsWith("*"))
				return Value.StartsWith(Pattern.Substring(0, Pattern.Length - 1));
			else
				return Pattern == Value;
		}

		private void Append(StringBuilder sb, string Attribute, string[] Strings)
		{
			int i, c;

			if (Strings != null && (c = Strings.Length) > 0)
			{
				sb.Append(';');
				sb.Append(Attribute);
				sb.Append("=\"");

				for (i = 0; i < c; i++)
				{
					if (i > 0)
						sb.Append(' ');

					sb.Append(Strings[i].Replace("\"", "\\\""));
				}

				sb.Append("\"");
			}
		}

	}
}
