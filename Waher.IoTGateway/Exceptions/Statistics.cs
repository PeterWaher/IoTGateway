using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Exceptions
{
    internal class Statistics
    {
        internal Histogram<string> PerType = new Histogram<string>();
        internal Histogram<string> PerMessage = new Histogram<string>();
        internal Histogram<string> PerSource = new Histogram<string>();
        internal Histogram<DateTime> PerHour = new Histogram<DateTime>();
        internal Histogram<DateTime> PerDay = new Histogram<DateTime>();
        internal Histogram<DateTime> PerMonth = new Histogram<DateTime>();
    }
}
