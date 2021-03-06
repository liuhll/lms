﻿using System;

namespace Silky.Lock.Configuration
{
    public class LockOptions
    {
        public static string Lock = "Lock";

        public string LockRedisConnection { get; set; }

        public int DefaultExpiry { get; set; } = 120;

        public int Wait { get; set; } = 30;

        public int Retry { get; set; } = 1;

        public TimeSpan DefaultExpiryTimeSpan => TimeSpan.FromSeconds(DefaultExpiry);

        public TimeSpan WaitTimeSpan => TimeSpan.FromSeconds(Wait);

        public TimeSpan RetryTimeSpan => TimeSpan.FromSeconds(Retry);
    }
}