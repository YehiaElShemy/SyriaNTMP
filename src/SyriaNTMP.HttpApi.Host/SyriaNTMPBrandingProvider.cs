using Microsoft.Extensions.Localization;
using SyriaNTMP.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SyriaNTMP;

[Dependency(ReplaceServices = true)]
public class SyriaNTMPBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<SyriaNTMPResource> _localizer;

    public SyriaNTMPBrandingProvider(IStringLocalizer<SyriaNTMPResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
