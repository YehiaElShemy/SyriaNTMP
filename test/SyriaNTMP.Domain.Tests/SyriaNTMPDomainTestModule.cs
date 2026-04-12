using Volo.Abp.Modularity;

namespace SyriaNTMP;

[DependsOn(
    typeof(SyriaNTMPDomainModule),
    typeof(SyriaNTMPTestBaseModule)
)]
public class SyriaNTMPDomainTestModule : AbpModule
{

}
