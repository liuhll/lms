﻿using System;
using System.Diagnostics.CodeAnalysis;
using Silky.Core;

namespace Silky.Rpc.Transport.CachingIntercept
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RemoveCachingInterceptAttribute : Attribute, IRemoveCachingInterceptProvider
    {
        public RemoveCachingInterceptAttribute([NotNull]string cacheName,[NotNull]string keyTemplete)
        {
            Check.NotNullOrEmpty(cacheName, nameof(cacheName));
            Check.NotNullOrEmpty(keyTemplete, nameof(keyTemplete));
            CacheName = cacheName;
            KeyTemplete = keyTemplete;
            OnlyCurrentUserData = false;
        }
        
        public string CacheName { get; }
        public string KeyTemplete { get; }
        
        public bool OnlyCurrentUserData { get; set; }
    }
    
}