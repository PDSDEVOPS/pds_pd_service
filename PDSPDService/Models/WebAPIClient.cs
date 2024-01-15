using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductionWebService.Models
{
    public class WebAPIClient
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public int TokenExpirationMinutes { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual WebAPIUser WebAPIUser { get; set; }
    }
}