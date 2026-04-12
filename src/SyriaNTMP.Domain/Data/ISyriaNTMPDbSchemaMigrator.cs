using System.Threading.Tasks;

namespace SyriaNTMP.Data;

public interface ISyriaNTMPDbSchemaMigrator
{
    Task MigrateAsync();
}
