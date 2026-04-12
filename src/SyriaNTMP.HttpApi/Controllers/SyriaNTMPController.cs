using SyriaNTMP.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SyriaNTMP.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SyriaNTMPController : AbpControllerBase
{
    protected SyriaNTMPController()
    {
        LocalizationResource = typeof(SyriaNTMPResource);
    }
}
