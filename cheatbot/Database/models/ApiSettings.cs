using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Database.models
{    
    public class ApiSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id {  get; set; }
        public int? group { get; set; }  
        public string? phone_number { get; set; }
        public string? api_id {  get; set; }
        public string? api_hash { get; set; }
        public string? password { get; set; }
        public string? description { get; set; }
    }
}
