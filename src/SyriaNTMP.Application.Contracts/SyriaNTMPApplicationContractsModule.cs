using Volo.Abp.Account;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;

namespace SyriaNTMP;

[DependsOn(
    typeof(SyriaNTMPDomainSharedModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule)
)]
public class SyriaNTMPApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SyriaNTMPDtoExtensions.Configure();
    }
}
