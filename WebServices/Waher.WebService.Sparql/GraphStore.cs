using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Waher.Content;
using Waher.Content.Multipart;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Content.Xml;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Script.Persistence.SPARQL;
using Waher.Script.Persistence.SPARQL.Sources;
using Waher.Security;
using Waher.Things;
using Waher.Things.Semantic.Sources.DynamicGraphs;

namespace Waher.WebService.Sparql
{
	/// <summary>
	/// Graph Store for semantic graphs.
	/// https://www.w3.org/TR/sparql12-graph-store-protocol/
	/// </summary>
	public class GraphStore : HttpSynchronousResource, IHttpGetMethod, IHttpPostMethod, IHttpPutMethod, IHttpDeleteMethod
	{
		private static readonly Dictionary<string, ISemanticModel> defaultGraphs = new Dictionary<string, ISemanticModel>();
		private static readonly SemaphoreSlim defaultGraphSemaphore = new SemaphoreSlim(1);

		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Graph Store for semantic graphs.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public GraphStore(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
			this.authenticationSchemes = AuthenticationSchemes;
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

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
		/// If the PUT method is allowed.
		/// </summary>
		public bool AllowsPUT => true;

		/// <summary>
		/// If the DELETE method is allowed.
		/// </summary>
		public bool AllowsDELETE => true;

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
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
			if (Request.User is null || !Request.User.HasPrivilege(SparqlServiceModule.GetPrivileges))
			{
				await Response.SendResponse(new ForbiddenException(Request, "Access to store denied."));
				return;
			}

			ISemanticModel Graph;

			if (Request.Header.TryGetQueryParameter("default", out _))
				Graph = await GetDefaultSource(await GetOrigin(Request));
			else
			{
				(GraphReference Reference, Uri GraphUri) = await GetGraphReference(Request, false);

				Response.StatusCode = 200;

				if (Request.Header.Method == "HEAD")
					return;

				IGraphSource Source = await Reference.GetGraphSource();

				Graph = await Source.LoadGraph(GraphUri, null, false, await GetOrigin(Request));
			}

			await Response.Return(Graph);
		}

		/// <summary>
		/// Gets an origin object from an HTTP request object.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <returns>Request origin object.</returns>
		public static Task<RequestOrigin> GetOrigin(HttpRequest Request)
		{
			if (Request.User is IRequestOrigin RequestOrigin)
				return RequestOrigin.GetOrigin();
			else
				return Task.FromResult(new RequestOrigin(Request.RemoteEndPoint, null, null, null, null));
		}

		internal static async Task<ISemanticModel> GetDefaultSource(RequestOrigin Caller)
		{
			await defaultGraphSemaphore.WaitAsync();
			try
			{
				string DefaultGraphKey = Caller?.From ?? string.Empty;

				lock (defaultGraphs)
				{
					if (defaultGraphs.TryGetValue(DefaultGraphKey, out ISemanticModel Doc))
						return Doc;
				}

				Language Language = await Translator.GetDefaultLanguageAsync();  // TODO: Check Accept-Language HTTP header.
				InMemorySemanticCube Result = new InMemorySemanticCube();
				UriNode GraphUriNode;
				bool First;

				foreach (IDataSource Source in Gateway.ConcentratorServer.RootDataSources)
					await SourceGraph.GenerateGraph(Result, Language, Source);

				foreach (GraphReference Reference in await Database.Find<GraphReference>())
				{
					GraphUriNode = new UriNode(new Uri(Reference.GraphUri));

					Result.Add(new SemanticTriple(
						GraphUriNode,
						new UriNode(DublinCoreTerms.type),
						new UriNode(DublinCoreMetadataInitiativeType.Dataset)));

					Result.Add(new SemanticTriple(
						GraphUriNode,
						new UriNode(DublinCoreTerms.created),
						new DateTimeLiteral(Reference.Created)));

					Result.Add(new SemanticTriple(
						GraphUriNode,
						new UriNode(DublinCoreTerms.updated),
						new DateTimeLiteral(Reference.Updated)));

					if (!(Reference.Creators is null))
					{
						First = true;

						foreach (string Creator in Reference.Creators)
						{
							if (First)
							{
								First = false;

								Result.Add(new SemanticTriple(
									GraphUriNode,
									new UriNode(DublinCoreTerms.creator),
									new StringLiteral(Creator)));
							}
							else
							{
								Result.Add(new SemanticTriple(
									GraphUriNode,
									new UriNode(DublinCoreTerms.contributor),
									new StringLiteral(Creator)));
							}
						}
					}
				}

				lock (defaultGraphs)
				{
					defaultGraphs[DefaultGraphKey] = Result;
				}

				return Result;
			}
			finally
			{
				defaultGraphSemaphore.Release();
			}
		}

