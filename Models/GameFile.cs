using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace launher
{
    public class GameFile
    {
        public string UserPath { get; set; }
        public string ServerPath { get; set; }
        public long ServerFileSize { get; set; }
        public long CurrentFileSize { get; set; }
        public byte[] bytes { get; set; }
    }
}
