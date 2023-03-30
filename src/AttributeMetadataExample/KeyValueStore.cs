using System.Collections.Generic;


namespace Quantifi;


public interface IKeyValueStore {

    T Get<T>(string key) where T : class;

    bool ContainsKey(string key);

    void Add<T>(string key, T value) where T : class;

}



public class KeyValueStore : IKeyValueStore
{
    private IDictionary<string, object> _dict = new Dictionary<string, object> ();

    public void Add<T>(string key, T value) where T : class => _dict[key] = value;
    public bool ContainsKey(string key) => _dict.ContainsKey(key);
    public T Get<T>(string key) where T : class => _dict[key] as T;
}
