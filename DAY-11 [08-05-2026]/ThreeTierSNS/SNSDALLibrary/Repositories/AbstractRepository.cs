namespace SNSDALLibrary
{
    public abstract class AbstractRepository<K, T> : IRepository<K, T> where T : class where K : notnull
    {
        protected Dictionary<K, T> _items;

        protected AbstractRepository()
        {
            _items = new Dictionary<K, T>();
        }

        public abstract T Create(T item);

        public virtual T? Delete(K key)
        {
            if (!_items.ContainsKey(key))
                return null;
                
            var item = _items[key];
            _items.Remove(key);
            return item;
        }

        public virtual T? GetAccount(K key)
        {
            if (_items.ContainsKey(key))
                return _items[key];
            return null;
        }

        public virtual List<T>? GetAccounts()
        {
            if (_items.Count == 0) 
                return null;
            
            return _items.Values.ToList();
        }

        public virtual T? Update(K key, T item)
        {
            if (!_items.ContainsKey(key))
                return null;
                
            _items[key] = item;
            return item;
        }
    }
}
