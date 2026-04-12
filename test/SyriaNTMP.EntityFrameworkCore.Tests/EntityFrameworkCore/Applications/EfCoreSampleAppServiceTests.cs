using SyriaNTMP.Samples;
using Xunit;

namespace SyriaNTMP.EntityFrameworkCore.Applications;

[Collection(SyriaNTMPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<SyriaNTMPEntityFrameworkCoreTestModule>
{

}
