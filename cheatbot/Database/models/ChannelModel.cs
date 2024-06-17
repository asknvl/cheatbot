using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Database.models
{
    public class ChannelModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? geotag { get; set; }
        public string? link { get; set; }
        public long tg_id { get; set; }        
        public bool is_active { get; set; }
        public string? name { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is ChannelModel other)
            {
                return this.tg_id == other.tg_id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.tg_id.GetHashCode();
        }

    }
}
