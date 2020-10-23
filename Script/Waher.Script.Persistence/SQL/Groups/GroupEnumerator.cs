using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Persistence.SQL.Groups
{
	/// <summary>
	/// Enumerator that groups items into groups, and returns aggregated elements.
	/// </summary>
	public class GroupEnumerator : IResultSetEnumerator
	{
		private readonly ScriptNode[] groupBy;
		private readonly ScriptNode[] groupNames;
		private readonly Variables variables;
		private readonly IResultSetEnumerator e;
		private bool processLast = false;
		private Type lastType = null;
		private IEnumerable<PropertyInfo> properties = null;
		private IEnumerable<FieldInfo> fields = null;
		private object current = null;
		private ObjectProperties objectVariables = null;

		/// <summary>
		/// Enumerator that groups items into groups, and returns aggregated elements.
		/// </summary>
		/// <param name="ItemEnumerator">Item enumerator</param>
		/// <param name="Variables">Current set of variables</param>
		/// <param name="GroupBy">Group on these fields</param>
		/// <param name="GroupNames">Names given to grouped fields</param>
		public GroupEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, ScriptNode[] GroupBy, ScriptNode[] GroupNames)
		{
			this.e = ItemEnumerator;
			this.variables = Variables;
			this.groupBy = GroupBy;
			this.groupNames = GroupNames;
		}

		/// <summary>
		/// <see cref="IEnumerator.Current"/>
		/// </summary>
		public object Current => this.current;

		/// <summary>
		/// <see cref="IEnumerator.MoveNext"/>
		/// </summary>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if
		/// the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public async Task<bool> MoveNextAsync()
		{
			Dictionary<string, List<object>> Aggregated = null;
			IElement E;
			object[] Last = null;
			int i, c = this.groupBy.Length;
			object o1, o2;

			while (this.processLast || await e.MoveNextAsync())
			{
				this.processLast = false;

				if (this.objectVariables is null)
					this.objectVariables = new ObjectProperties(e.Current, this.variables);
				else
					this.objectVariables.Object = e.Current;

				if (Last is null)
				{
					Last = new object[c];

					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(this.objectVariables);
						Last[i] = E.AssociatedObjectValue;
					}
				}
				else
				{
					for (i = 0; i < c; i++)
					{
						E = this.groupBy[i].Evaluate(this.objectVariables);

						o1 = Last[i];
						o2 = E.AssociatedObjectValue;

						if (o1 is null ^ o2 is null)
							break;

						if (!(o1 is null) && !o1.Equals(o2))
							break;
					}

					if (i < c)
					{
						this.processLast = true;
						break;
					}
				}

				o1 = e.Current;

				Type T = o1.GetType();
				if (T != this.lastType)
				{
					List<PropertyInfo> Properties = new List<PropertyInfo>();

					foreach (PropertyInfo PI in T.GetRuntimeProperties())
					{
						if (!PI.CanRead || !PI.CanWrite)
							continue;

						if (PI.GetIndexParameters().Length > 0)
							continue;

						Properties.Add(PI);
					}

					this.lastType = T;
					this.properties = Properties.ToArray();
					this.fields = T.GetRuntimeFields();
				}

				if (Aggregated is null)
					Aggregated = new Dictionary<string, List<object>>();

				foreach (PropertyInfo PI in this.properties)
				{
					if (!Aggregated.TryGetValue(PI.Name, out List<object> List))
					{
						List = new List<object>();
						Aggregated[PI.Name] = List;
					}

					List.Add(PI.GetValue(o1));
				}

				foreach (FieldInfo FI in this.fields)
				{
					if (!Aggregated.TryGetValue(FI.Name, out List<object> List))
					{
						List = new List<object>();
						Aggregated[FI.Name] = List;
					}

					List.Add(FI.GetValue(o1));
				}
			}

			if (Aggregated is null)
				return false;

			Dictionary<string, object> Result = new Dictionary<string, object>();
			bool First = true;

			foreach (KeyValuePair<string, List<object>> Rec in Aggregated)
			{
				object[] A = Rec.Value.ToArray();
				Result[Rec.Key] = A;

				if (First)
				{
					First = false;
					Result[" First "] = A;
				}
			}

			if (!(this.groupNames is null))
			{
				for (i = 0; i < c; i++)
				{
					ScriptNode Node = this.groupNames[i];
					if (Node is null)
						continue;

					if (Node is VariableReference Ref)
						Result[Ref.VariableName] = Last[i];
					else
					{
						E = this.groupNames[i]?.Evaluate(this.variables);
						if (!(E is null) && E is StringValue S)
							Result[S.Value] = Last[i];
					}
				}
			}

			this.current = Result;

			return true;
		}

		/// <summary>
		/// <see cref="IEnumerator.Reset"/>
		/// </summary>
		public void Reset()
		{
			this.e.Reset();
			this.processLast = false;
			this.current = null;
		}
	}
}
