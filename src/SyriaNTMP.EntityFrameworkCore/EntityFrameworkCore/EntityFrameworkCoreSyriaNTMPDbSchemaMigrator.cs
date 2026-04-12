using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SyriaNTMP.Data;
using Volo.Abp.DependencyInjection;

namespace SyriaNTMP.EntityFrameworkCore;

public class EntityFrameworkCoreSyriaNTMPDbSchemaMigrator
    : ISyriaNTMPDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSyriaNTMPDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SyriaNTMPDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SyriaNTMPDbContext>()
            .Database
            .MigrateAsync();
    }
}
