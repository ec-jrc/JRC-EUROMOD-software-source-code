using System.Collections.Generic;

namespace EM_Common
{
    public class TSDictionary // a dictionary allowing for values of different type and their type-safe retrieval
    {
        private Dictionary<string, object> _content = new Dictionary<string, object>();

        public T GetItem<T>(string key) // does not cause an exception itself, but using the result may (if not initialised or wrong cast), see (*)
        {
            if (_content.ContainsKey(key))
                return (new TSObject(_content[key])).GetValue<T>();
            return default(T); // (*) for most types (including string) this is null, however e.g. for bool it is false
        }

        public void SetItem(string key, object value)
        {
            try { if (_content.ContainsKey(key)) _content[key] = value; else _content.Add(key, value); }
            catch { }
        }

        public bool ContainsKey(string key) { return _content.ContainsKey(key); }

        public void Clear() { _content.Clear(); }
        public Dictionary<string, object> GetCopyOfContent() { return new Dictionary<string, object>(_content); }
    }
}
