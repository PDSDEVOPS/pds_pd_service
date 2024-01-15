using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionWebService.Models
{
    public class ExSysRouteAccess
    {
        [Key]
        public int Id { get; set; }
        public string RouteUrl { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual WebAPIUser WebAPIUser { get; set; }
        public string IPAddress { get; set; }
        public DateTime? StopDt { get; set; }
    }
}