using System;
using System.Data.SqlClient;
using System.Security;
using System.Threading.Tasks;
using MySqlConnector;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.MySQL.Model;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Data.MySQL.Functions
{
	/// <summary>
	/// Creates a connection to an external MS SQL database.
	/// </summary>
	public class ConnectMySql : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a connection to an external MS SQL database.
		/// </summary>
		/// <param name="ConnectionString">Connection string.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMySql(ScriptNode ConnectionString, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { ConnectionString }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a connection to an external MS SQL database.
		/// </summary>
		/// <param name="ConnectionString">Connection string.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMySql(ScriptNode ConnectionString, ScriptNode UserName, ScriptNode Password, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { ConnectionString, UserName, Password }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "ConnectMySql";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "ConnectionString", "UserName", "Password" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			string ConnectionString = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;
			MySqlConnection Connection;

			switch (Arguments.Length)
			{
				case 1:
				default:
					Connection = new MySqlConnection(ConnectionString);
					break;

				case 3:
					string UserName = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
					string Password = Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty;

					ConnectionString += ";User ID=" + UserName + ";Password=" + Password;
					Connection = new MySqlConnection(ConnectionString);
					break;
			}

			await Connection.OpenAsync();

			return new ObjectValue(new MySqlDatabase(Connection));
		}
	}
}

