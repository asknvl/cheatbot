using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Database.models
{
    public class AppSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? RootPath { get; set; }
        public string? TGPath { get; set; }
        public string? ProxyString { get; set; }
        public string? ChannelsURL { get; set; }
        public string? ChannelsToken { get; set;}
          
    }
}
