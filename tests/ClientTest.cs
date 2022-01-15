using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using slock4net;

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

            SlockReplsetClient repletClient = new SlockReplsetClient(new string[]{clientHost + ":" + clinetPort
    });
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
    }
}
