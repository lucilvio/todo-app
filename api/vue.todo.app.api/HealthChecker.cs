using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Vue.TodoApp
{
    public class HealthChecker : IHealthCheck
    {
        private readonly TodoAppContext _context;

        public HealthChecker(TodoAppContext context)
        {
            this._context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await _context.Database.CanConnectAsync();
            
            return HealthCheckResult.Healthy();
        }
    }
}