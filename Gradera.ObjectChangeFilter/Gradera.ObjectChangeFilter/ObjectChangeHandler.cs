using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gradera.ObjectChangeFilter
{
    internal class ObjectChangeHandler
    {
        private static ObjectChangeHandler _instance;
        public static ObjectChangeHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ObjectChangeHandler();

                return _instance;
            }
        }

        public List<ObjectChangeModel> SaveList { get; set; }

        public ObjectChangeHandler()
        {
            SaveList = new List<ObjectChangeModel>();
        }
    }
}
