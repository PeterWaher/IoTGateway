using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Networking.HTTP.ScriptExtensions
{
	/// <summary>
	/// Page-local variables.
	/// </summary>
	public class Page : IConstant
	{
		/// <summary>
		/// Page-local variables.
		/// </summary>
		public Page()
		{
		}

		/// <summary>
		/// Name of the constant
		/// </summary>
		public string ConstantName => nameof(Page);

		/// <summary>
		/// Optional aliases. If there are no aliases for the constant, null is returned.
		/// </summary>
		public string[] Aliases => null;

		/// <summary>
		/// Gets the constant value element.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		public IElement GetValueElement(Variables Variables)
		{
			if (!(Variables is SessionVariables SessionVariables))
			{
				if (Variables.ContextVariables is SessionVariables SessionVariables2)
					SessionVariables = SessionVariables2;
				else
					return ObjectValue.Null;
			}

			if (SessionVariables.CurrentPageVariables is null ||
				SessionVariables.CurrentRequest is null)
			{
				return ObjectValue.Null;
			}

			if (string.Compare(SessionVariables.CurrentPageUrl,
				SessionVariables.CurrentRequest.Header.ResourcePart, true) == 0)
			{
				return SessionVariables.CurrentPageVariablesElement;
			}

			if (!(SessionVariables.CurrentRequest.Header.Referer is null) &&
				Uri.TryCreate(SessionVariables.CurrentRequest.Header.Referer.Value, UriKind.Absolute, out Uri Referer) &&
				string.Compare(Referer.Host, SessionVariables.CurrentRequest.Host, true) == 0 &&
				string.Compare(Referer.PathAndQuery, SessionVariables.CurrentPageUrl, true) == 0)
			{
				return SessionVariables.CurrentPageVariablesElement;
			}

			return ObjectValue.Null;
		}

		/// <summary>
		/// Gets the variable collection for the current page.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <returns>Page variables.</returns>
		public static Variables GetPageVariables(HttpRequest Request)
		{
			return GetPageVariables(Request.Session, Request.Header.ResourcePart);
		}

		/// <summary>
		/// Gets the variable collection for the current page.
		/// </summary>
		/// <param name="Session">Session</param>
		/// <param name="Resource">Resource part of the URL of the page.</param>
		/// <returns>Page variables.</returns>
		public static Variables GetPageVariables(SessionVariables Session, string Resource)
		{
			if (Session.CurrentPageVariables is null)
			{
				Session.CurrentPageUrl = Resource;
				Session.CurrentPageVariables = new Variables();
			}
			else if (Session.CurrentPageUrl is null || Session.CurrentPageUrl != Resource)
			{
				Session.CurrentPageUrl = Resource;
				Session.CurrentPageVariables.Clear();
			}

			return Session.CurrentPageVariables;
		}

		/// <summary>
		/// Clears the current set of page variables.
		/// </summary>
		/// <param name="Session">Current session.</param>
		public static void Clear(SessionVariables Session)
		{
			Session.CurrentPageUrl = null;
			Session.CurrentPageVariables = null;
		}
	}
}
