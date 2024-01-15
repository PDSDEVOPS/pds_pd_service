using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionWebService.Models
{
    public class UserRouteQueryParameter
    {
        [Key]
        public int Id { get; set; }
        public int ExSysRouteAccessId { get; set; }
        [ForeignKey("ExSysRouteAccessId")]
        public virtual ExSysRouteAccess ExSysRouteAccess { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Folder { get; set; }
        public string FileMatch { get; set; }
        public string AdditionalParameters { get; set; }
    }
}