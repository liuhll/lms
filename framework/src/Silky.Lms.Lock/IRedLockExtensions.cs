﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Silky.Lms.Core;
using Silky.Lms.Core.Exceptions;
using Microsoft.Extensions.Options;
using RedLockNet;
using Silky.Lms.Lock.Configuration;
using Silky.Lms.Lock.Provider;

namespace Silky.Lms.Lock
{
    public static class IRedLockExtensions
    {
        public static async Task Lock(this IRedLock redLock, Func<Task> callback)
        {
            var lockOptions = EngineContext.Current.Resolve<IOptions<LockOptions>>().Value;
            var lockerTime = DateTime.Now;
            try
            {
                while (true)
                {
                    if (redLock.IsAcquired)
                    {
                        await callback();
                        break;
                    }

                    redLock.Dispose();
                    Thread.Sleep(lockOptions.Retry);
                    redLock = await EngineContext.Current.Resolve<ILockerProvider>().CreateLockAsync(redLock.Resource);
                    if (DateTime.Now - lockerTime > lockOptions.DefaultExpiryTimeSpan)
                    {
                        throw new LmsException($"获取分布式锁资源{redLock.Resource}超时");
                    }
                }
            }
            finally
            {
               redLock?.Dispose();
            }
           
        }

        public static async Task<T> Lock<T>(this IRedLock redLock, Func<Task<T>> callback)
        {
            var lockerTime = DateTime.Now;
            var lockOptions = EngineContext.Current.Resolve<IOptions<LockOptions>>().Value;
            try
            {
                while (true)
                {
                    if (redLock.IsAcquired)
                    {
                        return await callback();
                    }
                    redLock.Dispose();
                    Thread.Sleep(lockOptions.Retry);
                    redLock = await EngineContext.Current.Resolve<ILockerProvider>().CreateLockAsync(redLock.Resource);
                    if (DateTime.Now - lockerTime > lockOptions.DefaultExpiryTimeSpan)
                    {
                        throw new LmsException($"获取分布式锁资源{redLock.Resource}超时");
                    }
                }
            }
            finally
            {
                redLock?.Dispose();
            }
        }
    }
}