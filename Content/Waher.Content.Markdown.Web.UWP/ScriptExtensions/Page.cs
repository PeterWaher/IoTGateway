using System;
using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Markdown.Web.ScriptExtensions
{
	/// <summary>
	/// Page-local variables.
	/// </summary>
	public class Page : IConstant
	{
		/// <summary>
		/// Page internal variable name.
		/// </summary>
		public const string VariableName = " PageVariables ";

		/// <summary>
		/// Last Page internal variable name.
		/// </summary>
		public const string LastPageVariableName = " LastPage ";

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
			if (!Variables.TryGetVariable(VariableName, out Variable v))
				return null;

			IElement Result = v.ValueElement;

			if (!Variables.TryGetVariable(LastPageVariableName, out v) ||
				!(v.ValueObject is string Url) ||
				!Variables.TryGetVariable("Request", out v) ||
				!(v.ValueObject is HttpRequest Request))
			{
				return null;
			}

			if (string.Compare(Url, Request.Header.ResourcePart, true) == 0)
				return Result;

			if (!(Request.Header.Referer is null) &&
				Uri.TryCreate(Request.Header.Referer.Value, UriKind.Absolute, out Uri Referer) &&
				string.Compare(Referer.Host, Request.Host, true) == 0 &&
				string.Compare(Referer.PathAndQuery, Url, true) == 0)
			{
				return Result;
			}

			return null;
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
		public static Variables GetPageVariables(Variables Session, string Resource)
		{
			if (!Session.TryGetVariable(Page.VariableName, out Variable v) ||
				!(v.ValueObject is Variables PageVariables))
			{
				Session[Page.LastPageVariableName] = Resource;
				Session[Page.VariableName] = PageVariables = new Variables();
			}
			else if (!Session.TryGetVariable(Page.LastPageVariableName, out v) ||
				!(v.ValueObject is string LastPageUrl) ||
				LastPageUrl != Resource)
			{
				Session[Page.LastPageVariableName] = Resource;
				PageVariables.Clear();
			}

			return PageVariables;
		}

		/// <summary>
		/// Clears the current set of page variables.
		/// </summary>
		/// <param name="Session">Current session.</param>
		public static void Clear(Variables Session)
		{
			Session.Remove(Page.VariableName);
			Session.Remove(Page.LastPageVariableName);
		}

		/// <summary>
		/// Removes a page variable from the session.
		/// </summary>
		/// <param name="Session">Current session.</param>
		/// <param name="PageVariableName">Page variable name to remove.</param>
		/// <returns>If such a page variable was found and removed.</returns>
		public static bool Remove(Variables Session, string PageVariableName)
		{
			if (Session.TryGetVariable(Page.VariableName, out Variable v) &&
				v.ValueObject is Variables PageVariables)
			{
				return PageVariables.Remove(PageVariableName);
			}
			else
				return false;
		}

	}
}
