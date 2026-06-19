using System;
using System.Threading.Tasks;
using MySqlConnector;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.MySQL.Model;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Data.MySQL.Functions
{
	/// <summary>
	/// Creates a connection to an external MySQL database.
	/// </summary>
	public class ConnectMySql : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a connection to an external MySQL database.
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
		/// Creates a connection to an external MySQL database.
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
		/// Creates a connection to an external MySQL database.
		/// </summary>
		/// <param name="Host">Host machine of database.</param>
		/// <param name="Database">Database to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMySql(ScriptNode Host, ScriptNode Database, ScriptNode UserName, ScriptNode Password, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Database, UserName, Password }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ConnectMySql);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Host", "Database", "UserName", "Password" };

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
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
			string ConnectionString = ToString(Arguments[0]) ?? string.Empty;
			MySqlConnection Connection;

			switch (Arguments.Length)
			{
				case 1:
				default:
					break;

				case 3:
					string UserName = ToString(Arguments[1]) ?? string.Empty;
					string Password = ToString(Arguments[2]) ?? string.Empty;

					ConnectionString += ";User ID=" + UserName + ";Password=" + Password;
					break;

				case 4:
					string Database = ToString(Arguments[1]) ?? string.Empty;
					UserName = ToString(Arguments[2]) ?? string.Empty;
					Password = ToString(Arguments[3]) ?? string.Empty;

					ConnectionString = "Server=" + ConnectionString + ";Database=" + Database + ";User ID=" + UserName + ";Password=" + Password;
					break;
			}

			Connection = new MySqlConnection(ConnectionString);
			await Connection.OpenAsync();

			return new ObjectValue(new MySqlDatabase(Connection));
		}
	}
}

