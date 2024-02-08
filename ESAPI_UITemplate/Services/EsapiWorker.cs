using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;

namespace ESAPI_UITemplate.Services
{
    public class EsapiWorker
    {
        private readonly Application _app;
        private readonly Dispatcher _dispatcher;

        public EsapiWorker(Application scriptContext)
        {
            _app = scriptContext;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Run(Action<Application> a)
        {
            _dispatcher.BeginInvoke(a, _app);
        }

        public void RunWithWait(Action<Application> a)
        {
            _dispatcher.BeginInvoke(a, _app).Wait();
        }
    }
}
