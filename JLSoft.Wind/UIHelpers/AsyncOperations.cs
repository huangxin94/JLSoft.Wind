using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLSoft.Wind.UIHelpers
{
    // 文档1: AsyncOperations.cs 修改
    public static class AsyncOperations
    {
        // 保留原有方法，使用默认文字
        public static async Task<T> RunWithLoading<T>(
            Form parentForm,
            Func<CancellationToken, Task<T>> operation)
        {
            return await RunWithLoading(parentForm, operation, "处理中...");
        }

        // 新增重载方法，支持自定义文字
        public static async Task<T> RunWithLoading<T>(
            Form parentForm,
            Func<CancellationToken, Task<T>> operation,
            string loadingText)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            LoadingOverlay loading = null;
            CancellationTokenSource cts = null;

            try
            {
                // 在UI线程创建加载窗口
                parentForm.Invoke((Action)(() =>
                {
                    try
                    {
                        loading = new LoadingOverlay(parentForm, loadingText);
                        cts = new CancellationTokenSource(15000); // 15秒超时

                        loading.Shown += async (sender, e) =>
                        {
                            try
                            {
                                // 使用链接取消令牌
                                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                                    cts.Token,
                                    new CancellationToken() // 空令牌仅用于结构
                                );

                                var result = await operation(linkedCts.Token);
                                tcs.TrySetResult(result);
                            }
                            catch (OperationCanceledException)
                            {
                                tcs.TrySetCanceled();
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                            finally
                            {
                                // 安全关闭窗体
                                parentForm.Invoke((Action)(() =>
                                {
                                    if (loading != null && !loading.IsDisposed)
                                    {
                                        loading.SafeClose();
                                    }
                                }));
                            }
                        };

                        // 模态显示窗口
                        loading.ShowDialog(parentForm);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }));

                // 设置超时监控
                var timeoutTask = Task.Delay(15000).ContinueWith(_ =>
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.TrySetException(new TimeoutException("操作超时（15秒）"));
                        cts?.Cancel();
                    }
                });

                // 等待操作完成或超时
                return await tcs.Task;
            }
            finally
            {
                // 安全释放资源
                cts?.Dispose();
                parentForm.Invoke((Action)(() =>
                {
                    if (loading != null && !loading.IsDisposed)
                    {
                        loading.SafeClose();
                        loading.Dispose();
                    }
                }));
            }
        }
    }
}
