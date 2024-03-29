using slock4net.Commands;
using slock4net.Exceptions;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class Event
    {
        private SlockDatabase database;
        private byte[] eventKey;
        private uint timeout;
        private uint expried;
        private Lock eventLock;
        private Lock checkLock;
        private Lock waitLock;
        private bool defaultSeted;

        public Event(SlockDatabase database, byte[] eventKey, uint timeout, uint expried, bool defaultSeted)
        {
            this.database = database;
            if (eventKey.Length > 16)
            {
                using (MD5 md5 = MD5.Create())
                {
                    this.eventKey = md5.ComputeHash(eventKey);
                }
            }
            else
            {
                this.eventKey = new byte[16];
                Array.Copy(eventKey, 0, this.eventKey, 16 - eventKey.Length, eventKey.Length);
            }
            this.timeout = timeout;
            this.expried = expried;
            this.defaultSeted = defaultSeted;
        }

        public Event(SlockDatabase database, byte[] eventKey, uint timeout, uint expried) : this(database, eventKey, timeout, expried, true)
        {
        }

        public Event(SlockDatabase database, string eventKey, uint timeout, uint expried, bool defaultSeted) : this(database, Encoding.UTF8.GetBytes(eventKey), timeout, expried, defaultSeted)
        {

        }

        public Event(SlockDatabase database, string eventKey, uint timeout, uint expried) : this(database, Encoding.UTF8.GetBytes(eventKey), timeout, expried, true)
        {
        }

        public void Clear()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                }
                catch (LockLockedException)
                {
                }
                return;
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                eventLock.Release();
            }
            catch (LockUnlockedException)
            {
            }
        }

        public async Task ClearAsync()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                }
                catch (LockLockedException)
                {
                }
                return;
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                await eventLock.ReleaseAsync();
            }
            catch (LockUnlockedException)
            {
            }
        }

        public void Set()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    eventLock.Release();
                }
                catch (LockUnlockedException)
                {
                }
                return;
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
            }
            catch (LockLockedException)
            {
            }
        }

        public async Task SetAsync()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    await eventLock.ReleaseAsync();
                }
                catch (LockUnlockedException)
                {
                }
                return;
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
            }
            catch (LockLockedException)
            {
            }
        }

        public bool IsSet()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (checkLock == null)
                    {
                        checkLock = new Lock(database, eventKey, null, 0, 0, 0, 0);
                    }
                }
                try
                {
                    checkLock.Acquire();
                }
                catch (LockTimeoutException)
                {
                    return false;
                }
                return true;
            }

            lock (this)
            {
                if (checkLock == null)
                {
                    checkLock = new Lock(database, eventKey, null, 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                checkLock.Acquire();
            }
            catch (LockNotOwnException)
            {
                return false;
            }
            catch (LockTimeoutException)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsSetAsync()
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (checkLock == null)
                    {
                        checkLock = new Lock(database, eventKey, null, 0, 0, 0, 0);
                    }
                }
                try
                {
                    await checkLock.AcquireAsync();
                }
                catch (LockTimeoutException)
                {
                    return false;
                }
                return true;
            }

            lock (this)
            {
                if (checkLock == null)
                {
                    checkLock = new Lock(database, eventKey, null, 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                await checkLock.AcquireAsync();
            }
            catch (LockNotOwnException)
            {
                return false;
            }
            catch (LockTimeoutException)
            {
                return false;
            }
            return true;
        }

        public void Wait(uint timeout)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (waitLock == null)
                    {
                        waitLock = new Lock(database, eventKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    waitLock.Acquire();
                }
                catch (LockTimeoutException)
                {
                    throw new EventWaitTimeoutException();
                }
                catch (ClientCommandTimeoutException)
                {
                    throw new EventWaitTimeoutException();
                }
                return;
            }

            lock (this)
            {
                if (waitLock == null)
                {
                    waitLock = new Lock(database, eventKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }

            try
            {
                waitLock.Acquire();
            }
            catch (LockTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
            catch (ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
        }

        public async Task WaitAsync(uint timeout)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (waitLock == null)
                    {
                        waitLock = new Lock(database, eventKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    await waitLock.AcquireAsync();
                }
                catch (LockTimeoutException)
                {
                    throw new EventWaitTimeoutException();
                }
                catch (ClientCommandTimeoutException)
                {
                    throw new EventWaitTimeoutException();
                }
                return;
            }

            lock (this)
            {
                if (waitLock == null)
                {
                    waitLock = new Lock(database, eventKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }

            try
            {
                await waitLock.AcquireAsync();
            }
            catch (LockTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
            catch (ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }
        }

        public void WaitAndTimeoutRetryClear(uint timeout)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (waitLock == null)
                    {
                        waitLock = new Lock(database, eventKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    waitLock.Acquire();
                }
                catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
                {
                    lock (this)
                    {
                        if (eventLock == null)
                        {
                            eventLock = new Lock(database, eventKey, eventKey, this.timeout, expried, 0, 0);
                        }
                    }

                    try
                    {
                        eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                        try
                        {
                            eventLock.Release();
                        }
                        catch (SlockException)
                        {
                        }
                        return;
                    }
                    catch (SlockException)
                    {
                    }
                    throw new EventWaitTimeoutException();
                }
                return;
            }

            lock (this)
            {
                if (waitLock == null)
                {
                    waitLock = new Lock(database, eventKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                waitLock.Acquire();
            }
            catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, this.timeout, expried, 1, 0);
                }
            }
            try
            {
                eventLock.Release();
            }
            catch (SlockException)
            {
            }
        }

        public async Task WaitAndTimeoutRetryClearAsync(uint timeout)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (waitLock == null)
                    {
                        waitLock = new Lock(database, eventKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    await waitLock.AcquireAsync();
                }
                catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
                {
                    lock (this)
                    {
                        if (eventLock == null)
                        {
                            eventLock = new Lock(database, eventKey, eventKey, this.timeout, expried, 0, 0);
                        }
                    }

                    try
                    {
                        await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                        try
                        {
                            await eventLock.ReleaseAsync();
                        }
                        catch (SlockException)
                        {
                        }
                        return;
                    }
                    catch (SlockException)
                    {
                    }
                    throw new EventWaitTimeoutException();
                }
                return;
            }

            lock (this)
            {
                if (waitLock == null)
                {
                    waitLock = new Lock(database, eventKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                await waitLock.AcquireAsync();
            }
            catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, eventKey, eventKey, this.timeout, expried, 1, 0);
                }
            }
            try
            {
                await eventLock.ReleaseAsync();
            }
            catch (SlockException)
            {
            }
        }
    }
}