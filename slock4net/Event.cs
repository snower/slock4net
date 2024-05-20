using slock4net.Commands;
using slock4net.datas;
using slock4net.Exceptions;
using System.Text;
using System.Threading.Tasks;

namespace slock4net
{
    public class Event : AbstractExecution
    {
        private Lock eventLock;
        private Lock checkLock;
        private Lock waitLock;
        private bool defaultSeted;

        public Event(SlockDatabase database, byte[] eventKey, uint timeout, uint expried, bool defaultSeted) : base(database, eventKey, timeout, expried)
        {
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
            Clear((LockData) null);
        }

        public void Clear(byte[] data)
        {
            Clear(new LockSetData(data));
        }

        public void Clear(string data)
        {
            Clear(new LockSetData(data));
        }

        public void Clear(LockData lockData)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
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
                    eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                eventLock.Release(lockData);
            }
            catch (LockUnlockedException)
            {
            }
        }

        public async Task ClearAsync()
        {
            await ClearAsync((LockData) null);
        }

        public async Task ClearAsync(byte[] data)
        {
            await ClearAsync(new LockSetData(data));
        }

        public async Task ClearAsync(string data)
        {
            await ClearAsync(new LockSetData(data));
        }

        public async Task ClearAsync(LockData lockData)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
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
                    eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                await eventLock.ReleaseAsync(lockData);
            }
            catch (LockUnlockedException)
            {
            }
        }

        public void Set()
        {
            Set((LockData) null);
        }

        public void Set(byte[] data)
        {
            Set(new LockSetData(data));
        }

        public void Set(string data)
        {
            Set(new LockSetData(data));
        }

        public void Set(LockData lockData)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    eventLock.Release(lockData);
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
                    eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
            }
            catch (LockLockedException)
            {
            }
        }

        public async Task SetAsync()
        {
            await SetAsync((LockData) null);
        }

        public async Task SetAsync(byte[] data)
        {
            await SetAsync(new LockSetData(data));
        }

        public async Task SetAsync(string data)
        {
            await SetAsync(new LockSetData(data));
        }

        public async Task SetAsync(LockData lockData)
        {
            if (defaultSeted)
            {
                lock (this)
                {
                    if (eventLock == null)
                    {
                        eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 0, 0);
                    }
                }
                try
                {
                    await eventLock.ReleaseAsync(lockData);
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
                    eventLock = new Lock(database, lockKey, lockKey, timeout, expried, 1, 0);
                }
            }
            try
            {
                await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, lockData);
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
                        checkLock = new Lock(database, lockKey, null, 0, 0, 0, 0);
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
                    checkLock = new Lock(database, lockKey, null, 0x02000000, 0, 1, 0);
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
                        checkLock = new Lock(database, lockKey, null, 0, 0, 0, 0);
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
                    checkLock = new Lock(database, lockKey, null, 0x02000000, 0, 1, 0);
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
                        waitLock = new Lock(database, lockKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    waitLock.Acquire();
                    currentLockData = waitLock.CurrentLockData;
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
                    waitLock = new Lock(database, lockKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }

            try
            {
                waitLock.Acquire();
                currentLockData = waitLock.CurrentLockData;
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
                        waitLock = new Lock(database, lockKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    await waitLock.AcquireAsync();
                    currentLockData = waitLock.CurrentLockData;
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
                    waitLock = new Lock(database, lockKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }

            try
            {
                await waitLock.AcquireAsync();
                currentLockData = waitLock.CurrentLockData;
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
                        waitLock = new Lock(database, lockKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    waitLock.Acquire(new LockUnsetData());
                    currentLockData = waitLock.CurrentLockData;
                }
                catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
                {
                    lock (this)
                    {
                        if (eventLock == null)
                        {
                            eventLock = new Lock(database, lockKey, lockKey, this.timeout, expried, 0, 0);
                        }
                    }

                    try
                    {
                        eventLock.Acquire(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED, new LockUnsetData());
                        currentLockData = eventLock.CurrentLockData;
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
                    waitLock = new Lock(database, lockKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                waitLock.Acquire(new LockUnsetData());
                currentLockData = waitLock.CurrentLockData;
            }
            catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, lockKey, lockKey, this.timeout, expried, 1, 0);
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
                        waitLock = new Lock(database, lockKey, null, timeout, 0, 0, 0);
                    }
                }

                try
                {
                    await waitLock.AcquireAsync(new LockUnsetData());
                    currentLockData = waitLock.CurrentLockData;
                }
                catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
                {
                    lock (this)
                    {
                        if (eventLock == null)
                        {
                            eventLock = new Lock(database, lockKey, lockKey, this.timeout, expried, 0, 0);
                        }
                    }

                    try
                    {
                        await eventLock.AcquireAsync(ICommand.LOCK_FLAG_UPDATE_WHEN_LOCKED);
                        currentLockData = eventLock.CurrentLockData;
                        try
                        {
                            await eventLock.ReleaseAsync(new LockUnsetData());
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
                    waitLock = new Lock(database, lockKey, null, timeout | 0x02000000, 0, 1, 0);
                }
            }
            try
            {
                await waitLock.AcquireAsync(new LockUnsetData());
                currentLockData = waitLock.CurrentLockData;
            }
            catch (SlockException e) when (e is LockTimeoutException || e is ClientCommandTimeoutException)
            {
                throw new EventWaitTimeoutException();
            }

            lock (this)
            {
                if (eventLock == null)
                {
                    eventLock = new Lock(database, lockKey, lockKey, this.timeout, expried, 1, 0);
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