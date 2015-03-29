using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JScript;
using Nancy;

namespace FeelKnitService
{
    public class LoggingErrorHandler : IErrorHandler
    {

        public bool HandlesStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            object errorObject;
            context.Items.TryGetValue(NancyEngine.ERROR_EXCEPTION, out errorObject);
            var error = errorObject as Exception;

            //_logger.Error("Unhandled error", error);
        }

        public bool OnCompilerError(IVsaFullErrorInfo error)
        {
            return false;
        }
    }
}
