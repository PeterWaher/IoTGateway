using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Model;
using Waher.Script.Exceptions;

namespace Waher.Script.Persistence.SQL.Enumerators
{
    /// <summary>
    /// Enumerator that reorders a sequence of items.
    /// </summary>
    public class CustomOrderEnumerator : IResultSetEnumerator
    {
        private readonly Dictionary<Type, ObjectProperties> propertiesX = new Dictionary<Type, ObjectProperties>();
        private readonly Dictionary<Type, ObjectProperties> propertiesY = new Dictionary<Type, ObjectProperties>();
        private readonly KeyValuePair<ScriptNode, bool>[] order;
        private readonly IResultSetEnumerator items;
        private readonly Variables variables;
        private IEnumerator e = null;

        /// <summary>
        /// Enumerator that reorders a sequence of items.
        /// </summary>
        /// <param name="ItemEnumerator">Item enumerator</param>
        /// <param name="Variables">Current set of variables.</param>
        /// <param name="Order">Custom order.</param>
        public CustomOrderEnumerator(IResultSetEnumerator ItemEnumerator, Variables Variables, KeyValuePair<ScriptNode, bool>[] Order)
        {
            order = Order;
            items = ItemEnumerator;
            variables = Variables;
        }

        /// <summary>
        /// <see cref="IEnumerator.Current"/>
        /// </summary>
        public object Current => e.Current;

        /// <summary>
        /// <see cref="IEnumerator.MoveNext"/>
        /// </summary>
        public bool MoveNext()
        {
            return MoveNextAsync().Result;
        }

        /// <summary>
        /// <see cref="IEnumerator.MoveNext"/>
        /// </summary>
        public async Task<bool> MoveNextAsync()
        {
            if (e is null)
            {
                List<object> Items = new List<object>();

                while (await items.MoveNextAsync())
                    Items.Add(items.Current);

                Items.Sort((x, y) =>
                {
                    if (x is null)
                    {
                        if (y is null)
                            return 0;
                        else
                            return -1;
                    }
                    else if (y is null)
                        return 1;

                    Type Tx = x.GetType();
                    Type Ty = y.GetType();

                    if (propertiesX.TryGetValue(Tx, out ObjectProperties Vx))
                        Vx.Object = x;
                    else
                    {
                        Vx = new ObjectProperties(x, variables);
                        propertiesX[Tx] = Vx;
                    }

                    if (propertiesY.TryGetValue(Ty, out ObjectProperties Vy))
                        Vy.Object = y;
                    else
                    {
                        Vy = new ObjectProperties(y, variables);
                        propertiesY[Ty] = Vy;
                    }

                    int i, j, c = order.Length;
                    IElement Ex, Ey;
                    ScriptNode Node;

                    for (i = 0; i < c; i++)
                    {
                        Node = order[i].Key;
                        Ex = Node.Evaluate(Vx);
                        Ey = Node.Evaluate(Vy);

                        if (!(Ex.AssociatedSet is IOrderedSet S))
                            throw new ScriptRuntimeException("Result not member of an ordered set.", Node);

                        j = S.Compare(Ex, Ey);
                        if (j != 0)
                        {
                            if (order[i].Value)
                                return j;
                            else
                                return -j;
                        }
                    }

                    return 0;
                });

                e = Items.GetEnumerator();
            }

            return e.MoveNext();
        }

        /// <summary>
        /// <see cref="IEnumerator.Reset"/>
        /// </summary>
        public void Reset()
        {
            items.Reset();
            e = null;
        }
    }
}
