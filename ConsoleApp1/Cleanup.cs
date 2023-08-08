using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGtoOBJGen
{
    internal class Cleanup
    {
        private List<Delegate> DelegateList;
        public Cleanup() 
        {
            
        }
        public void ExecuteCleanup()
        {
            foreach(Delegate item in DelegateList)
            {
               
            }
        }
        public void AddDelegate(Delegate d)
        {
            DelegateList.Add(d);
        }
    }
}
