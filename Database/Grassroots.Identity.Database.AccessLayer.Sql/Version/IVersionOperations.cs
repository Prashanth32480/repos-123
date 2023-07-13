using System.Collections.Generic;
using System.Threading.Tasks;
using Grassroots.Identity.Database.Model.DbEntity;

namespace Grassroots.Identity.Database.AccessLayer.Sql
{
    public interface IVersionOperations
    {
        Task<IEnumerable<Version>> GetVersion();
    }
}