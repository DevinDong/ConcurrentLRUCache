using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentLRUCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class ConcurrentLRUCacheTest
    {
        [TestMethod]
        public void TestPut()
        {
            var lruCache = new ConcurrentLRUCache<string, int>(100);

            #region Test for Sync

            lruCache.Put("1", 1);
            try
            {
                lruCache.Put("1", 1);
            }
            catch (ArgumentException)
            {

            }
            catch (Exception)
            {
                Assert.Fail("uncatched exception.");
            }

            lruCache.Put("2", 2);
            lruCache.Put("3", 3);

            int var1 = lruCache.Get("1");
            int var2 = lruCache.Get("2");
            int var3 = lruCache.Get("3");

            int var4 = lruCache.Get("4");


            Assert.AreEqual(1, var1);
            Assert.AreEqual(2, var2);
            Assert.AreEqual(3, var3);
            Assert.AreEqual(default(Int32), var4);

            var array = new string[lruCache.Keys.Count];

            lruCache.Keys.CopyTo(array, 0);
            Assert.AreEqual("3", array[0]);
            Assert.AreEqual("2", array[1]);
            Assert.AreEqual("1", array[2]);


            #endregion

            #region Test for Async

            Parallel.For(4, 81, i => lruCache.Put(i.ToString(CultureInfo.InvariantCulture), i));

            lruCache.Put("81", 81);
            lruCache.Put("82", 82);
            lruCache.Put("83", 83);

            Assert.AreEqual((UInt32)83, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);
            #endregion

            #region Test for LRU Strategy

            //wait for LRU.
            Thread.Sleep((Int32)(lruCache.LRUPeroid + 1000));

            Assert.AreEqual((UInt32)80, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);
            #endregion

        }


        [TestMethod]
        public void TestDelete()
        {
            var lruCache = new ConcurrentLRUCache<string, int>(100);

            lruCache.Put("1", 1);
            lruCache.Put("2", 2);
            lruCache.Put("3", 3);
            lruCache.Put("4", 4);

            int var1 = lruCache.Get("1");
            Assert.AreEqual(1, lruCache.Get("1"));

            lruCache.Delete("1");

            Assert.AreEqual((UInt32)3, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);

            Assert.AreEqual(0, lruCache.Get("1"));

            Parallel.Invoke(
                () =>
                {
                    try
                    {
                        lruCache.Delete("2");
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                    catch (Exception)
                    {
                        Assert.Fail("uncaughted exception.");
                    }

                }
                , () =>
                {
                    try
                    {
                        lruCache.Delete("2");
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                    catch (Exception)
                    {
                        Assert.Fail("uncaughted exception.");
                    }

                }
                , () =>
                {
                    try
                    {
                        lruCache.Delete("2");
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                    catch (Exception)
                    {
                        Assert.Fail("uncaughted exception.");
                    }
                }
                , () =>
                {
                    try
                    {
                        lruCache.Delete("2");
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                    catch (Exception)
                    {
                        Assert.Fail("uncaughted exception.");
                    }
                }
                );

            Assert.AreEqual((UInt32)2, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);

            Assert.AreEqual(0, lruCache.Get("2"));
            Assert.AreEqual(3, lruCache.Get("3"));
            Assert.AreEqual(4, lruCache.Get("4"));

        }

        [TestMethod]
        public void TestUpdate()
        {
            var lruCache = new ConcurrentLRUCache<string, int>(100);

            lruCache.Put("1", 1);
            lruCache.Put("2", 2);
            lruCache.Put("3", 3);
            lruCache.Put("4", 4);

            int var1 = lruCache.Get("1");
            Assert.AreEqual(1, lruCache.Get("1"));

            lruCache.Update("1", 11);
            Assert.AreEqual(11, lruCache.Get("1"));

            Parallel.Invoke(
                () => lruCache.Update("1", 111)
                , () => lruCache.Update("1", 111)
                , () => lruCache.Update("1", 111)
                , () => lruCache.Update("1", 111)
                );

            Assert.AreEqual((UInt32)4, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);

            Assert.AreEqual(111, lruCache.Get("1"));
            Assert.AreEqual(2, lruCache.Get("2"));
            Assert.AreEqual(3, lruCache.Get("3"));
            Assert.AreEqual(4, lruCache.Get("4"));


            Parallel.Invoke(
              () => lruCache.Update("1", 1)
              , () => lruCache.Update("1", 11)
              , () => lruCache.Update("1", 111)
              , () => lruCache.Update("1", 1111)
              );

            Assert.AreEqual((UInt32)4, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);
            var a1 = lruCache.Get("1");

            Assert.IsTrue(new Int32[] { 1, 11, 111, 1111 }.Contains(a1));
            Assert.AreEqual(2, lruCache.Get("2"));
            Assert.AreEqual(3, lruCache.Get("3"));
            Assert.AreEqual(4, lruCache.Get("4"));

            Parallel.Invoke(
               () => lruCache.Update("1", 11)
               , () => lruCache.Update("2", 22)
               , () => lruCache.Update("3", 33)
               , () => lruCache.Update("4", 44)
               );

            Assert.AreEqual((UInt32)4, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);

            Assert.AreEqual(11, lruCache.Get("1"));
            Assert.AreEqual(22, lruCache.Get("2"));
            Assert.AreEqual(33, lruCache.Get("3"));
            Assert.AreEqual(44, lruCache.Get("4"));
        }


        [TestMethod]
        public void TestClear()
        {
            var lruCache = new ConcurrentLRUCache<string, int>(100);
            lruCache.Put("1", 1);
            lruCache.Put("2", 2);
            lruCache.Put("3", 3);

            Assert.AreEqual((UInt32)3, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);

            lruCache.Clear();

            Parallel.Invoke(
                lruCache.Clear
                , lruCache.Clear
                , lruCache.Clear
                , lruCache.Clear
                );


            Assert.AreEqual((UInt32)0, lruCache.Size);
            Assert.AreEqual((Int32)lruCache.Size, lruCache.Keys.Count);
            Assert.AreEqual(0, lruCache.Keys.Count);
        }




    }
}
