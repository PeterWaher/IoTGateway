using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Semantic;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Persistence.SPARQL;
using Waher.Security.LoginMonitor;

namespace Waher.WebService.Sparql
{
	/// <summary>
	/// Web service that can be used to execute SPARQL queries on the server.
	/// 
	/// References:
	/// https://www.w3.org/TR/sparql12-protocol/
	/// https://www.w3.org/TR/sparql12-query/
	/// https://www.w3.org/TR/sparql12-federated-query/
	/// </summary>
	public class SparqlEndpoint : HttpAsynchronousResource, IHttpGetMethod, IHttpPostMethod
	{
		private readonly HttpAuthenticationScheme[] authenticationSchemes;

		/// <summary>
		/// Web service that can be used to execute SPARQL queries on the server.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="AuthenticationSchemes">Authentication schemes.</param>
		public SparqlEndpoint(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
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
		/// Executes the GET method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task GET(HttpRequest Request, HttpResponse Response)
		{
			CheckAuthorization(Request);

			if (!Request.Header.TryGetQueryParameter("query", out _))
			{
				await Response.SendResponse(new SeeOtherException("/Sparql.md"));
				return;
			}

			// TODO: Return page if empty GET.

			State State = new State(Request);
			State.Start();

			(SparqlQuery Query, ISemanticCube[] DefaultGraphs, string[] NamedGraphs, bool Pretty) =
				await this.GetQueryGraphs(Request.Header.QueryParameters, State);

			Task T = Task.Run(() => this.Process(Request, Response, Query, DefaultGraphs, NamedGraphs, State, Pretty));
		}

		private static void CheckAuthorization(HttpRequest Request)
		{
			if (Request.User is null || !Request.User.HasPrivilege(SparqlServiceModule.QueryPrivileges))
				throw new ForbiddenException(Request, "Access to endpoint denied.");
		}

		private async Task<(SparqlQuery, ISemanticCube[], string[], bool)> GetQueryGraphs(
			IEnumerable<KeyValuePair<string, string>> QueryParameters, State State)
		{
			SparqlQuery Query = null;

			foreach (KeyValuePair<string, string> P in QueryParameters)
			{
				switch (P.Key)
				{
					case "query":
						Expression Exp = new Expression(P.Value);
						if (!(Exp.Root is SparqlQuery Query2))
							throw new BadRequestException("Invalid SPARQL Query.");

						Query = Query2;
						break;

					case "default-graph-uri":
					case "named-graph-uri":
					case "pretty":
						break;

					default:
						throw new BadRequestException("Unrecognized query parameter: " + P.Key);
				}
			}

			if (Query is null)
				throw new BadRequestException("Query missing.");

			State.Parsed();

			(ISemanticCube[] DefaultGraphs, string[] NamedGraphs, bool Pretty) =
				await this.GetQueryGraphs(Query, QueryParameters, State);

			return (Query, DefaultGraphs, NamedGraphs, Pretty);
		}

		private async Task<(ISemanticCube[], string[], bool)> GetQueryGraphs(SparqlQuery Query,
			IEnumerable<KeyValuePair<string, string>> QueryParameters, State State)
		{
			List<ISemanticCube> DefaultGraphs = null;
			List<string> NamedGraphs = null;
			bool Pretty = false;

			foreach (KeyValuePair<string, string> P in QueryParameters)
			{
				switch (P.Key)
				{
					case "default-graph-uri":
						if (!string.IsNullOrEmpty(P.Value))
						{
							Uri SourceUri = new Uri(P.Value);
							IGraphSource Source = await SparqlQuery.GetSourceHandler(SourceUri, false);
							ISemanticCube Cube = await Source.LoadGraph(SourceUri, Query, false, await State.GetOrigin());

							DefaultGraphs ??= new List<ISemanticCube>();
							DefaultGraphs.Add(Cube);
						}
						break;

					case "named-graph-uri":
						if (!string.IsNullOrEmpty(P.Value))
						{
							NamedGraphs ??= new List<string>();
							NamedGraphs.Add(P.Value);
						}
						break;

					case "pretty":
						if (!CommonTypes.TryParse(P.Value, out bool b))
							throw new BadRequestException("Invalid boolean value.");
						Pretty = b;
						break;
				}
			}

			State.DefaultLoaded();

			return (DefaultGraphs?.ToArray(), NamedGraphs?.ToArray(), Pretty);
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			CheckAuthorization(Request);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			State State = new State(Request);
			State.Start();

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError)
			{
				await Response.SendResponse(Content.Error);
				return;
			}

			object Obj = Content.Decoded;
			ISemanticCube[] DefaultGraphs;
			string[] NamedGraphs;
			bool Pretty;

			State.Parsed();

			if (Obj is SparqlQuery Query)
			{
				(DefaultGraphs, NamedGraphs, Pretty) = await this.GetQueryGraphs(Query,
					Request.Header.QueryParameters, State);
			}
			else if (Obj is Dictionary<string, string> Form)
			{
				(Query, DefaultGraphs, NamedGraphs, Pretty) = await this.GetQueryGraphs(Form, State);
			}
			else if (Obj is Dictionary<string, string[]> Form2)
			{
				LinkedList<KeyValuePair<string, string>> Parameters = new LinkedList<KeyValuePair<string, string>>();

				foreach (KeyValuePair<string, string[]> P in Form2)
				{
					foreach (string s2 in P.Value)
						Parameters.AddLast(new KeyValuePair<string, string>(P.Key, s2));
				}

				(Query, DefaultGraphs, NamedGraphs, Pretty) = await this.GetQueryGraphs(Parameters, State);
			}
			else if (Obj is Dictionary<string, object> Form3)
			{
				LinkedList<KeyValuePair<string, string>> Parameters = new LinkedList<KeyValuePair<string, string>>();

				foreach (KeyValuePair<string, object> P in Form3)
				{
					if (P.Value is string s)
						Parameters.AddLast(new KeyValuePair<string, string>(P.Key, s));
					else if (P.Value is Array A)
					{
						foreach (object Item in A)
						{
							if (Item is string s2)
								Parameters.AddLast(new KeyValuePair<string, string>(P.Key, s2));
							else
							{
								await Response.SendResponse(new BadRequestException("Invalid form."));
								return;
							}
						}
					}
					else
					{
						await Response.SendResponse(new BadRequestException("Invalid form."));
						return;
					}
				}

				(Query, DefaultGraphs, NamedGraphs, Pretty) = await this.GetQueryGraphs(Parameters, State);
			}
			else
			{
				await Response.SendResponse(new UnsupportedMediaTypeException("Content must be a SPARQL query or a web form containing a SPARQL query."));
				return;
			}

			Task _ = Task.Run(() => this.Process(Request, Response, Query, DefaultGraphs, NamedGraphs, State, Pretty));
		}

