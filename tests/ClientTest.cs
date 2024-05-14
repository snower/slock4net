using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using slock4net.Exceptions;
using System.Threading.Tasks;

namespace slock4net.tests
{
    [TestClass]
    public class ClientTest
    {
        static readonly string clientHost = "127.0.0.1";
        static readonly int clinetPort = 5658;

        [TestMethod]
        public void TestClientLock()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();
            try
            {
                Lock l = client.NewLock(Encoding.ASCII.GetBytes("test1"), 5, 5);
                l.Acquire();
                l.Release();
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
                Lock l = client.NewLock(Encoding.ASCII.GetBytes("test1_async"), 5, 5);
                await l.AcquireAsync();
                await l.ReleaseAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void TestReplsetClientLock()
        {
            SlockReplsetClient client = new SlockReplsetClient(new string[]{clientHost + ":" + clinetPort});
            client.Open();
            try
            {
                Lock l = client.NewLock(Encoding.ASCII.GetBytes("test2"), 5, 5);
                l.Acquire();
                l.Release();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task TestReplsetClientLockAsync()
        {
            SlockReplsetClient client = new SlockReplsetClient(new string[]{clientHost + ":" + clinetPort});
            await client.OpenAsync();
            try
            {
                Lock l = client.NewLock(Encoding.ASCII.GetBytes("test2_async"), 5, 5);
                await l.AcquireAsync();
                await l.ReleaseAsync();
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
                Event e = client.NewEvent(Encoding.ASCII.GetBytes("event1"), 5, 60, true);
                Assert.IsTrue(e.IsSet());
                e.Clear();
                Assert.IsFalse(e.IsSet());
                e.Set();
                Assert.IsTrue(e.IsSet());
                e.Wait(2);
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
                Event e = client.NewEvent(Encoding.ASCII.GetBytes("event1_async"), 5, 60, true);
                Assert.IsTrue(await e.IsSetAsync());
                await e.ClearAsync();
                Assert.IsFalse(await e.IsSetAsync());
                await e.SetAsync();
                Assert.IsTrue(await e.IsSetAsync());
                await e.WaitAsync(2);
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
                Event e = client.NewEvent(Encoding.ASCII.GetBytes("event2"), 5, 60, false);
                Assert.IsFalse(e.IsSet());
                e.Set();
                Assert.IsTrue(e.IsSet());
                e.Clear();
                Assert.IsFalse(e.IsSet());
                e.Set();
                Assert.IsTrue(e.IsSet());
                e.Wait(2);
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
                Event e = client.NewEvent(Encoding.ASCII.GetBytes("event2_async"), 5, 60, false);
                Assert.IsFalse(await e.IsSetAsync());
                await e.SetAsync();
                Assert.IsTrue(await e.IsSetAsync());
                await e.ClearAsync();
                Assert.IsFalse(await e.IsSetAsync());
                await e.SetAsync();
                Assert.IsTrue(await e.IsSetAsync());
                await e.WaitAsync(2);
            }
            finally
            {
                await client.CloseAsync();
            }
        }

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
                MaxConcurrentFlow maxConcurrentFlow1 = client.NewMaxConcurrentFlow("maxconcurrentflow1", (ushort)5, 60, 60);
                MaxConcurrentFlow maxConcurrentFlow2 = client.NewMaxConcurrentFlow("maxconcurrentflow1", (ushort)5, 60, 60);
                maxConcurrentFlow1.Acquire();
                maxConcurrentFlow2.Acquire();
                maxConcurrentFlow1.Release();
                maxConcurrentFlow2.Release();
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
                MaxConcurrentFlow maxConcurrentFlow1 = client.NewMaxConcurrentFlow("maxconcurrentflow1_async", (ushort)5, 60, 60);
                MaxConcurrentFlow maxConcurrentFlow2 = client.NewMaxConcurrentFlow("maxconcurrentflow1_async", (ushort)5, 60, 60);
                await maxConcurrentFlow1.AcquireAsync();
                await maxConcurrentFlow2.AcquireAsync();
                await maxConcurrentFlow1.ReleaseAsync();
                await maxConcurrentFlow2.ReleaseAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }

        [TestMethod]
        public void testTokenBucketFlow()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            client.Open();

            try
            {
                TokenBucketFlow tokenBucketFlow1 = client.NewTokenBucketFlow("tokenbucketflow1", (ushort)5, 60, 0.1);
                TokenBucketFlow tokenBucketFlow2 = client.NewTokenBucketFlow("tokenbucketflow1", (ushort)5, 60, 0.1);
                tokenBucketFlow1.Acquire();
                tokenBucketFlow2.Acquire();
            }
            finally
            {
                client.Close();
            }
        }

        [TestMethod]
        public async Task testTokenBucketFlowAsync()
        {
            SlockClient client = new SlockClient(clientHost, clinetPort);
            await client.OpenAsync();

            try
            {
                TokenBucketFlow tokenBucketFlow1 = client.NewTokenBucketFlow("tokenbucketflow1_async", (ushort)5, 60, 0.1);
                TokenBucketFlow tokenBucketFlow2 = client.NewTokenBucketFlow("tokenbucketflow1_async", (ushort)5, 60, 0.1);
                await tokenBucketFlow1.AcquireAsync();
                await tokenBucketFlow2.AcquireAsync();
            }
            finally
            {
                await client.CloseAsync();
            }
        }
    }
}
