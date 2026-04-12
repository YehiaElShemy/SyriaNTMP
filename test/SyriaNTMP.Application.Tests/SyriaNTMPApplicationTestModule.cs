using Volo.Abp.Modularity;

namespace SyriaNTMP;

[DependsOn(
    typeof(SyriaNTMPApplicationModule),
    typeof(SyriaNTMPDomainTestModule)
)]
public class SyriaNTMPApplicationTestModule : AbpModule
{

}
