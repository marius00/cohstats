using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoHStats.Websocket {
    class OnSocketReadEventArg : EventArgs {
        public string Id { get; set; }
        public string Content { get; set; }
    }
}
