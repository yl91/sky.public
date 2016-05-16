using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Sky.Public;

namespace Sky.Test
{
    class StackExchageRedisTest
    {
        [Test]
        public void Get()
        {
            var host = "127.0.0.1:6379";
            StackExchangeRedisHelper redis=new StackExchangeRedisHelper(0,host);

            var value = "abc123";
            StackExchangeRedisHelper.Set("name",value);

            var str= StackExchangeRedisHelper.Get("name");
            Assert.AreEqual(str,value);
        }

        [Test]
        public void HashGet()
        {
            var host = "127.0.0.1:6379";
            StackExchangeRedisHelper redis = new StackExchangeRedisHelper(0, host);

            var dic=new Dictionary<string,string>(){};
            dic.Add("name","zhangsan");
            dic.Add("age", "20");
            StackExchangeRedisHelper.HashSet("hashName",dic);

            var result= StackExchangeRedisHelper.HashGet("hashName");
            Assert.AreEqual(dic,result);
        }

        [Test]
        public void PublishTest()
        {
            var host = "127.0.0.1:6379";
            StackExchangeRedisHelper redis = new StackExchangeRedisHelper(0, host);


            StackExchangeRedisHelper.Subscribe("test", (arg1,arg2) =>
            {
                Assert.AreEqual(new Person() { Age = 20, Name = "小明" }, JsonConvert.DeserializeObject(arg2));
            });

            StackExchangeRedisHelper.Publish<Person>("test", new Person() {Age = 20,Name = "小明"});

        }

    }

    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

    }
}
