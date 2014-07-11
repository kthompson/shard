using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Shard.Util
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncCollection<T> : IList<T>, IList
    {
        private readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The internal collection
        /// </summary>
        protected readonly Collection<T> InternalCollection = new Collection<T>();

        /// <summary>
        /// Creates the write block.
        /// </summary>
        /// <returns></returns>
        protected IDisposable CreateWriteBlock()
        {
            _sync.EnterWriteLock();

            return Disposable.Create(() => _sync.ExitWriteLock());
        }

        /// <summary>
        /// Creates the read block.
        /// </summary>
        /// <returns></returns>
        protected IDisposable CreateReadBlock()
        {
            _sync.EnterReadLock();

            return Disposable.Create(_sync.ExitReadLock);
        }

        #region IList<T> Implementation

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            using (CreateReadBlock())
            {
                return this.InternalCollection.IndexOf(item);
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, T item)
        {
            using (CreateWriteBlock())
            {
                InsertItem(index, item);
            }
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            using (CreateWriteBlock())
            {
                this.InternalCollection.RemoveAt(index);
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                using (CreateReadBlock())
                {
                    return GetItem(index);
                }
            }
            set
            {
                using (CreateWriteBlock())
                {
                    SetItem(index, value);
                }
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(T item)
        {
            using (CreateWriteBlock())
            {
                var count = this.InternalCollection.Count;
                InsertItem(count, item);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            using (CreateWriteBlock())
            {
                ClearItems();
            }
        }

        /// <summary>
        /// Clears the collection performing the specified action on each item.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ClearWithAction(Action<T> action)
        {
            using (CreateWriteBlock())
            {
                foreach (var item in this.InternalCollection)
                    action(item);

                ClearItems();
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            using (CreateReadBlock())
            {
                return this.InternalCollection.Contains(item);
            }
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            using (CreateReadBlock())
            {
                this.InternalCollection.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count
        {
            get
            {
                using (CreateReadBlock())
                {
                    return this.InternalCollection.Count;    
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(T item)
        {
            using (CreateWriteBlock())
            {
                int num = this.InternalCollection.IndexOf(item);
                if (num < 0)
                    return false;

                RemoveItem(num);
                return true;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            using (CreateReadBlock())
            {
                return this.InternalCollection.GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        #endregion

        #region IList Implementation

        int IList.Add(object value)
        {
            using (CreateWriteBlock())
            {
                return (this.InternalCollection as IList).Add(value);
            }
        }

        void IList.Clear()
        {
            (this as IList<T>).Clear();
        }

        bool IList.Contains(object value)
        {
            using (CreateReadBlock())
            {
                return (this.InternalCollection as IList).Contains(value);
            }
        }

        int IList.IndexOf(object value)
        {
            using (CreateReadBlock())
            {
                return (this.InternalCollection as IList).IndexOf(value);
            }
        }

        void IList.Insert(int index, object value)
        {
            using (CreateWriteBlock())
            {
                (this.InternalCollection as IList).Add(value);
            }
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            using (CreateWriteBlock())
            {
                (this.InternalCollection as IList).Remove(value);
            }
        }

        void IList.RemoveAt(int index)
        {
            (this as IList<T>).RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return (this as IList<T>)[index];
            }
            set
            {
                using (CreateWriteBlock())
                {
                    (this.InternalCollection as IList)[index] = value;
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            using (CreateReadBlock())
            {
                (this.InternalCollection as IList).CopyTo(array, index);
            }
        }

        int ICollection.Count
        {
            get { return this.InternalCollection.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inserts the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected virtual void InsertItem(int index, T item)
        {
            this.InternalCollection.Insert(index, item);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual void RemoveItem(int index)
        {
            this.InternalCollection.RemoveAt(index);
        }

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        protected virtual void SetItem(int index, T value)
        {
            this.InternalCollection[index] = value;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual T GetItem(int index)
        {
            return this.InternalCollection[index];
        }

        /// <summary>
        /// Clears the items.
        /// </summary>
        protected virtual void ClearItems()
        {
            this.InternalCollection.Clear();
        }
        #endregion
    }
}