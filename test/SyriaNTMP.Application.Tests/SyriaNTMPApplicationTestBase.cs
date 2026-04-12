using Volo.Abp.Modularity;

namespace SyriaNTMP;

public abstract class SyriaNTMPApplicationTestBase<TStartupModule> : SyriaNTMPTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