		private static async Task<(GraphReference, Uri)> GetGraphReference(HttpRequest Request, bool NullIfNotFound)
		{
			if (!Request.Header.TryGetQueryParameter("graph", out string GraphUri))
				throw new SeeOtherException("/GraphStore.md");

			GraphUri = HttpUtility.UrlDecode(GraphUri);

			if (!Uri.TryCreate(GraphUri, UriKind.RelativeOrAbsolute, out Uri ParsedUri))
				throw new BadRequestException("Invalid graph URI.");

			if (!ParsedUri.IsAbsoluteUri)
				throw new BadRequestException("Graph URI must be an absolute URI.");

			if (Gateway.IsDomain(ParsedUri.Host, true))
				throw new ForbiddenException(Request, "Unauthorized access to server domain.");

			GraphReference Reference = await Database.FindFirstIgnoreRest<GraphReference>(
				new FilterFieldEqualTo("GraphUri", GraphUri));

			if (Reference is null && !NullIfNotFound)
				throw new NotFoundException("Graph not found.");

			return (Reference, ParsedUri);
		}

		/// <summary>
		/// Executes the PUT method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task PUT(HttpRequest Request, HttpResponse Response)
		{
			if (Request.User is null || !Request.User.HasPrivilege(SparqlServiceModule.AddPrivileges))
			{
				await Response.SendResponse(new ForbiddenException(Request, "Access to store denied."));
				return;
			}

			await this.Update(Request, Response, true);
		}

		private async Task Update(HttpRequest Request, HttpResponse Response, bool DeleteOld)
		{
			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No data in request."));
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError)
			{
				await Response.SendResponse(Content.Error);
				return;
			}

			List<KeyValuePair<string, string>> Files = new List<KeyValuePair<string, string>>();
			ChunkedList<ISemanticModel> Models = new ChunkedList<ISemanticModel>();

			if (Content.Decoded is TurtleDocument TurtleDoc)
			{
				Files.Add(new KeyValuePair<string, string>(TurtleDoc.Text, TurtleCodec.DefaultExtension));
				Models.Add(TurtleDoc);
			}
			else if (Content.Decoded is RdfDocument RdfDoc)
			{
				Files.Add(new KeyValuePair<string, string>(RdfDoc.Text, RdfCodec.DefaultExtension));
				Models.Add(RdfDoc);
			}
			else if (Content.Decoded is Dictionary<string, object> Form)
			{
				foreach (KeyValuePair<string, object> P in Form)
				{
					if (P.Value is TurtleDocument TurtleDoc2)
					{
						Files.Add(new KeyValuePair<string, string>(TurtleDoc2.Text, TurtleCodec.DefaultExtension));
						Models.Add(TurtleDoc2);
					}
					else if (P.Value is RdfDocument RdfDoc2)
					{
						Files.Add(new KeyValuePair<string, string>(RdfDoc2.Text, RdfCodec.DefaultExtension));
						Models.Add(RdfDoc2);
					}
					else
					{
						await Response.SendResponse(new UnsupportedMediaTypeException("Content in form must be semantic triples documents."));
						return;
					}
				}
			}
			else if (Content.Decoded is MultipartContent Form2)
			{
				foreach (EmbeddedContent P in Form2.Content)
				{
					if (P.Decoded is TurtleDocument TurtleDoc2)
					{
						Files.Add(new KeyValuePair<string, string>(TurtleDoc2.Text, TurtleCodec.DefaultExtension));
						Models.Add(TurtleDoc2);
					}
					else if (P.Decoded is RdfDocument RdfDoc2)
					{
						Files.Add(new KeyValuePair<string, string>(RdfDoc2.Text, RdfCodec.DefaultExtension));
						Models.Add(RdfDoc2);
					}
					else
					{
						await Response.SendResponse(new UnsupportedMediaTypeException("Content in form must be semantic triples documents."));
						return;
					}
				}
			}
			else if (Content.Decoded is string s)
			{
				if (XML.IsValidXml(s))
				{
					try
					{
						Models.Add(new RdfDocument(s));
						Files.Add(new KeyValuePair<string, string>(s, RdfCodec.DefaultExtension));
					}
					catch (Exception)
					{
						await Response.SendResponse(new BadRequestException("Unable to parse document as an RDF/XML document."));
						return;
					}
				}
				else
				{
					try
					{
						Models.Add(new TurtleDocument(s));
						Files.Add(new KeyValuePair<string, string>(s, TurtleCodec.DefaultExtension));
					}
					catch (Exception)
					{
						await Response.SendResponse(new BadRequestException("Unable to parse the document as a Turtle document."));
						return;
					}
				}
			}
			else
			{
				await Response.SendResponse(new UnsupportedMediaTypeException("Content must be a semantic triples document, or a collection of semantic triples document in a multipart form."));
				return;
			}