		private async void Process(HttpRequest Request, HttpResponse Response,
			SparqlQuery Query, ISemanticCube[] DefaultGraphs, string[] NamedGraphs,
			State State, bool Pretty)
		{
			bool Error = false;

			try
			{
				Variables v = Request.Session ?? new Variables();

				if (!(DefaultGraphs is null) && DefaultGraphs.Length > 0)
				{
					if (DefaultGraphs.Length == 1)
						v[" Default Graph "] = DefaultGraphs[0];
					else
					{
						SemanticDataSet DataSet = new SemanticDataSet();

						foreach (ISemanticCube Cube in DefaultGraphs)
							DataSet.Add(Cube);

						v[" Default Graph "] = DataSet;
					}
				}
				else
					v[" Default Graph "] = await GraphStore.GetDefaultSource(await GraphStore.GetOrigin(Request));

				if (!(NamedGraphs is null))
					Query.RegisterNamedGraph(NamedGraphs);

				object Result = await Query.Expression.EvaluateAsync(v);
				State.Evaluated();

				if (Result is SparqlResultSet ResultSet)
					ResultSet.Pretty = Pretty;

				await Response.Return(Result);

				State.Returned();
			}
			catch (Exception ex)
			{
				Error = true;
				Log.Exception(ex);

				if (!Response.ResponseSent)
				{
					await Response.SendResponse(ex);
					State.Returned();
				}
			}
			finally
			{
				State.Stop();

				try
				{
					KeyValuePair<string, object>[] Tags = await LoginAuditor.Annotate(Request.RemoteEndPoint,
						new KeyValuePair<string, object>("RemoteEndPoint", Request.RemoteEndPoint),
						new KeyValuePair<string, object>("SPARQL", Query.SubExpression),
						new KeyValuePair<string, object>("Total ms", State.TotalMilliseconds),
						new KeyValuePair<string, object>("Parsing ms", State.ParsingTimeMs),
						new KeyValuePair<string, object>("Loading ms", State.LoadDefaultMs),
						new KeyValuePair<string, object>("Evaluating ms", State.EvaluatingMs),
						new KeyValuePair<string, object>("Returning ms", State.ReturningMs),
						new KeyValuePair<string, object>("Error", Error));

					if (Error)
					{
						Log.Warning("SPARQL evaluated with errors.", Request.Resource.ResourceName,
							Request.User.UserName, "SparqlEval", Tags);
					}
					else
					{
						Log.Notice("SPARQL evaluated.", Request.Resource.ResourceName,
							Request.User.UserName, "SparqlEval", Tags);
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		/// <summary>
		/// Any authentication schemes used to authenticate users before access is granted to the corresponding resource.
		/// </summary>
		/// <param name="Request">Current request</param>
		public override HttpAuthenticationScheme[] GetAuthenticationSchemes(HttpRequest Request)
		{
			return this.authenticationSchemes;
		}
	}
}
