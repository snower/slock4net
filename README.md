# slock4net

[![Tests](https://img.shields.io/github/actions/workflow/status/snower/slock4net/build-test.yml?label=tests)](https://github.com/snower/slock4net/actions/workflows/build-test.yml)
[![GitHub Repo stars](https://img.shields.io/github/stars/snower/slock4net?style=social)](https://github.com/snower/slock4net/stargazers)

High-performance distributed sync service and atomic DB. Provides good multi-core support through lock queues, high-performance asynchronous binary network protocols. Can be used for spikes, synchronization, event notification, concurrency control. https://github.com/snower/slock

# Install

```bash
NuGet\Install-Package slock4net -Version 1.0.3
```

# Client Lock

```C#
using slock4net.Exceptions;
using slock4net;

SlockClient client = new SlockClient("localhost", 5658);
try
{
    client.Open();
    Lock lck = client.NewLock("test", 5, 5);
    lck.Acquire();
    lck.Release();
}
catch (IOException e) {
    Console.WriteLine(e.ToString());
}
catch (SlockException e) {
    Console.WriteLine(e.ToString());
} finally
{
    client.Close();
}
Console.WriteLine("Success");
```

# Replset Client Lock

```C#
using slock4net.Exceptions;
using slock4net;

SlockReplsetClient replsetClient = new SlockReplsetClient(new String[] { "localhost:5658" });
try
{
    replsetClient.Open();
    Lock lck = replsetClient.NewLock("test", 5, 5);
    lck.Acquire();
    lck.Release();
}
catch (IOException e) {
    Console.WriteLine(e.ToString());
}
catch (SlockException e) {
    Console.WriteLine(e.ToString());
} finally
{
    replsetClient.Close();
}
Console.WriteLine("Success");
```

# Async Client Lock

```C#
using slock4net.Exceptions;
using slock4net;

async Task TestLock()
{
    SlockReplsetClient replsetClient = new SlockReplsetClient(new String[] { "localhost:5658" });
    try
    {
        await replsetClient.OpenAsync();
        Lock lck = replsetClient.NewLock("test", 5, 5);
        await lck.AcquireAsync();
        await lck.ReleaseAsync();
    }
    catch (IOException e)
    {
        Console.WriteLine(e.ToString());
    }
    catch (SlockException e)
    {
        Console.WriteLine(e.ToString());
    }
    finally
    {
        await replsetClient.CloseAsync();
    }
}
TestLock().Wait();
Console.WriteLine("Success");
```

# Event

```C#
using slock4net.Exceptions;
using slock4net;

SlockClient client = new SlockClient("localhost", 5658);
try
{
    client.Open();
    Event event1 = client.NewEvent("test", 5, 5, true);
    event1.Clear();

    Event event2 = client.NewEvent("test", 5, 5, true);
    event2.Set("{\"value\": 10}");

    event1.Wait(10);
    Console.WriteLine(event1.CurrentLockDataAsString);
}
catch (IOException e) {
    Console.WriteLine(e.ToString());
}
catch (SlockException e) {
    Console.WriteLine(e.ToString());
} finally
{
    client.Close();
}
Console.WriteLine("Success");

```