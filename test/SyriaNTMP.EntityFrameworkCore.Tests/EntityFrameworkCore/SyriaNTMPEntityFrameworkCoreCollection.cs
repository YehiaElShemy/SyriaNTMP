using Xunit;

namespace SyriaNTMP.EntityFrameworkCore;

[CollectionDefinition(SyriaNTMPTestConsts.CollectionDefinitionName)]
public class SyriaNTMPEntityFrameworkCoreCollection : ICollectionFixture<SyriaNTMPEntityFrameworkCoreFixture>
{

}
