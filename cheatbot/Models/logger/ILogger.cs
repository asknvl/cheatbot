using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.logger
{
    public interface ILogger
    {
        bool DisableFileOutput { get; set; }
        void dbg(string tag, string text);
        void err(string tag, string text);
        void inf(string tag, string text);
        void inf_urgent(string tag, string text);
    }
}
