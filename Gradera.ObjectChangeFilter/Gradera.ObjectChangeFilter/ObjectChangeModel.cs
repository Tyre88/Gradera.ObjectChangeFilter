using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gradera.ObjectChangeFilter
{
    internal class ObjectChangeModel
    {
        public string ObjectHash { get; set; }
        public string AssemblyHash { get; set; }
        public DateTime GetDate { get; set; }
    }
}
