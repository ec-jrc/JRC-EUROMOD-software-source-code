using System;

namespace EM_Common
{
    public class TSObject // a type save object
    {
        private object _value;

        public TSObject() { _value = null; }
        public TSObject(object value) { _value = value; }

        public T GetValue<T>() // does not cause an exception itself, but using the result may (if not initialised or wrong cast), see (*)
        {
            if (_value == null)
            {
                return default(T);
            }
            try
            {
                if (_value.GetType() != typeof(T) && !(_value is T)) // this is very likely to happen when parameters are passed via Request (they are all strings)
                    try { _value = Convert.ChangeType(_value, typeof(T)); }
                    catch { } // if conversion fails still can hope that a normal cast will succeed ...
                return (T)_value;
            }
            catch
            {
                return default(T); // (*) for most types (including string) this is null, however e.g. for bool it is false
            }
        }

        public void SetValue(object value) { _value = value; }
    }
}
