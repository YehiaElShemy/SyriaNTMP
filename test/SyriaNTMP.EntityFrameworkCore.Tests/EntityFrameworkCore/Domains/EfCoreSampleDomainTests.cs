using SyriaNTMP.Samples;
using Xunit;

namespace SyriaNTMP.EntityFrameworkCore.Domains;

[Collection(SyriaNTMPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SyriaNTMPEntityFrameworkCoreTestModule>
{

}
