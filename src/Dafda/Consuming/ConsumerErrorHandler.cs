using System;
using System.Threading.Tasks;
using Dafda.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dafda.Consuming
{
    internal delegate Task<ConsumerFailureStrategy> EvaluateError(Exception exception);

    internal class ConsumerErrorHandler
    {
        private readonly EvaluateError _evaluateError;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ConsumerErrorHandler(EvaluateError evaluateError, IHostApplicationLifetime applicationLifetime)
        {
            _evaluateError = evaluateError;
            _applicationLifetime = applicationLifetime;
        }

        public async Task<bool> HandleError(Exception exception)
        {

            var failureStrategy = await _evaluateError(exception);
            if (failureStrategy == ConsumerFailureStrategy.Default)
            {
                _applicationLifetime.StopApplication();
                return false;
            }
            return true;
        }
    }
}