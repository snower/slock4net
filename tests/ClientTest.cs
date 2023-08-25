using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using slock4net;
using slock4net.Exceptions;
using tests;

namespace tests
{
    [TestClass]
    public class ClientTest
    {
        static string clientHost = "localhost";
        static int clinetPort = 5658;

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
        public void TestReplsetClientLock()
        {
            SlockReplsetClient client = new SlockReplsetClient(new string[]{clientHost + ":" + clinetPort
    });
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

        private void TestChildTreeLock(SlockClient client, TreeLock rootLock, TreeLock childLock, TreeLock.TreeLeafLock treeLeafLock, int depth)
        {
            TreeLock.TreeLeafLock clock1 = childLock.NewLeafLock();
            clock1.acquire();

            TreeLock.TreeLeafLock clock2 = childLock.NewLeafLock();
            clock2.acquire();

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
                treeLeafLock.acquire();
                TestChildTreeLock(client, childLock, childLock.NewChild(), treeLeafLock, depth - 1);
                treeLeafLock.acquire();
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

            TreeLock rootLock = client.newTreeLock("TestTreeLock", 5, 10);

            rootLock.Acquire();
            rootLock.Release();
            rootLock.Wait(1);

            TreeLock.TreeLeafLock treeLeafLock = rootLock.NewLeafLock();
            treeLeafLock.acquire();

            TestChildTreeLock(client, rootLock, rootLock.NewChild(), treeLeafLock, 5);

            rootLock.Wait(1);
            Lock testLock = client.NewLock(rootLock.GetLockKey(), 0, 0);
            testLock.Acquire();
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
    }
}
