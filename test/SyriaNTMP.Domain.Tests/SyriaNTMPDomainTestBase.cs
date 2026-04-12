using Volo.Abp.Modularity;

namespace SyriaNTMP;

/* Inherit from this class for your domain layer tests. */
public abstract class SyriaNTMPDomainTestBase<TStartupModule> : SyriaNTMPTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
