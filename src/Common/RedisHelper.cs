using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Text;

namespace Common

{

    /// <summary>

    /// Redis 帮助类

    /// </summary>

    public static class RedisHelper

    {

        private static string _conn = ConfigurationManager.AppSettings["redis_connection_string"] ?? "127.0.0.1:6379";

        private static string _pwd = ConfigurationManager.AppSettings["redis_connection_pwd"] ?? "123456";



        static ConnectionMultiplexer _redis;

        static readonly object _locker = new object();



        #region 单例模式

        public static ConnectionMultiplexer Manager

        {

            get

            {

                if (_redis == null)

                {

                    lock (_locker)

                    {

                        if (_redis != null) return _redis;



                        _redis = GetManager();

                        return _redis;

                    }

                }

                return _redis;

            }

        }



        private static ConnectionMultiplexer GetManager(string connectionString = null)

        {

            if (string.IsNullOrEmpty(connectionString))

            {

                connectionString = _conn;

            }

            var options = ConfigurationOptions.Parse(connectionString);

            options.Password = _pwd;

            return ConnectionMultiplexer.Connect(options);

        }

        #endregion



        /// <summary>

        /// 添加

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="value">值</param>

        /// <param name="expireMinutes">过期时间，单位：分钟。默认600分钟</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        /// <returns></returns>

        public static bool StringSet(string key, string value, int expireMinutes = 600, int db = -1)

        {
            return Manager.GetDatabase(db).StringSet(key, value, TimeSpan.FromMinutes(expireMinutes));
        }



        /// <summary>

        /// 获取

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        /// <returns></returns>

        public static string StringGet(string key, int db = -1)

        {

            try

            {
                return Manager.GetDatabase(db).StringGet(key);

            }

            catch (Exception)

            {

                return null;

            }

        }



        /// <summary>

        /// 删除

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        /// <returns></returns>

        public static bool StringRemove(string key, int db = -1)

        {

            try

            {


                return Manager.GetDatabase(db).KeyDelete(key);

            }

            catch (Exception)

            {

                return false;

            }

        }

        /// <summary>

        /// 是否存在

        /// </summary>

        /// <param name="key">键</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        public static bool KeyExists(string key, int db = -1)

        {

            try

            {


                return Manager.GetDatabase(db).KeyExists(key);

            }

            catch (Exception)

            {

                return false;

            }

        }



        /// <summary>

        /// 延期

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="min">延长时间，单位：分钟，默认600分钟</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        public static bool AddExpire(string key, int min = 600, int db = -1)

        {

            try

            {



                return Manager.GetDatabase(db).KeyExpire(key, DateTime.Now.AddMinutes(min));

            }

            catch (Exception)

            {

                return false;

            }

        }



        /// <summary>

        /// 添加实体

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="t">实体</param>

        /// <param name="ts">延长时间，单位：分钟，默认600分钟</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        public static bool Set<T>(string key, T t, int expireMinutes = 600, int db = -1)

        {



            var str = JsonConvert.SerializeObject(t);

            return Manager.GetDatabase(db).StringSet(key, str, TimeSpan.FromMinutes(expireMinutes));

        }



        /// <summary>

        /// 获取实体

        /// </summary>

        /// <param name="folder">目录</param>

        /// <param name="key">键</param>

        /// <param name="db">库，默认第一个。0~15共16个库</param>

        public static T Get<T>(string key, int db = -1) where T : class

        {



            var strValue = Manager.GetDatabase(db).StringGet(key);

            return string.IsNullOrEmpty(strValue) ? null : JsonConvert.DeserializeObject<T>(strValue);

        }



        /// <summary>

        /// 获得枚举的Description

        /// </summary>

        /// <param name="value">枚举值</param>

        /// <param name="nameInstead">当枚举值没有定义DescriptionAttribute，是否使用枚举名代替，默认是使用</param>

        /// <returns>枚举的Description</returns>

        private static string GetDescription(this Enum value, Boolean nameInstead = true)

        {

            Type type = value.GetType();

            string name = Enum.GetName(type, value);

            if (name == null)

            {

                return null;

            }



            FieldInfo field = type.GetField(name);

            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;



            if (attribute == null && nameInstead == true)

            {

                return name;

            }

            return attribute == null ? null : attribute.Description;

        }

    }

}