using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cheatbot.Database.models
{
    public class DropSubscribeModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int drop_id { get; set; }
        public int channel_id { get; set; }

        public DropSubscribeModel(int drop_id, int channel_id)
        {            
            this.drop_id = drop_id;
            this.channel_id = channel_id;
        }
    }
}