			(GraphReference Reference, Uri GraphUri) = await GetGraphReference(Request, true);
			DateTime TP = DateTime.UtcNow;
			string H = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(GraphUri.AbsoluteUri));
			string FileName;
			int i = 0;

			if (Reference is null)
			{
				Reference = new GraphReference()
				{
					Created = TP,
					Updated = TP,
					GraphUri = GraphUri.AbsoluteUri,
					NrFiles = Files.Count,
					GraphDigest = H,
					Folder = Path.Combine(Gateway.AppDataFolder, "GraphStore", H),
					Creators = new string[] { Request.User.UserName },
					InDatabase = false,
					DatabaseKey = 0
				};

				if (!Directory.Exists(Reference.Folder))
					Directory.CreateDirectory(Reference.Folder);

				foreach (KeyValuePair<string, string> P in Files)
				{
					FileName = Path.Combine(Reference.Folder, (++i).ToString());
					FileName = Path.ChangeExtension(FileName, P.Value);

					await Runtime.IO.Files.WriteAllTextAsync(FileName, P.Key);
				}

				await Database.Insert(Reference);

				Response.StatusCode = 201;
				Response.StatusMessage = "Created";
			}
			else
			{
				Reference.Updated = TP;

				if (DeleteOld)
				{
					foreach (string FileName2 in Directory.GetFiles(Reference.Folder, "*.*", SearchOption.TopDirectoryOnly))
						File.Delete(FileName2);

					Reference.NrFiles = Files.Count;
					Reference.Creators = new string[] { Request.User.UserName };

					if (Reference.InDatabase)
						await Database.FindDelete<DatabaseTriple>(new FilterFieldEqualTo("GraphKey", Reference.DatabaseKey));
				}
				else
				{
					i = Reference.NrFiles;
					Reference.NrFiles += Files.Count;
					Reference.Creators = AddIfNotIncluded(Reference.Creators, Request.User.UserName);
				}

				foreach (KeyValuePair<string, string> P in Files)
				{
					FileName = Path.Combine(Reference.Folder, (++i).ToString());
					FileName = Path.ChangeExtension(FileName, P.Value);

					await Runtime.IO.Files.WriteAllTextAsync(FileName, P.Key);
				}

				await Database.Update(Reference);

				Response.StatusCode = 200;  // OK
			}

			await Reference.ModelsAdded(Models);

			InvalidateDefaultGraph();
		}

		/// <summary>
		/// Invalidates the default graph, triggering a re-generation of the default
		/// graph the next time it is requested.
		/// </summary>
		internal static void InvalidateDefaultGraph()
		{
			lock (defaultGraphs)
			{
				defaultGraphs.Clear();
			}
		}

		private static string[] AddIfNotIncluded(string[] Values, string Value)
		{
			if (Array.IndexOf(Values, Value) >= 0)
				return Values;

			int c = Values.Length;
			string[] Result = new string[c + 1];

			Array.Copy(Values, 0, Result, 0, c);
			Result[c] = Value;

			return Result;
		}

		/// <summary>
		/// Executes the DELETE method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task DELETE(HttpRequest Request, HttpResponse Response)
		{
			if (Request.User is null || !Request.User.HasPrivilege(SparqlServiceModule.DeletePrivileges))
			{
				await Response.SendResponse(new ForbiddenException(Request, "Access to store denied."));
				return;
			}

			(GraphReference Reference, Uri _) = await GetGraphReference(Request, false);
			bool FilesDeleted = false;

			foreach (string FileName2 in Directory.GetFiles(Reference.Folder, "*.*", SearchOption.TopDirectoryOnly))
			{
				File.Delete(FileName2);
				FilesDeleted = true;
			}

			if (Reference.InDatabase)
				await Database.FindDelete<DatabaseTriple>(new FilterFieldEqualTo("GraphKey", Reference.DatabaseKey));

			await Database.Delete(Reference);

			if (FilesDeleted)
				Response.StatusCode = 200;
			else
			{
				Response.StatusCode = 204;
				Response.StatusMessage = "No Content";
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			if (Request.User is null || !Request.User.HasPrivilege(SparqlServiceModule.UpdatePrivileges))
			{
				await Response.SendResponse(new ForbiddenException(Request, "Access to store denied."));
				return;
			}

			await this.Update(Request, Response, false);
		}
	}
}
