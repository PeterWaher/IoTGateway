using System;
using System.Collections;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Enumerator that limits the return set to a maximum number of records.
    /// </summary>
    public class RecordEnumerator : IResultSetEnumerator
    {
        private readonly IResultSetEnumerator e;
        private readonly ScriptNode[] columns;
        private readonly Variables variables;
        private readonly int count;
        private IElement[] record;
        private ObjectProperties properties = null;

        /// <summary>
        /// Enumerator that limits the return set to a maximum number of records.
        /// </summary>
        /// <param name="ItemEnumerator">Item enumerator</param>
        /// <param name="Columns">Column definitions. Might be null if objects are to be returned.</param>
        /// <param name="Variables">Current set of variables.</param>
        public RecordEnumerator(IResultSetEnumerator ItemEnumerator, ScriptNode[] Columns, Variables Variables)
        {
            e = ItemEnumerator;
            columns = Columns;
            variables = Variables;
            count = columns?.Length ?? 0;
        }

        /// <summary>
        /// <see cref="IEnumerator.Current"/>
        /// </summary>
        public object Current => e.Current;

        /// <summary>
        /// Current record
        /// </summary>
        public IElement[] CurrentRecord => record;

        /// <summary>
        /// <see cref="IEnumerator.MoveNext"/>
        /// </summary>
        public virtual bool MoveNext()
        {
            return MoveNextAsync().Result;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if
        /// the enumerator has passed the end of the collection.</returns>
        /// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
        public virtual async Task<bool> MoveNextAsync()
        {
            if (!await e.MoveNextAsync())
                return false;

            int i;
            object Item = e.Current;
            IElement Element = Item as IElement;

            if (properties is null)
                properties = new ObjectProperties(Element?.AssociatedObjectValue ?? Item, variables);
            else
                properties.Object = Element?.AssociatedObjectValue ?? Item;

            if (columns is null)
                record = new IElement[1] { Element ?? Expression.Encapsulate(Item) };
            else
            {
                record = new IElement[count];

                for (i = 0; i < count; i++)
                {
                    try
                    {
                        record[i] = await columns[i].EvaluateAsync(properties);
                    }
                    catch (Exception ex)
                    {
                        ex = Log.UnnestException(ex);
                        record[i] = Expression.Encapsulate(ex);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// <see cref="IEnumerator.Reset"/>
        /// </summary>
        public virtual void Reset()
        {
            e.Reset();
        }

    }
}
