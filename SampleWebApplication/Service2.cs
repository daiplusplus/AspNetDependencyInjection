using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleWebApplication
{
    public class Service2
    {
        private Service1 _service1;

        public Service2( Service1 svc1 )
        {
            _service1 = svc1;
        }

        public string SayHello()
        {
            return _service1.SayHello() + " (Called from Service2)";
        }
    }
}