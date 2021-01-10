using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealMatch.Lib.Storage
{
    public class Store<T> where T : class, ITableEntity, new()
    {
        private readonly CloudTable _table;

        public Store(IConfiguration cfg)
        {
            var acct = CloudStorageAccount.Parse(cfg["Storage"]);
            _table = acct.CreateCloudTableClient().GetTableReference(cfg["Table"]);
        }

        public async IAsyncEnumerable<T> GetPartitionQuery(string pk)
        {
            var q = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", "eq", pk));
            var task = _table.ExecuteQuerySegmentedAsync(q, null);
            while (task != null)
            {
                var seg = await task;

                foreach (var o in seg.Results)
                {
                    yield return o;
                }

                task = null;
                var tok = seg.ContinuationToken;
                if (tok != null)
                {
                    task = _table.ExecuteQuerySegmentedAsync(q, tok);
                }
            }
        }

        public async Task<T> GetAsync(string pk, string rk)
        {
            var op = TableOperation.Retrieve<T>(pk, rk);
            var response = await _table.ExecuteAsync(op);
            if (response.Result == null)
            {
                throw new NotFoundException($"{typeof(T).Name} not found");
            }
            return response.Result as T;
        }

        public async Task CreateAsync(T obj)
        {
            var op = TableOperation.Insert(obj);
            await _table.ExecuteAsync(op);
        }

        public async Task UpdateAsync(T obj)
        {
            var op = TableOperation.Replace(obj);
            await _table.ExecuteAsync(op);
        }

        public async Task DeleteAsync(T obj)
        {
            var op = TableOperation.Delete(obj);
            await _table.ExecuteAsync(op);
        }
    }
}
