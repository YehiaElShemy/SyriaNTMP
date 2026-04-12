using SyriaNTMP.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SyriaNTMP.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SyriaNTMPEntityFrameworkCoreModule),
    typeof(SyriaNTMPApplicationContractsModule)
)]
public class SyriaNTMPDbMigratorModule : AbpModule
{
}
