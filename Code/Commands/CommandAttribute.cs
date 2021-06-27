using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Commands {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute {


    }

}
