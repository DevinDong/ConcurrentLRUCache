using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConcurrentLRUCache
{
    interface ICache<TKey,TValue>
    {
        void Put(TKey key, TValue value);
        
        TValue Get(TKey key);

        void Delete(TKey key);

        void Update(TKey key, TValue value);
    }
}
