using Entities;
using StoreDataManager;

namespace QueryProcessor.Operations
{
    internal class SetDatabase
    {
        internal OperationStatus Execute(string dbName)
        {
            return Store.GetInstance().SetDatabase(dbName);
        }
    }
}
