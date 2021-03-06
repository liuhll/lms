using Microsoft.Extensions.Configuration;
using Silky.Core;
using Silky.Core.Extensions;
using StackExchange.Profiling;

namespace Silky.Rpc
{
    public class DefaultMiniProfiler : IMiniProfiler
    {
        /// <summary>
        /// 打印验证信息到 MiniProfiler
        /// </summary>
        /// <param name="category">分类</param>
        /// <param name="state">状态</param>
        /// <param name="message">消息</param>
        /// <param name="isError">是否为警告消息</param>
        public void Print(string category, string state, string message = null, bool isError = false)
        {
            var injectMiniProfiler = EngineContext.Current.Configuration.GetValue<bool?>("gateway:injectMiniProfiler") ?? true;
            if (!injectMiniProfiler) return;
            // 打印消息
            var customTiming = StackExchange.Profiling.MiniProfiler.Current?.CustomTiming(category,
                string.IsNullOrWhiteSpace(message) ? $"{category.ToTitleCase()} {state}" : message, state);
            if (customTiming == null) return;
            // 判断是否是警告消息
            if (isError) customTiming.Errored = true;
        }
    }
}