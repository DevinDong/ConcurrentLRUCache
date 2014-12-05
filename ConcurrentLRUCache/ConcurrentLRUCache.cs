using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentLRUCache
{
    /// <summary>
    /// The concurrent LRU Cache strategy(thread safe version).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ConcurrentLRUCache<TKey, TValue> : ICache<TKey, TValue>
    {
        #region const

        /// <summary>
        /// Cache Factor(threshold=capacity*factor).
        /// </summary>
        private const Double FACTOR = 0.8;

        /// <summary>
        /// The amount of time to delay before callback of the scan-timer is invoked, in milliseconds.
        /// </summary>
        private const Int32 DUE_TIME = 0;

        /// <summary>
        /// The time interval between invocations of callback of the scan-timer, in milliseconds.
        /// </summary>
        private const Int32 PERIOD = 10000;

        #endregion

        #region fields

        /// <summary>
        /// Capacity of the cache.
        /// </summary>
        private readonly UInt32 _capacity = 0;

        /// <summary>
        /// Threshold (threshold=capacity*factor).
        /// </summary>
        private readonly UInt32 _threshold = 0;


        /// <summary>
        /// The current cache size.
        /// </summary>
        private UInt32 _size = 0;

        /// <summary>
        /// The lock object.
        /// </summary>
        private readonly object _gate = new object();


        /// <summary>
        /// Cache container.
        /// </summary>
        private readonly Dictionary<TKey, TValue> _cache = null;

        /// <summary>
        /// The key collection of che cache.
        /// </summary>
        private readonly LinkedList<TKey> _keys = null;

        /// <summary>
        /// Store the relationship between Key and LinkedListNode(Key).
        /// </summary>
        private readonly Dictionary<TKey, LinkedListNode<TKey>> _mapping;

        /// <summary>
        /// Check if the size up to the threshold,if true,execute lru strategy.
        /// </summary>
        private Timer _scanner = null;

        #endregion

        #region properties

        /// <summary>
        /// The current size of the cache.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// The key collection of the cache.
        /// </summary>
        public LinkedList<TKey> Keys
        {
            get { return _keys; }
        }


        public UInt32 LRUPeroid
        {
            get { return PERIOD; }
        }

        #endregion

        #region constructors

        private ConcurrentLRUCache()
        {
            _keys = new LinkedList<TKey>();
            _mapping = new Dictionary<TKey, LinkedListNode<TKey>>();
            _cache = new Dictionary<TKey, TValue>();
            _scanner = new Timer(ScannerCallback, null, DUE_TIME, PERIOD);
        }

        public ConcurrentLRUCache(UInt32 capacity)
            : this()
        {
            _capacity = capacity;
            _threshold = (UInt32)(capacity * FACTOR);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Put a key,value pair into the cache.
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <param name="value">the cache value</param>
        public void Put(TKey key, TValue value)
        {
            lock (_gate)
            {
                if (_cache.ContainsKey(key))
                {
                    throw new ArgumentException(string.Format("Key:{0} has exsits in the cache.Try Update method.", key));
                }

                if (_size + 1 > _capacity)
                {
                    Lru();
                }

                var node = _keys.AddFirst(key);
                _mapping.Add(key, node);
                _cache.Add(key, value);
                _size++;
            }
        }

        /// <summary>
        /// Get cache item value,if not found,return the default value.
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            lock (_gate)
            {
                TValue ret = default(TValue);
                if (_cache.ContainsKey(key))
                {
                    //change the position of the key
                    ChangeListNodePosition(key);

                    _cache.TryGetValue(key, out ret);
                }
                return ret;
            }
        }

        /// <summary>
        /// Update cache item.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>

        public void Update(TKey key, TValue value)
        {
            lock (_gate)
            {
                if (_cache.ContainsKey(key))
                {
                    _cache[key] = value;
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("Update failed.Key not found:{0}", key));
                }
            }
        }


        /// <summary>
        /// Remove cache item if key found.
        /// </summary>
        /// <param name="key"></param>
        public void Delete(TKey key)
        {
            lock (_gate)
            {
                if (_cache.ContainsKey(key))
                {
                    //remove from keys
                    var node = _mapping[key];
                    _keys.Remove(node);

                    // remove from mapping
                    _mapping.Remove(key);

                    //remove from cache.
                    _cache.Remove(key);

                    _size--;
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("Delete failed.Key not found:{0}", key));
                }
            }
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        public void Clear()
        {
            lock (_gate)
            {
                _size = 0;
                _keys.Clear();
                _cache.Clear();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Callback for scan-timer.
        /// </summary>
        /// <param name="state"></param>
        private void ScannerCallback(object state)
        {
            Lru();
        }

        /// <summary>
        /// The LRU strategy.
        /// </summary>
        private void Lru()
        {
            lock (_gate)
            {
                while (_size > _threshold)
                {
                    TKey key = _keys.Last.Value;
                    _cache.Remove(key);
                    _mapping.Remove(key);
                    _keys.RemoveLast();

                    _size--;
                }
            }
        }

        /// <summary>
        /// Change the node positon in the linked list.
        /// </summary>
        /// <param name="key">the key.</param>
        private void ChangeListNodePosition(TKey key)
        {
            var node = _mapping[key];
            if (node != _keys.First)
            {
                _keys.Remove(node);

                node = _keys.AddFirst(key);
                _mapping[key] = node;
            }
        }

        #endregion
    }
}
