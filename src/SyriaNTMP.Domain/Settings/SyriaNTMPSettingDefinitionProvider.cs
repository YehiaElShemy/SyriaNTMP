using Volo.Abp.Settings;

namespace SyriaNTMP.Settings;

public class SyriaNTMPSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
       context.Add(new SettingDefinition(SyriaNTMPSettings.GetCurrenies));
    }
}
