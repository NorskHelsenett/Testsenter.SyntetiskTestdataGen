using System.Collections.Concurrent;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class IdentifierDublicateControl
    {
        private readonly ConcurrentDictionary<string, object> _taken;
        public ConcurrentDictionary<string, object> GetTaken() => _taken;
        public bool TakenContains(string s)
        {
            return _taken.ContainsKey(s);
        }

        public bool TakenAdd(string s)
        {
            return _taken.TryAdd(s, null);
        }

        public IdentifierDublicateControl()
        {
            _taken = new ConcurrentDictionary<string, object>();
        }
    }
}
