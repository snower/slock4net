using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using slock4net.Exceptions;
using System.Threading.Tasks;
using slock4net.datas;
using slock4net.Commands;
using System;
using System.Collections.Generic;
using System.Threading;

namespace slock4net.tests
{
    [TestClass]
    public class ClientTest
    {
        private readonly static Random random = new Random();
        private static readonly string clientHost = "127.0.0.1";
        private static readonly int clinetPort = 5658;

        [TestMethod]
        public void TestPing()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();
            try
            {
                client.Ping();
            }
            finally
            {
                client.Close();
            }

            SlockReplsetClient repletClient = new SlockReplsetClient(new string[] { clientHost + ":" + clinetPort });
            repletClient.Open();
            try
            {
                repletClient.Ping();
            }
            finally
            {
                repletClient.Close();
            }
        }

        [TestMethod]
        public async Task TestPingAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();
            try
            {
                await client.PingAsync();
            }
            finally
            {
                await client.CloseAsync();
            }

            SlockReplsetClient repletClient = new SlockReplsetClient(new string[] { clientHost + ":" + clinetPort });
            await repletClient.OpenAsync();
            try
            {
                await repletClient.PingAsync();
            }
            finally
            {
                await repletClient.CloseAsync();
            }
        }

        [TestMethod]
        public void TestClientLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();
            try
            {
                Lock lck = client.NewLock(Encoding.ASCII.GetBytes("test1"), 5, 5);
                lck.Acquire();
                lck.Release();

                Lock lock1 = client.NewLock(Encoding.ASCII.GetBytes("test1"), 5, 5);
                lock1.Count = 10;
                Lock lock2 = client.NewLock(Encoding.ASCII.GetBytes("test1"), 5, 5);
                lock2.Count = 10;

                lock1.Acquire(new LockSetData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                lock2.Acquire(new LockSetData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                lock1.Release(new LockSetData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbb");
                lock2.Release();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestClientLockAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();
            try
            {
                Lock lck = client.NewLock(Encoding.ASCII.GetBytes("test1_async"), 5, 5);
                await lck.AcquireAsync();
                await lck.ReleaseAsync();

                Lock lock1 = client.NewLock(Encoding.ASCII.GetBytes("test1_async"), 5, 5);
                lock1.Count = 10;
                Lock lock2 = client.NewLock(Encoding.ASCII.GetBytes("test1_async"), 5, 5);
                lock2.Count = 10;

                await lock1.AcquireAsync(new LockSetData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                await lock2.AcquireAsync(new LockSetData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                await lock1.ReleaseAsync(new LockSetData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbb");
                await lock2.ReleaseAsync();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestReplsetClientLock()
        {
            SlockReplsetClient client = new SlockReplsetClient(new string[] { clientHost + ":" + clinetPort });
            client.Open();
            try
            {
                Lock lck = client.NewLock(Encoding.ASCII.GetBytes("test2"), 5, 5);
                lck.Acquire();
                lck.Release();

                Lock lock1 = client.NewLock(Encoding.ASCII.GetBytes("test2"), 5, 5);
                lock1.Count = 10;
                Lock lock2 = client.NewLock(Encoding.ASCII.GetBytes("test2"), 5, 5);
                lock2.Count = 10;

                lock1.Acquire(new LockSetData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                lock2.Acquire(new LockSetData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                lock1.Release(new LockSetData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbb");
                lock2.Release();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestReplsetClientLockAsync()
        {
            SlockReplsetClient client = new SlockReplsetClient(new string[] { clientHost + ":" + clinetPort });
            await client.OpenAsync();
            try
            {
                Lock lck = client.NewLock(Encoding.ASCII.GetBytes("test2_async"), 5, 5);
                await lck.AcquireAsync();
                await lck.ReleaseAsync();

                Lock lock1 = client.NewLock(Encoding.ASCII.GetBytes("test2_async"), 5, 5);
                lock1.Count = 10;
                Lock lock2 = client.NewLock(Encoding.ASCII.GetBytes("test2_async"), 5, 5);
                lock2.Count = 10;

                await lock1.AcquireAsync(new LockSetData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                await lock2.AcquireAsync(new LockSetData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                await lock1.ReleaseAsync(new LockSetData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbb");
                await lock2.ReleaseAsync();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestEventDefaultSeted()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                Event evt = client.NewEvent(Encoding.ASCII.GetBytes("event1"), 5, 60, true);
                Assert.IsTrue(evt.IsSet());
                evt.Clear();
                Assert.IsFalse(evt.IsSet());
                evt.Set();
                Assert.IsTrue(evt.IsSet());
                evt.Wait(2);

                evt = client.NewEvent(Encoding.ASCII.GetBytes("event1"), 5, 60, true);
                Assert.IsTrue(evt.IsSet());
                evt.Clear();
                Assert.IsFalse(evt.IsSet());
                evt.Set("aaa");
                Assert.IsTrue(evt.IsSet());
                evt.Wait(2);
                Assert.AreEqual(evt.CurrentLockDataAsString, "aaa");
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestEventDefaultSetedAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                Event evt = client.NewEvent(Encoding.ASCII.GetBytes("event1_async"), 5, 60, true);
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync();
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.WaitAsync(2);

                evt = client.NewEvent(Encoding.ASCII.GetBytes("event1_async"), 5, 60, true);
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync("aaa");
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.WaitAsync(2);
                Assert.AreEqual(evt.CurrentLockDataAsString, "aaa");
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestEventDefaultUnseted()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                Event evt = client.NewEvent(Encoding.ASCII.GetBytes("event2"), 5, 60, false);
                Assert.IsFalse(evt.IsSet());
                evt.Set();
                Assert.IsTrue(evt.IsSet());
                evt.Clear();
                Assert.IsFalse(evt.IsSet());
                evt.Set();
                Assert.IsTrue(evt.IsSet());
                evt.Wait(2);
                evt.Clear();
                Assert.IsFalse(evt.IsSet());

                evt = client.NewEvent(Encoding.ASCII.GetBytes("event2"), 5, 60, false);
                Assert.IsFalse(evt.IsSet());
                evt.Set();
                Assert.IsTrue(evt.IsSet());
                evt.Clear();
                Assert.IsFalse(evt.IsSet());
                evt.Set("aaa");
                Assert.IsTrue(evt.IsSet());
                evt.Wait(2);
                Assert.AreEqual(evt.CurrentLockDataAsString, "aaa");
                evt.Clear();
                Assert.IsFalse(evt.IsSet());
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestEventDefaultUnsetedAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                Event evt = client.NewEvent(Encoding.ASCII.GetBytes("event2_async"), 5, 60, false);
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync();
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync();
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.WaitAsync(2);
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());

                evt = client.NewEvent(Encoding.ASCII.GetBytes("event2_async"), 5, 60, false);
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync();
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());
                await evt.SetAsync("aaa");
                Assert.IsTrue(await evt.IsSetAsync());
                await evt.WaitAsync(2);
                Assert.AreEqual(evt.CurrentLockDataAsString, "aaa");
                await evt.ClearAsync();
                Assert.IsFalse(await evt.IsSetAsync());
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestGroupEvent()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                GroupEvent groupEvent = client.NewGroupEvent(Encoding.UTF8.GetBytes("groupEvent1"), 1, 1, 5, 60);
                GroupEvent groupEventWaiter = client.NewGroupEvent(Encoding.UTF8.GetBytes("groupEvent1"), 2, 0, 5, 60);
                Assert.IsTrue(groupEvent.IsSet());
                groupEvent.Clear();
                Assert.IsFalse(groupEvent.IsSet());

                groupEvent.Wakeup("aaa");
                Assert.AreEqual((ulong)2, groupEvent.VersionId);
                Assert.IsFalse(groupEvent.IsSet());
                groupEventWaiter.Wait(2);
                Assert.AreEqual((ulong)2, groupEventWaiter.VersionId);
                Assert.AreEqual("aaa", groupEventWaiter.CurrentLockDataAsString);

                groupEvent.Wakeup("bbb");
                Assert.AreEqual((ulong)3, groupEvent.VersionId);
                Assert.IsFalse(groupEvent.IsSet());
                groupEventWaiter.Wait(2);
                Assert.AreEqual((ulong)3, groupEventWaiter.VersionId);
                Assert.AreEqual("bbb", groupEventWaiter.CurrentLockDataAsString);

                groupEvent.Set();
                Assert.IsTrue(groupEvent.IsSet());
                groupEvent.Wait(2);
                Assert.AreEqual((ulong)3, groupEvent.VersionId);
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestGroupEventAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                GroupEvent groupEvent = client.NewGroupEvent(Encoding.UTF8.GetBytes("groupEvent1_async"), 1, 1, 5, 60);
                GroupEvent groupEventWaiter = client.NewGroupEvent(Encoding.UTF8.GetBytes("groupEvent1_async"), 2, 0, 5, 60);
                Assert.IsTrue(await groupEvent.IsSetAsync());
                await groupEvent.ClearAsync();
                Assert.IsFalse(await groupEvent.IsSetAsync());

                await groupEvent.WakeupAsync("aaa");
                Assert.AreEqual((ulong)2, groupEvent.VersionId);
                Assert.IsFalse(await groupEvent.IsSetAsync());
                await groupEventWaiter.WaitAsync(2);
                Assert.AreEqual((ulong)2, groupEventWaiter.VersionId);
                Assert.AreEqual("aaa", groupEventWaiter.CurrentLockDataAsString);

                await groupEvent.WakeupAsync("bbb");
                Assert.AreEqual((ulong)3, groupEvent.VersionId);
                Assert.IsFalse(await groupEvent.IsSetAsync());
                await groupEventWaiter.WaitAsync(2);
                Assert.AreEqual((ulong)3, groupEventWaiter.VersionId);
                Assert.AreEqual("bbb", groupEventWaiter.CurrentLockDataAsString);

                await groupEvent.SetAsync();
                Assert.IsTrue(await groupEvent.IsSetAsync());
                await groupEvent.WaitAsync(2);
                Assert.AreEqual((ulong)3, groupEvent.VersionId);
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestReadWriteLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                ReadWriteLock readLock = client.NewReadWriteLock(Encoding.UTF8.GetBytes("readWriteLock1"), 0, 60);
                ReadWriteLock writeLock = client.NewReadWriteLock(Encoding.UTF8.GetBytes("readWriteLock1"), 0, 60);
                readLock.AcquireRead();
                readLock.AcquireRead();
                readLock.ReleaseRead();
                readLock.ReleaseRead();

                readLock.AcquireRead();
                try
                {
                    writeLock.AcquireWrite();
                    writeLock.ReleaseWrite();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                readLock.ReleaseRead();

                writeLock.AcquireWrite();
                try
                {
                    readLock.AcquireRead();
                    readLock.ReleaseRead();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                writeLock.ReleaseWrite();
                readLock.AcquireRead();
                readLock.AcquireRead();
                readLock.ReleaseRead();
                readLock.ReleaseRead();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestReadWriteLockAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                ReadWriteLock readLock = client.NewReadWriteLock(Encoding.UTF8.GetBytes("readWriteLock1_async"), 0, 60);
                ReadWriteLock writeLock = client.NewReadWriteLock(Encoding.UTF8.GetBytes("readWriteLock1_async"), 0, 60);
                await readLock.AcquireReadAsync();
                await readLock.AcquireReadAsync();
                await readLock.ReleaseReadAsync();
                await readLock.ReleaseReadAsync();

                await readLock.AcquireReadAsync();
                try
                {
                    await writeLock.AcquireWriteAsync();
                    await writeLock.ReleaseWriteAsync();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                await readLock.ReleaseReadAsync();

                await writeLock.AcquireWriteAsync();
                try
                {
                    await readLock.AcquireReadAsync();
                    await readLock.ReleaseReadAsync();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                await writeLock.ReleaseWriteAsync();
                await readLock.AcquireReadAsync();
                await readLock.AcquireReadAsync();
                await readLock.ReleaseReadAsync();
                await readLock.ReleaseReadAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestReentrantLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                ReentrantLock reentrantLock = client.NewReentrantLock(Encoding.UTF8.GetBytes("reentrantLock1"), 0, 60);
                for (int i = 0; i < 10; i++)
                {
                    reentrantLock.Acquire();
                }
                for (int i = 0; i < 10; i++)
                {
                    reentrantLock.Release();
                }
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("reentrantLock1"), 0, 60);
                try
                {
                    lck.ReleaseHead();
                    throw new SlockException("lock error");
                }
                catch (LockUnlockedException)
                {
                }
                lck.Acquire();
                lck.Release();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestReentrantLockAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                ReentrantLock reentrantLock = client.NewReentrantLock(Encoding.UTF8.GetBytes("reentrantLock1_async"), 0, 60);
                for (int i = 0; i < 10; i++)
                {
                    await reentrantLock.AcquireAsync();
                }
                for (int i = 0; i < 10; i++)
                {
                    await reentrantLock.ReleaseAsync();
                }
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("reentrantLock1_async"), 0, 60);
                try
                {
                    await lck.ReleaseHeadAsync();
                    throw new SlockException("lock error");
                }
                catch (LockUnlockedException)
                {
                }
                await lck.AcquireAsync();
                await lck.ReleaseAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestSemaphore()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                Semaphore semaphore = client.NewSemaphore(Encoding.UTF8.GetBytes("semaphore1"), 10, 0, 60);
                for (int i = 0; i < 10; i++)
                {
                    semaphore.Acquire();
                }
                try
                {
                    semaphore.Acquire();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                for (int i = 0; i < 10; i++)
                {
                    semaphore.Release();
                }
                semaphore.Acquire();
                semaphore.Release();
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("semaphore1"), 0, 60);
                try
                {
                    lck.ReleaseHead();
                    throw new SlockException("lock error");
                }
                catch (LockUnlockedException)
                {
                }
                lck.Acquire();
                lck.Release();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestSemaphoreAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                Semaphore semaphore = client.NewSemaphore(Encoding.UTF8.GetBytes("semaphore1_async"), 10, 0, 60);
                for (int i = 0; i < 10; i++)
                {
                    await semaphore.AcquireAsync();
                }
                try
                {
                    await semaphore.AcquireAsync();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                for (int i = 0; i < 10; i++)
                {
                    await semaphore.ReleaseAsync();
                }
                await semaphore.AcquireAsync();
                await semaphore.ReleaseAsync();
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("semaphore1_async"), 0, 60);
                try
                {
                    await lck.ReleaseHeadAsync();
                    throw new SlockException("lock error");
                }
                catch (LockUnlockedException)
                {
                }
                await lck.AcquireAsync();
                await lck.ReleaseAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        private void TestChildTreeLock(SlockClient client, TreeLock rootLock, TreeLock childLock, TreeLock.TreeLeafLock treeLeafLock, int depth)
        {
            TreeLock.TreeLeafLock clock1 = childLock.NewLeafLock();
            clock1.Acquire();

            TreeLock.TreeLeafLock clock2 = childLock.NewLeafLock();
            clock2.Acquire();

            Lock testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            treeLeafLock.Release();
            testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            if (depth - 1 > 0)
            {
                treeLeafLock.Acquire();
                TestChildTreeLock(client, childLock, childLock.NewChild(), treeLeafLock, depth - 1);
                treeLeafLock.Acquire();
                TestChildTreeLock(client, childLock, childLock.NewChild(), treeLeafLock, depth - 1);
            }

            clock1.Release();
            testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                testLock.Acquire();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            clock2.Release();
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            testLock.Acquire();
        }

        [TestMethod]
        public void TestTreeLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            TreeLock rootLock = client.NewTreeLock("TestTreeLock", 5, 10);

            rootLock.Acquire();
            rootLock.Release();
            rootLock.Wait(1);

            TreeLock.TreeLeafLock treeLeafLock = rootLock.NewLeafLock();
            treeLeafLock.Acquire();

            TestChildTreeLock(client, rootLock, rootLock.NewChild(), treeLeafLock, 5);

            rootLock.Wait(1);
            Lock testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            testLock.Acquire();

            client.Close();
        }


        private async Task TestChildTreeLockAsync(SlockClient client, TreeLock rootLock, TreeLock childLock, TreeLock.TreeLeafLock treeLeafLock, int depth)
        {
            TreeLock.TreeLeafLock clock1 = childLock.NewLeafLock();
            await clock1.AcquireAsync();

            TreeLock.TreeLeafLock clock2 = childLock.NewLeafLock();
            await clock2.AcquireAsync();

            Lock testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            await treeLeafLock.ReleaseAsync();
            testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            if (depth - 1 > 0)
            {
                await treeLeafLock.AcquireAsync();
                await TestChildTreeLockAsync(client, childLock, childLock.NewChild(), treeLeafLock, depth - 1);
                await treeLeafLock.AcquireAsync();
                await TestChildTreeLockAsync(client, childLock, childLock.NewChild(), treeLeafLock, depth - 1);
            }

            await clock1.ReleaseAsync();
            testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            try
            {
                await testLock.AcquireAsync();
                throw new SlockException();
            }
            catch (LockTimeoutException)
            {
            }

            await clock2.ReleaseAsync();
            testLock = client.NewLock(childLock.GetLockKey(), 0, 0);
            await testLock.AcquireAsync();
        }

        [TestMethod]
        public async Task TestTreeLockAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            TreeLock rootLock = client.NewTreeLock("TestTreeLock2", 5, 10);

            await rootLock.AcquireAsync();
            await rootLock.ReleaseAsync();
            await rootLock.WaitAsync(1);

            TreeLock.TreeLeafLock treeLeafLock = rootLock.NewLeafLock();
            await treeLeafLock.AcquireAsync();

            await TestChildTreeLockAsync(client, rootLock, rootLock.NewChild(), treeLeafLock, 5);

            await rootLock.WaitAsync(1);
            Lock testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            await testLock.AcquireAsync();

            await client.CloseAsync();
        }

        [TestMethod]
        public void TestMaxConcurrentFlow()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                MaxConcurrentFlow maxConcurrentFlow1 = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1"), 5, 0, 10);
                MaxConcurrentFlow maxConcurrentFlow2 = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1"), 5, 0, 10);
                maxConcurrentFlow1.Acquire();
                maxConcurrentFlow2.Acquire();
                maxConcurrentFlow1.Release();
                maxConcurrentFlow2.Release();

                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("maxconcurrentflow1"), 0, 10);
                lck.Acquire();
                lck.Release();

                MaxConcurrentFlow[] maxConcurrentFlows = new MaxConcurrentFlow[5];
                for (int i = 0; i < 5; i++)
                {
                    maxConcurrentFlows[i] = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1"), 5, 0, 10);
                    maxConcurrentFlows[i].Acquire();
                }
                try
                {
                    maxConcurrentFlow1.Acquire();
                    throw new SlockException("acquire error");
                }
                catch (LockTimeoutException)
                {
                }
                for (int i = 0; i < 5; i++)
                {
                    maxConcurrentFlows[i].Release();
                }
                maxConcurrentFlow1.Acquire();
                maxConcurrentFlow1.Release();

                for (int i = 0; i < 5; i++)
                {
                    maxConcurrentFlows[i] = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1"), 5, 0, 100);
                    maxConcurrentFlows[i].ExpriedFlag = (ushort)ICommand.EXPRIED_FLAG_MILLISECOND_TIME;
                    maxConcurrentFlows[i].Acquire();
                }
                System.Threading.Thread.Sleep(200);
                maxConcurrentFlow1.Acquire();
                maxConcurrentFlow1.Release();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestMaxConcurrentFlowAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                MaxConcurrentFlow maxConcurrentFlow1 = client.NewMaxConcurrentFlow("maxconcurrentflow1_async", (ushort)5, 0, 10);
                MaxConcurrentFlow maxConcurrentFlow2 = client.NewMaxConcurrentFlow("maxconcurrentflow1_async", (ushort)5, 0, 10);
                await maxConcurrentFlow1.AcquireAsync();
                await maxConcurrentFlow2.AcquireAsync();
                await maxConcurrentFlow1.ReleaseAsync();
                await maxConcurrentFlow2.ReleaseAsync();

                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("maxconcurrentflow1_async"), 0, 10);
                await lck.AcquireAsync();
                await lck.ReleaseAsync();

                MaxConcurrentFlow[] maxConcurrentFlows = new MaxConcurrentFlow[5];
                for (int i = 0; i < 5; i++)
                {
                    maxConcurrentFlows[i] = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1_async"), 5, 0, 10);
                    await maxConcurrentFlows[i].AcquireAsync();
                }
                try
                {
                    await maxConcurrentFlow1.AcquireAsync();
                    throw new SlockException("acquire error");
                }
                catch (LockTimeoutException)
                {
                }
                for (int i = 0; i < 5; i++)
                {
                    await maxConcurrentFlows[i].ReleaseAsync();
                }
                await maxConcurrentFlow1.AcquireAsync();
                await maxConcurrentFlow1.ReleaseAsync();

                for (int i = 0; i < 5; i++)
                {
                    maxConcurrentFlows[i] = client.NewMaxConcurrentFlow(Encoding.UTF8.GetBytes("maxconcurrentflow1_async"), 5, 0, 100);
                    maxConcurrentFlows[i].ExpriedFlag = (ushort)ICommand.EXPRIED_FLAG_MILLISECOND_TIME;
                    await maxConcurrentFlows[i].AcquireAsync();
                }
                await Task.Delay(200);
                await maxConcurrentFlow1.AcquireAsync();
                await maxConcurrentFlow1.ReleaseAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestTokenBucketFlow()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                TokenBucketFlow tokenBucketFlow1 = client.NewTokenBucketFlow(Encoding.UTF8.GetBytes("tokenbucketflow1"), 5, 0, 0.1);
                TokenBucketFlow tokenBucketFlow2 = client.NewTokenBucketFlow(Encoding.UTF8.GetBytes("tokenbucketflow1"), 5, 0, 0.1);
                tokenBucketFlow1.Acquire();
                tokenBucketFlow2.Acquire();

                System.Threading.Thread.Sleep(200);
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("tokenbucketflow1"), 0, 10);
                lck.Acquire();
                lck.Release();

                TokenBucketFlow tokenBucketFlow = client.NewTokenBucketFlow(Encoding.UTF8.GetBytes("tokenbucketflow1"), 50, 5, 0.05);
                int count = 0;
                long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                for (int i = 0; i < 2000; i++)
                {
                    tokenBucketFlow.Acquire();
                    count++;
                }
                long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                System.Threading.Thread.Sleep(100);
                lck = client.NewLock(Encoding.UTF8.GetBytes("tokenbucketflow1"), 0, 10);
                lck.Acquire();
                lck.Release();

                int rate = (int)(count * 1000 / (endTime - startTime));
                Console.WriteLine("TokenBucketFlow 1000r/s Count: " + rate);
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestTokenBucketFlowAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                TokenBucketFlow tokenBucketFlow1 = client.NewTokenBucketFlow("tokenbucketflow1_async", (ushort)5, 60, 0.1);
                TokenBucketFlow tokenBucketFlow2 = client.NewTokenBucketFlow("tokenbucketflow1_async", (ushort)5, 60, 0.1);
                await tokenBucketFlow1.AcquireAsync();
                await tokenBucketFlow2.AcquireAsync();

                await Task.Delay(200);
                Lock lck = client.NewLock(Encoding.UTF8.GetBytes("tokenbucketflow1_async"), 0, 10);
                await lck.AcquireAsync();
                await lck.ReleaseAsync();

                TokenBucketFlow tokenBucketFlow = client.NewTokenBucketFlow(Encoding.UTF8.GetBytes("tokenbucketflow1_async"), 50, 5, 0.05);
                Exception exception = null;
                int count = 0;
                List<Task> tasks = new List<Task>();
                long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                for (int i = 0; i < 256; i++)
                {
                    Task task = Task.Run(async () =>
                    {
                        while (Interlocked.CompareExchange(ref count, 0, 0) < 2000)
                        {
                            try
                            {
                                await tokenBucketFlow.AcquireAsync();
                                Interlocked.Increment(ref count);
                            }
                            catch (Exception e)
                            {
                                Interlocked.Exchange(ref exception, e);
                            }
                        }
                    });
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                long endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                await Task.Delay(100);
                lck = client.NewLock(Encoding.UTF8.GetBytes("tokenbucketflow1_async"), 0, 10);
                await lck.AcquireAsync();
                await lck.ReleaseAsync();

                int rate = (int)(count * 1000 / (endTime - startTime));
                Console.WriteLine("Async TokenBucketFlow 1000r/s Count: " + rate);
                if (exception != null)
                {
                    throw exception;
                }
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public async Task TestPriorityLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                Exception exception = null;
                List<Task> tasks = new List<Task>();
                List<PriorityLock> priorityLocks = new List<PriorityLock>();
                for (int i = 0; i < 1000; i++)
                {
                    Task task = Task.Run(async () =>
                    {
                        PriorityLock priorityLock = client.NewPriorityLock(Encoding.UTF8.GetBytes("testPriorityLock"), (byte)(random.Next(50) + 1), 5, 10);
                        try
                        {
                            await priorityLock.AcquireAsync();
                            priorityLocks.Add(priorityLock);
                            await priorityLock.ReleaseAsync();
                        }
                        catch (Exception e)
                        {
                            Interlocked.Exchange(ref exception, e);
                        }
                    });
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                if (exception != null)
                {
                    throw exception;
                }
                byte currentPriority = 0;
                foreach (PriorityLock priorityLock in priorityLocks)
                {
                    if (priorityLock.Priority < currentPriority)
                    {
                        throw new SlockException("Priority Error");
                    }
                }
            }
            finally
            {
                await client.CloseAsync();
            }
        }


        [TestMethod]
        public void TestLockData()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                Lock lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata1"), 0, 10);
                lock1.Count = 10;
                Lock lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata1"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockSetData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                lock2.Acquire(new LockSetData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                lock1.Release(new LockSetData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbb");
                lock2.Release();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");

                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata2"), 0, 10);
                lock1.Count = 10;
                lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata2"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockIncrData(1));
                Assert.AreEqual((long)lock1.CurrentLockDataAsLong, 0);
                lock2.Acquire(new LockIncrData(-3));
                Assert.AreEqual((long)lock2.CurrentLockDataAsLong, 1);
                lock1.Release(new LockIncrData(4));
                Assert.AreEqual((long)lock1.CurrentLockDataAsLong, -2);
                lock2.Release();
                Assert.AreEqual((long)lock2.CurrentLockDataAsLong, 2);

                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata3"), 0, 10);
                lock1.Count = 10;
                lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata3"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockAppendData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                lock2.Acquire(new LockAppendData("bbb"));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaa");
                lock1.Release(new LockAppendData("ccc"));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "aaabbb");
                lock2.Release();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaabbbccc");

                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata4"), 0, 10);
                lock1.Count = 10;
                lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata4"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockSetData("aaabbbccc"));
                Assert.IsNull(lock1.CurrentLockDataAsString);
                lock2.Acquire(new LockShiftData(4));
                Assert.AreEqual(lock2.CurrentLockDataAsString, "aaabbbccc");
                lock1.Release(new LockShiftData(2));
                Assert.AreEqual(lock1.CurrentLockDataAsString, "bbccc");
                lock2.Release();
                Assert.AreEqual(lock2.CurrentLockDataAsString, "ccc");

                byte[] lockKey = LockCommand.GenLockId();
                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata5"), 0, 10);
                lock1.Acquire(new LockExecuteData(new LockCommand(ICommand.COMMAND_TYPE_LOCK, (byte)0, 0, lockKey, LockCommand.GenLockId(), 0, 10, 0, 0, new LockSetData("aaa"))));
                System.Threading.Thread.Sleep(100);
                lock2 = client.NewLock(lockKey, 0, 10);
                try
                {
                    lock2.Acquire();
                    lock2.Release();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                lock1.Release();

                lockKey = LockCommand.GenLockId();
                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata6"), 0, 10);
                lock1.Acquire(new LockPipelineData(new LockData[] {
                        new LockSetData("aaa"),
                        new LockExecuteData(new LockCommand(ICommand.COMMAND_TYPE_LOCK, (byte)0, 0, lockKey, LockCommand.GenLockId(), 0, 10, 0, 0, new LockSetData("aaa"))),
                }));
                System.Threading.Thread.Sleep(100);
                lock2 = client.NewLock(lockKey, 0, 10);
                try
                {
                    lock2.Acquire();
                    lock2.Release();
                    throw new SlockException("lock error");
                }
                catch (LockTimeoutException)
                {
                }
                lock1.Release();
                Assert.AreEqual(lock1.CurrentLockDataAsString, "aaa");

                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata7"), 0, 10);
                lock1.Count = 10;
                lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata7"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockPushData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsList);
                lock2.Acquire(new LockPushData("bbb"));
                IList<string> values = lock2.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(1, values.Count);
                Assert.AreEqual("aaa", values[0]);
                lock1.Release(new LockPushData("ccc"));
                values = lock1.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(2, values.Count);
                Assert.AreEqual("aaa", values[0]);
                Assert.AreEqual("bbb", values[1]);
                lock2.Release();
                values = lock2.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("aaa", values[0]);
                Assert.AreEqual("bbb", values[1]);
                Assert.AreEqual("ccc", values[2]);

                lock1 = client.NewLock(Encoding.UTF8.GetBytes("lockdata8"), 0, 10);
                lock1.Count = 10;
                lock2 = client.NewLock(Encoding.UTF8.GetBytes("lockdata8"), 0, 10);
                lock2.Count = 10;
                lock1.Acquire(new LockPushData("aaa"));
                Assert.IsNull(lock1.CurrentLockDataAsList);
                lock1.Update(new LockPushData("bbb"));
                Assert.IsNotNull(lock1.CurrentLockDataAsList);
                lock1.Update(new LockPushData("ccc"));
                Assert.IsNotNull(lock1.CurrentLockDataAsList);
                lock2.Acquire(new LockPopData(1));
                values = lock2.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("aaa", values[0]);
                Assert.AreEqual("bbb", values[1]);
                Assert.AreEqual("ccc", values[2]);
                lock1.Release(new LockPopData(4));
                values = lock1.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(2, values.Count);
                Assert.AreEqual("bbb", values[0]);
                Assert.AreEqual("ccc", values[1]);
                lock2.Release();
                values = lock2.CurrentLockDataAsStringList;
                Assert.IsNotNull(values);
                Assert.AreEqual(0, values.Count);
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task BenchmarkTest()
        {
            int totalCount = 400000;

            SlockClient client1 = new SlockClient(clientHost, clinetPort);
            client1.Open();
            try
            {
                Exception exception = null;
                int count = 0;
                List<Thread> threads = new List<Thread>();
                long startMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                for (int i = 0; i < 256; i++)
                {
                    Thread thread = new Thread(() =>
                    {
                        while (Interlocked.CompareExchange(ref count, 0, 0) < totalCount)
                        {
                            Lock lck = client1.NewLock(Encoding.UTF8.GetBytes("benchmark" + Interlocked.Increment(ref count)), 5, 10);
                            try
                            {
                                lck.Acquire(count % 2 == 0 ? new LockSetData(new byte[random.Next(1, 256) + 1]) : null);
                                Interlocked.Increment(ref count);
                                lck.Release();
                                Interlocked.Increment(ref count);
                            }
                            catch (Exception e)
                            {
                                Interlocked.Exchange(ref exception, e);
                            }
                        }
                    });
                    thread.IsBackground = true;
                    thread.Start();
                    threads.Add(thread);
                }
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }
                long endMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Console.WriteLine("Benchmark " + totalCount + " Count Lock and Unlock: " + count + " " + (((double)totalCount) / ((endMs - startMs) / 1000d)) + "r/s " + (endMs - startMs) + "ms");
                if (exception != null)
                {
                    throw exception;
                }
            }
            finally
            {
                client1.Close();
            }

            SlockClient client2 = new SlockClient(clientHost, clinetPort);
            client2.Open();
            try
            {
                Exception exception = null;
                int count = 0;
                List<Task> tasks = new List<Task>();
                long startMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                for (int i = 0; i < 256; i++)
                {
                    Task task = Task.Run(async () =>
                    {
                        while (Interlocked.CompareExchange(ref count, 0, 0) < totalCount)
                        {
                            Lock lck = client2.NewLock(Encoding.UTF8.GetBytes("benchmark" + Interlocked.Increment(ref count)), 5, 10);
                            try
                            {
                                await lck.AcquireAsync(count % 2 == 0 ? new LockSetData(new byte[random.Next(1, 256) + 1]) : null);
                                Interlocked.Increment(ref count);
                                await lck.ReleaseAsync();
                                Interlocked.Increment(ref count);
                            }
                            catch (Exception e)
                            {
                                Interlocked.Exchange(ref exception, e);
                            }
                        }
                    });
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                long endMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                Console.WriteLine("Async Benchmark " + totalCount + " Count Lock and Unlock: " + count + " " + (((double)totalCount) / ((endMs - startMs) / 1000d)) + "r/s " + (endMs - startMs) + "ms");
                if (exception != null)
                {
                    throw exception;
                }
            }
            finally
            {
                client2.Close();
            }
        }
    }
}
