using System.Collections;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Il2CppInterop.Runtime;
using Action = System.Action;
using Exception = Il2CppSystem.Exception;

namespace TekaTeka.Utils;

public readonly struct UTask
{
    private readonly UniTask uniTask;

    public UTask(UniTask uniTask)
    {
        this.uniTask = uniTask;
    }

    public static implicit operator UTask(UniTask uniTask)
    {
        return new UTask(uniTask);
    }

    public Awaiter GetAwaiter()
    {
        return new Awaiter(uniTask.GetAwaiter());
    }

    public readonly struct Awaiter : INotifyCompletion
    {
        private readonly UniTask.Awaiter uAwaiter;

        public Awaiter(UniTask.Awaiter uAwaiter)
        {
            this.uAwaiter = uAwaiter;
        }

        public bool IsCompleted => uAwaiter.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            uAwaiter.OnCompleted(continuation);
        }

        public void GetResult()
        {
            uAwaiter.GetResult();
        }
    }
}

public readonly struct UTask<T>
{
    private readonly UniTask<T> uniTask;

    public UTask(UniTask<T> uniTask)
    {
        this.uniTask = uniTask;
    }

    public static implicit operator UTask<T>(UniTask<T> uniTask)
    {
        return new UTask<T>(uniTask);
    }

    public Awaiter<T> GetAwaiter()
    {
        return new Awaiter<T>(uniTask.GetAwaiter());
    }

    public readonly struct Awaiter<UT> : INotifyCompletion
    {
        private readonly UniTask<UT>.Awaiter uAwaiter;

        public Awaiter(UniTask<UT>.Awaiter awaiter)
        {
            uAwaiter = awaiter;
        }

        public bool IsCompleted => uAwaiter.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            uAwaiter.OnCompleted(continuation);
        }

        public UT GetResult()
        {
            return uAwaiter.GetResult();
        }
    }
}

public static class UTaskExt
{
    public static UTask<T> ToTask<T>(this UniTask<T> uniTask)
    {
        return new UTask<T>(uniTask);
    }

    public static UTask ToTask(this UniTask uniTask)
    {
        return new UTask(uniTask);
    }

    public static UniTask ToUniTask(this Task task)
    {
        UniTaskCompletionSource? source = new UniTaskCompletionSource();

        task.ContinueWith(task1 =>
        {
            if (task1.Exception != null)
            {
                if (source == null)
                    Plugin.Log.LogError(task1.Exception);
                else
                {
                    var ex = new Exception(task1.Exception.Message);
                    source.TrySetException(ex);
                }
            }
            else if (task1.IsCanceled)
                source?.TrySetCanceled();
            else
                source?.TrySetResult();
        });

        return source.Task;
    }

    public static IEnumerator Await<T>(this UniTask<T> uniTask, System.Action<T> onResult = null,
        System.Action<System.Exception> onException = null)
    {
        var result = default(T);
        Exception ex = null;
        var co = uniTask.ToCoroutine(
            DelegateSupport.ConvertDelegate<Il2CppSystem.Action<T>>(
                (T r) => { result = r; }
            ),
            DelegateSupport.ConvertDelegate<Il2CppSystem.Action<Exception>>(
                (Exception exception) => { ex = exception; }
            )
        );

        yield return co;
        if (ex != null)
        {
            if (onException == null)
                Plugin.Log.LogError($"Failed to execute UniTask: {ex}");
            else
                onException.Invoke(new System.Exception(ex.Message));
        }
        else
        {
            onResult?.Invoke(result);
        }
    }
}