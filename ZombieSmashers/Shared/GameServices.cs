using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Shared
{
    public static class GameServices
    {
        private static GameServiceContainer Container;
        private static GameServiceContainer Instance
        {
            get
            {
                if (Container == null)
                {
                    Container = new GameServiceContainer();
                }

                return Container;
            }
        }

        public static T GetService<T>()
        {
            return (T)Instance.GetService(typeof(T));
        }

        public static void AddService<T>(T service)
        {
            Instance.AddService(typeof(T), service);
        }

        public static void RemoveService<T>()
        {
            Instance.RemoveService(typeof(T));
        }
    }
}
