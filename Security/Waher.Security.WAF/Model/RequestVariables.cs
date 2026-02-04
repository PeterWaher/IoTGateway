using Waher.Networking.HTTP;
using Waher.Script;
using Waher.Script.Objects;

namespace Waher.Security.WAF.Model
{
	/// <summary>
	/// Request variables used in script.
	/// </summary>
	public class RequestVariables : Variables
	{
		private readonly HttpRequest request;
		private Variable requestVariable;

		/// <summary>
		/// Request variables used in script.
		/// </summary>
		/// <param name="Request">Current request.</param>
		public RequestVariables(HttpRequest Request)
			: base()
		{
			this.request = Request;
		}

		private void InitializeContext()
		{
			this.ContextVariables ??= this.request.GetSessionFromCookie();
		}

		/// <summary>
		/// Tries to get a variable object, given its name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <param name="Variable">Variable, if found, or null otherwise.</param>
		/// <returns>If a variable with the corresponding name was found.</returns>
		public override bool TryGetVariable(string Name, out Variable Variable)
		{
			switch (Name)
			{
				case "Request":
					this.requestVariable ??= new Variable(Name, new ObjectValue(this.request));
					Variable = this.requestVariable;
					return true;

				default:
					this.InitializeContext();
					return base.TryGetVariable(Name, out Variable);
			}
		}

		/// <summary>
		/// If the collection contains a variable with a given name.
		/// </summary>
		/// <param name="Name">Variable name.</param>
		/// <returns>If a variable with that name exists.</returns>
		public override bool ContainsVariable(string Name)
		{
			switch (Name)
			{
				case "Request":
					return true;

				default:
					if (base.ContainsVariable(Name))
						return true;

					this.InitializeContext();
					return base.ContainsVariable(Name);
			}
		}
	}
}
