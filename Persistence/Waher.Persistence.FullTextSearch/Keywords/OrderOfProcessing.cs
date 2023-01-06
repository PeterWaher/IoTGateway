using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Keywords
{
    /// <summary>
    /// Orders strings in descending length order
    /// </summary>
    public class OrderOfProcessing : IComparer<Keyword>
    {
        /// <summary>
        /// Orders strings in descending length order
        /// </summary>
        public OrderOfProcessing()
        {
        }

        /// <summary>
        /// <see cref="IComparer<string>.Compare"/>
        /// </summary>
        public int Compare(Keyword x, Keyword y)
        {
            int i = y.OrderCategory - x.OrderCategory;
            if (i != 0)
                return i;

            i = y.OrderComplexity - x.OrderComplexity;
            if (i != 0) 
                return i;

            return string.Compare(x.ToString(), y.ToString());
        }
    }
}
