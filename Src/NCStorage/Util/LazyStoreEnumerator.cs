using System.Collections;

namespace Alachisoft.NCache.Storage.Util
{
    /// <summary>
    /// provides Enumerator over RAMCacheStore
    /// </summary>
    internal class LazyStoreEnumerator : IDictionaryEnumerator
    {
        /// <summary> instance of StorageProviderBase </summary>
        private StorageProviderBase _store = null;

        /// <summary> Enumerator over Keys  </summary>
        IDictionaryEnumerator _enumerator;

        /// <summary> Dictionary Entry  </summary>
        DictionaryEntry _de;

        /// <summary>
        /// Constructs StorageProviderBase Enumerator 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="etr"></param>
        public LazyStoreEnumerator(StorageProviderBase store, IDictionaryEnumerator enumerator)
        {
            _store = store;
            _enumerator = enumerator;
        }

        #region	/                 --- IEnumerator ---           /

        /// <summary>
        /// Set the enumerator to its initial position. which is before the first element in the collection
        /// </summary>
        void IEnumerator.Reset()
        {
            _enumerator.Reset();
        }

        /// <summary>
        /// Advance the enumerator to the next element of the collection 
        /// </summary>
        /// <returns></returns>
        bool IEnumerator.MoveNext()
        {
            if (_enumerator.MoveNext())
            {
                _de = new DictionaryEntry(_enumerator.Key, null);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current element in the collection
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                if (_de.Value == null)
                {
                    _de.Value = _store.Get(_de.Key);
                }

                return _de;
            }
        }

        #endregion

        #region	/                 --- IDictionaryEnumerator ---           /

        /// <summary>
        /// Gets the key and value of the current dictionary entry.
        /// </summary>
        DictionaryEntry IDictionaryEnumerator.Entry
        {
            get
            {
                if (_de.Value == null)
                {
                    _de.Value = _store.Get(_de.Key);
                }

                return _de;
            }
        }

        /// <summary>
        /// Gets the key of the current dictionary entry 
        /// </summary>
        object IDictionaryEnumerator.Key
        {
            get { return _de.Key; }
        }

        /// <summary>
        /// Gets the value of the current dictionary entry
        /// </summary>
        object IDictionaryEnumerator.Value
        {
            get
            {
                if (_de.Value == null)
                {
                    _de.Value = _store.Get(_de.Key);
                }

                return _de.Value;
            }
        }

        #endregion
    }
}