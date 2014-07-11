using System;
using System.Collections.Generic;

namespace Shard.Util
{
    /// <summary>Provides the abstract base class for a collection whose keys are embedded in the values.</summary>
    /// <typeparam name="TKey">The type of keys in the collection.</typeparam>
    /// <typeparam name="TItem">The type of items in the collection.</typeparam>
    public abstract class KeyedSyncCollection<TKey, TItem> : SyncCollection<TItem>
    {
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        /// The _dictionary
        /// </summary>
        private readonly Dictionary<TKey, TItem> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSyncCollection{TKey, TItem}"/> class.
        /// </summary>
        protected KeyedSyncCollection()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedSyncCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        protected KeyedSyncCollection(IEqualityComparer<TKey> comparer)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _dictionary = new Dictionary<TKey, TItem>();
        }

        /// <summary>
        /// Gets the key for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        protected abstract TKey GetKeyForItem(TItem item);

        /// <summary>
        /// Gets the item with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public TItem this[TKey key]
        {
            get
            {
                using (CreateReadBlock())
                {
                    return _dictionary[key];
                }
            }
            set
            {
                using (CreateWriteBlock())
                {
                    _dictionary[key] = value;
                }
            }
        }

        /// <summary>
        /// Removes the item with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            using (CreateWriteBlock())
            {
                return this._dictionary.ContainsKey(key) && Remove(this._dictionary[key]);
            }
        }

        /// <summary>
        /// Determines whether the specified key is present.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool Contains(TKey key)
        {
            if (Equals(key, default(TKey)))
                throw new ArgumentNullException("key");

            using (CreateReadBlock())
            {
                return this._dictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Gets the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TItem item)
        {
            using (CreateReadBlock())
            {
                return _dictionary.TryGetValue(key, out item);
            }
        }

        /// <summary>
        /// Adds an item if its key is not present.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if the item was added</returns>
        public bool AddIfKeyNotPresent(ref TItem item)
        {
            using (CreateWriteBlock())
            {
                var key = GetKeyForItem(item);

                if (_dictionary.ContainsKey(key))
                {
                    item = _dictionary[key];
                    return false;
                }

                Add(item);
                return true;
            }
        }


        /// <summary>
        /// Clears the items without locking.
        /// </summary>
        protected override void ClearItems()
        {
            _dictionary.Clear();

            base.ClearItems();
        }

        /// <summary>
        /// Inserts an item without locking.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, TItem item)
        {
            var keyForItem = this.GetKeyForItem(item);
            if (!Equals(keyForItem, default(TKey)))
            {
                this.AddKey(keyForItem, item);
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes an item without locking.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var keyForItem = this.GetKeyForItem(InternalCollection[index]);
            if (!Equals(keyForItem, default(TKey)))
            {
                this.RemoveKey(keyForItem);
            }

            base.RemoveItem(index);
        }

        /// <summary>
        /// Sets an item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, TItem item)
        {
            var keyForItem = this.GetKeyForItem(item);
            var keyForItem2 = this.GetKeyForItem(InternalCollection[index]);
            if (this._comparer.Equals(keyForItem2, keyForItem))
            {
                if (!Equals(keyForItem, default(TKey)))
                {
                    this._dictionary[keyForItem] = item;
                }
            }
            else
            {
                if (!Equals(keyForItem, default(TKey)))
                {
                    this.AddKey(keyForItem, item);
                }
                if (!Equals(keyForItem2, default(TKey)))
                {
                    this.RemoveKey(keyForItem2);
                }
            }

            base.SetItem(index, item);
        }

        private void AddKey(TKey key, TItem item)
        {
            _dictionary.Add(key, item);
        }

        private void RemoveKey(TKey key)
        {
            _dictionary.Remove(key);
        }
    }
}