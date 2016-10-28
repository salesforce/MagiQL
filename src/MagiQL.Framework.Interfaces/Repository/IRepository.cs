using System.Data;

namespace MagiQL.Framework.Interfaces.Repository
{
    public interface IRepository
    {
        IDbTransaction CreateTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}