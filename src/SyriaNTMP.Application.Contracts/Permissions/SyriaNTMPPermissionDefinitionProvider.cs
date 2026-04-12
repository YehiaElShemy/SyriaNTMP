using SyriaNTMP.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace SyriaNTMP.Permissions;

public class SyriaNTMPPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SyriaNTMPPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(SyriaNTMPPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SyriaNTMPResource>(name);
    }
}
