using SyriaNTMP.Localization;
using Volo.Abp.Application.Services;

namespace SyriaNTMP;

/* Inherit your application services from this class.
 */
public abstract class SyriaNTMPAppService : ApplicationService
{
    protected SyriaNTMPAppService()
    {
        LocalizationResource = typeof(SyriaNTMPResource);
    }
}
