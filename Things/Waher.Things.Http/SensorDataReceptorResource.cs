using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Things.Metering;
using Waher.Things.SensorData;

namespace Waher.Things.Http
{
	/// <summary>
	/// Web Service REST API that receives sensor data from external sources.
	/// </summary>
	public class SensorDataReceptorResource : HttpSynchronousResource, IHttpGetMethod,
		IHttpPostMethod
	{
		private const string SensorDataNamespace = "urn:nfi:iot:sd:1.0";

		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Web Service REST API that receives sensor data from external sources.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public SensorDataReceptorResource(string ResourceName,
			params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

		/// <summary>
		/// If the GET method is allowed.
		/// </summary>
		public bool AllowsGET => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			if (Request.Header.Method.ToUpper() == "GET")
				return null;
			else
				return this.authenticationSchemes;
		}

		/// <summary>
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			byte[] Data = Resources.LoadResource(
				typeof(SensorDataReceptorResource).Namespace + ".Data.ApiDocumentation.md",
				typeof(SensorDataReceptorResource).Assembly);

			string Markdown = Strings.GetString(Data, Encoding.UTF8);
			MarkdownSettings Settings = new MarkdownSettings(null, true, new Variables())
			{
				RootFolder = HttpModule.RootFolder,
				ResourceMap = HttpModule.WebServer
			};
			MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, Settings,
				null, Request.Resource.ResourceName, Request.Header.GetURL());
			string Html = await Doc.GenerateHTML();

			Response.ContentType = HtmlCodec.DefaultContentType;

			await Response.Write(Html);
			await Response.SendResponse();
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (Request.User is null)
			{
				await Response.SendResponse(new ForbiddenException("Access denied."));
				return;
			}

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No payload."));
				return;
			}

			ContentResponse Payload = await Request.DecodeDataAsync();
			if (Payload.HasError)
			{
				await Response.SendResponse(new BadRequestException("Unable to decode payload: " + Payload.Error.Message));
				return;
			}

			if (!(Payload.Decoded is XmlDocument Xml) || Xml.DocumentElement is null)
			{
				await Response.SendResponse(new BadRequestException("Expected XML payload."));
				return;
			}

			if (Xml.DocumentElement.NamespaceURI != SensorDataNamespace)
			{
				await Response.SendResponse(new BadRequestException("Invalid namespace. Expected: " + SensorDataNamespace));
				return;
			}

			if (string.IsNullOrEmpty(Request.SubPath))
			{
				if (!Request.User.HasPrivilege(HttpModule.PostPrivileges))
				{
					await Response.SendResponse(new ForbiddenException("Access denied. User lacks sufficient privileges: " + HttpModule.PostPrivileges));
					return;
				}

				if (Xml.DocumentElement.LocalName == "resp")
				{
					foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						if (E.NamespaceURI != SensorDataNamespace || E.LocalName != "nd")
						{
							await Response.SendResponse(new BadRequestException("Expected <nd> element as child of <resp>."));
							return;
						}

						if (!await this.ProcessNode(E, Response))
							return;
					}
				}
				else if (Xml.DocumentElement.LocalName == "nd")
				{
					if (!await this.ProcessNode(Xml.DocumentElement, Response))
						return;
				}
				else
				{
					await Response.SendResponse(new BadRequestException("Expected <resp> or <nd> element."));
					return;
				}
			}
			else
			{
				string PrivilegeId = HttpModule.PostPrivileges + Request.SubPath.Replace('/', '.');

				if (!Request.User.HasPrivilege(PrivilegeId))
				{
					await Response.SendResponse(new ForbiddenException("Access denied. User lacks sufficient privileges: " + PrivilegeId));
					return;
				}

				if (Xml.DocumentElement.LocalName == "ts")
				{
					string[] Path = Request.SubPath[1..].Split('/');
					string NodeId = Path[^1];

					MeteringNode Node = await MeteringTopology.GetNode(NodeId);

					if (Node is ExternalWebNode ExternalWebNode)
					{
						if (!await this.ProcessTimestamp(ExternalWebNode, Xml.DocumentElement, Response))
							return;
					}
					else if (Node is null)
					{
						MeteringNode Parent = HttpModule.LocalWebServerNode;
						ExternalWebNode = null;

						foreach (string Part in Path)
						{
							ExternalWebNode = null;

							foreach (INode Child in await Parent.ChildNodes)
							{
								if (Child.NodeId == Part)
								{
									if (Child is ExternalWebNode E)
									{
										ExternalWebNode = E;
										break;
									}
									else
									{
										await Response.SendResponse(new ForbiddenException("Node not an external web node: " + Part));
										return;
									}
								}
							}

							if (ExternalWebNode is null)
							{
								if (!(await MeteringTopology.GetNode(Part) is null))
								{
									await Response.SendResponse(new ForbiddenException("Part already exists, or is of incorrect type: " + Part));
									return;
								}

								ExternalWebNode = new ExternalWebNode()
								{
									NodeId = Part
								};

								await Parent.AddAsync(ExternalWebNode);
							}
						}

						if (ExternalWebNode is null)
						{
							await Response.SendResponse(new BadRequestException("Undefined Node path."));
							return;
						}
						else if (!await this.ProcessTimestamp(ExternalWebNode, Xml.DocumentElement, Response))
							return;
					}
					else
					{
						await Response.SendResponse(new ForbiddenException("Node not an external web node: " + NodeId));
						return;
					}
				}
				else
				{
					await Response.SendResponse(new BadRequestException("Expected <td> element."));
					return;
				}
			}

			Response.StatusCode = 204; // No content.
			await Response.SendResponse();
		}

		private async Task<bool> ProcessNode(XmlElement Xml, HttpResponse Response)
		{
			string NodeId = Xml.GetAttribute("id");
			string SourceId = Xml.GetAttribute("src");
			string Partition = Xml.GetAttribute("pt");

			if (!string.IsNullOrEmpty(SourceId) &&
				SourceId != MeteringTopology.SourceID)
			{
				await Response.SendResponse(new ForbiddenException("Access to data source forbidden."));
				return false;
			}

			if (!string.IsNullOrEmpty(Partition))
			{
				await Response.SendResponse(new BadRequestException("Expected no partition."));
				return false;
			}

			MeteringNode Node = await MeteringTopology.GetNode(NodeId);
			if (Node is null)
			{
				await Response.SendResponse(new NotFoundException("Node not found: " + NodeId));
				return false;
			}

			if (!(Node is ExternalWebNode ExternalWebNode))
			{
				await Response.SendResponse(new ForbiddenException("Node not an external web node: " + NodeId));
				return false;
			}

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				if (E.NamespaceURI != SensorDataNamespace || E.LocalName != "ts")
				{
					await Response.SendResponse(new BadRequestException("Expected <ts> element as child of <nd>."));
					return false;
				}

				if (!await this.ProcessTimestamp(ExternalWebNode, E, Response))
					return false;
			}

			return true;
		}

		private async Task<bool> ProcessTimestamp(ExternalWebNode Node, XmlElement Xml, HttpResponse Response)
		{
			List<ThingError> Errors = null;
			List<Field> Fields = null;

			SensorClient.ParseTimespan(Xml, Node, ref Fields, ref Errors);

			if (Fields is null && Errors is null)
			{
				await Response.SendResponse(new BadRequestException("No sensor data in payload."));
				return false;
			}

			await Node.NewSensorData(Fields?.ToArray(), Errors?.ToArray());

			return true;
		}
	}

}
