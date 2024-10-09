using Entities;
using StoreDataManager;

namespace QueryProcessor.Operations
{
    internal class CreateDatabase
    {
        internal OperationStatus Execute(string dbName)
        {
            return Store.GetInstance().CreateDatabase(dbName);
        }
    }
}
