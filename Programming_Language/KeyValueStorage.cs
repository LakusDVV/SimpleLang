using System;
using System.Collections.Generic;

namespace SimpleLang
{
    /// <summary>
    /// Generic key-value storage for variables of the language.
    /// </summary>
    public class KeyValueStorage<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _storage = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Add new key-value pair. Throws if key already exists.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (_storage.ContainsKey(key))
                throw new ArgumentException("Key '" + key + "' already exists.");

            _storage[key] = value;
        }

        /// <summary>
        /// Get value by key. Throws if not found.
        /// </summary>
        public TValue Get(TKey key)
        {
            TValue value;
            if (_storage.TryGetValue(key, out value))
                return value;

            throw new KeyNotFoundException("Key '" + key + "' not found.");
        }

        /// <summary>
        /// Update an existing value by key. Throws if key does not exist.
        /// </summary>
        public void Update(TKey key, TValue newValue)
        {
            if (!_storage.ContainsKey(key))
                throw new KeyNotFoundException("Key '" + key + "' not found for update.");

            _storage[key] = newValue;
        }

        /// <summary>
        /// Check if key exists.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return _storage.ContainsKey(key);
        }

        /// <summary>
        /// Remove key-value pair. Throws if key does not exist.
        /// </summary>
        public void Remove(TKey key)
        {
            if (!_storage.Remove(key))
                throw new KeyNotFoundException("Key '" + key + "' not found for removal.");
        }

        /// <summary>
        /// Clear all stored variables.
        /// </summary>
        public void Clear()
        {
            _storage.Clear();
        }

        /// <summary>
        /// Return internal dictionary (read-only style).
        /// </summary>
        public Dictionary<TKey, TValue> GetDictionary()
        {
            return _storage;
        }

        /// <summary>
        /// Print all variables for debug.
        /// </summary>
        public void PrintAll()
        {
            foreach (var kv in _storage)
                Console.WriteLine(kv.Key + " = " + kv.Value);
        }

        /// <summary>
        /// Count of variables.
        /// </summary>
        public int Count
        {
            get { return _storage.Count; }
        }
    }
}