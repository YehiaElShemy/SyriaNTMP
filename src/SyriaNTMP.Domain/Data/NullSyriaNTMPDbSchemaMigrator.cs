using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SyriaNTMP.Data;

/* This is used if database provider does't define
 * ISyriaNTMPDbSchemaMigrator implementation.
 */
public class NullSyriaNTMPDbSchemaMigrator : ISyriaNTMPDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
