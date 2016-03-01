using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Networking.HTTP;
using Waher.Script;

namespace Waher.WebService.Script
{
	public class ScriptService : HttpAsynchronousResource, IHttpPostMethod
	{
		private HttpAuthenticationScheme[] authenticationSchemes;

		public ScriptService(string ResourceName, params HttpAuthenticationScheme[] AuthenticationSchemes)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public void POST(HttpRequest Request, HttpResponse Response)
		{
			if (!Request.HasData || Request.Session == null)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			string s = Obj as string;

			if (s == null)
				throw new BadRequestException();

			Expression Exp = new Expression(s);
			Obj = Exp.Evaluate(Request.Session);

			s = Obj.ToString();
			Response.Return(s);
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
