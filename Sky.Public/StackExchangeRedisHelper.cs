using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Sky.Public
{
    public class StackExchangeRedisHelper
    {
        private static ConnectionMultiplexer _instance = null;
        private static object _locker = new Object();

        private static string _connstr = string.Empty;
        private static int _databaseIndex = 0;
        public StackExchangeRedisHelper(int databaseIndex,string connstr)
        {
            _connstr = connstr;
            _databaseIndex = databaseIndex; 
        }

        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance==null)
                {
                    lock (_locker)
                    {
                        if (_instance==null||!Instance.IsConnected)
                        {
                            _instance=ConnectionMultiplexer.Connect(_connstr);
                        }
                    }
                }
                return _instance;
            }
        }



        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase(_databaseIndex); 
        }

     

        /// <summary>
        /// 根据key获取缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return SerializeHelper.Deserialize<T>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// 根据key获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(string key)
        {
            return SerializeHelper.Deserialize<object>(GetDatabase().StringGet(key));
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        public static void Set(string key,object value,TimeSpan? expired = null)
        {
            GetDatabase().StringSet(key, SerializeHelper.Serialize(value), expired);
        }

        public static void HashSet(string key, IDictionary<string, string> hash, TimeSpan? expired = null)
        {
            var entrys = hash.Select(m => new HashEntry(m.Key, m.Value)).ToArray();
            Instance.GetDatabase().HashSet(key, entrys);
            if (expired != null)
            {
                Instance.GetDatabase().KeyExpire(key, expired.Value); 
            }
        }

        public IDictionary<string, string> HashGet(string key)
        {
            HashEntry[] hashentries = Instance.GetDatabase().HashGetAll(key);
            return hashentries?.ToStringDictionary();
        }

        /// <summary>
        /// 判断在缓存中是否存在该key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return GetDatabase().KeyExists(key);  //可直接调用
        }

        /// <summary>
        /// 移除指定key的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            return GetDatabase().KeyDelete(key);
        }


        #region  当作消息代理中间件使用 一般使用更专业的消息队列来处理这种业务场景
        /// <summary>
        /// 当作消息代理中间件使用
        /// 消息组建中,重要的概念便是生产者,消费者,消息中间件。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static long Publish(string channel, string message)
        {
            ISubscriber sub = Instance.GetSubscriber();
            //return sub.Publish("messages", "hello");
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 在消费者端得到该消息并输出
        /// </summary>
        /// <param name="channelFrom"></param>
        /// <returns></returns>
        public static void Subscribe(string channelFrom)
        {
            ISubscriber sub = Instance.GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) =>
            {
                Console.WriteLine((string)message);
            });
        }
        #endregion

    }
}
