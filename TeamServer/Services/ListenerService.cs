using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{
    public interface IListenerService
    {
        void AddListener(Listener listener);
        IEnumerable<Listener> GetListeners();
        Listener GetListener(string id);
        void RemoveListener(Listener listener);
    }

    public class ListenerService : IListenerService
    {

        private readonly List<Listener> _listeners = new List<Listener>();

        public void AddListener(Listener listener)
        {
            _listeners.Add(listener);
        }

        public Listener GetListener(string id)
        {
            return this.GetListeners().FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Listener> GetListeners()
        {
            return this._listeners;
        }

        public void RemoveListener(Listener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
