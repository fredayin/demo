using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Framework
{
    public class UserEventArgs : EventArgs
    {
        public string UserName { get; set; }

        public string ErrorCode { get; set; }
    }
}
