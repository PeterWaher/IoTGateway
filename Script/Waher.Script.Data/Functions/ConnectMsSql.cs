using Microsoft.Data.SqlClient;
using System.Security;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Data.Functions
{
	/// <summary>
	/// Creates a connection to an external MS SQL database.
	/// </summary>
	public class ConnectMsSql : FunctionMultiVariate
	{
		/// <summary>
		/// Creates a connection to an external MS SQL database.
		/// </summary>
		/// <param name="ConnectionString">Connection string.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMsSql(ScriptNode ConnectionString, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { ConnectionString }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a connection to an external MS SQL database.
		/// </summary>
		/// <param name="Host">Host machine of database.</param>
		/// <param name="Database">Database to connect to.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMsSql(ScriptNode Host, ScriptNode Database, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Database }, argumentTypes2Scalar, Start, Length, Expression)
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
		public ConnectMsSql(ScriptNode ConnectionString, ScriptNode UserName, ScriptNode Password, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { ConnectionString, UserName, Password }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Creates a connection to an external MS SQL database.
		/// </summary>
		/// <param name="Host">Host machine of database.</param>
		/// <param name="Database">Database to connect to.</param>
		/// <param name="UserName">User Name</param>
		/// <param name="Password">Password</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression.</param>
		public ConnectMsSql(ScriptNode Host, ScriptNode Database, ScriptNode UserName, ScriptNode Password, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Host, Database, UserName, Password }, argumentTypes4Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(ConnectMsSql);

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
			SqlConnection Connection;

			switch (Arguments.Length)
			{
				case 1:
				default:
					Connection = new SqlConnection(ConnectionString);
					break;

				case 2:
					string Database = ToString(Arguments[1]) ?? string.Empty;

					ConnectionString = "Data Source=" + ConnectionString + ";Initial Catalog=" + Database + ";Integrated Security=true";
					Connection = new SqlConnection(ConnectionString);
					break;
				case 3:
					string UserName = ToString(Arguments[1]) ?? string.Empty;
					string Password = ToString(Arguments[2]) ?? string.Empty;
					SecureString Password2 = new SecureString();

					foreach (char ch in Password)
						Password2.AppendChar(ch);

					Password2.MakeReadOnly();
					Connection = new SqlConnection(ConnectionString, new SqlCredential(UserName, Password2));
					break;

				case 4:
					Database = ToString(Arguments[1]) ?? string.Empty;
					UserName = ToString(Arguments[2]) ?? string.Empty;
					Password = ToString(Arguments[3]) ?? string.Empty;
					Password2 = new SecureString();

					foreach (char ch in Password)
						Password2.AppendChar(ch);

					Password2.MakeReadOnly();

					ConnectionString = "Data Source=" + ConnectionString + ";Initial Catalog=" + Database;
					Connection = new SqlConnection(ConnectionString, new SqlCredential(UserName, Password2));
					break;
			}

			await Connection.OpenAsync();

			return new ObjectValue(new MsSqlDatabase(Connection));
		}
	}
}
