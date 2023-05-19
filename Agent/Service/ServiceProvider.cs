using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Service
{
    public static class ServiceProvider
    {
        private static Dictionary<Type, object> instances = new Dictionary<Type, object>();
        public static void RegisterSingleton<T>(T service)
        {
            if (instances.ContainsKey(typeof(T)))
                throw new ApplicationException($"Service Provider : {typeof(T).ToString()} is already registerd!");

            instances.Add(typeof(T), service);
        }


        public static T GetService<T>()
        {
            return (T)instances[typeof(T)];
        }

        public static List<RunningService> GetRunningServices()
        {
            var list = new List<RunningService>();
            foreach(var obj in instances.Values)
            {
                if(obj is RunningService)
                {
                    list.Add(obj as RunningService);
                }
            }

            return list;
        }
    }
}
