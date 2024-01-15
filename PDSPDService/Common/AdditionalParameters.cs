using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProductionWebService.Common
{
    public class AdditionalParameters 
    {
        public string[] IntegerFields { get; set; }
        public string[] DecimalFields { get; set; }
        public string[] DateTimeFields { get; set; }
        public string[] DoubleFields { get; set; }
    }
}