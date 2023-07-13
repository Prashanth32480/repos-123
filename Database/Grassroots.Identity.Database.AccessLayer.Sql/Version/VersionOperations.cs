using System.Collections.Generic;
using System.Threading.Tasks;
using Grassroots.Database.Infrastructure.Sql;
using Grassroots.Identity.Database.AccessLayer.Sql.RequestModel;

namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public class VersionOperations : IVersionOperations
    {
        private readonly IDatabaseConnectionFactory _factory;
        public VersionOperations(IDatabaseConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Model.DbEntity.Version>> GetVersion()
        {
            using var connection = _factory.CreateConnection();
            var resultCollection =
                await connection.ExecuteResultCollectionAsync<Model.DbEntity.Version, VersionRequestModel>(
                    DatabaseStoreProcedures.VersionGet, new VersionRequestModel { Limit = 1 });
            return resultCollection;
        }
    }
}