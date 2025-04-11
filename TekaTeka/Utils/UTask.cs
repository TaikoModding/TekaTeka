using System.Collections;
using System.Runtime.CompilerServices;
using Il2CppInterop.Runtime;
using Exception = Il2CppSystem.Exception;
using Cysharp.Threading.Tasks;
using CancellationToken = Il2CppSystem.Threading.CancellationToken;

namespace TekaTeka.Utils;

public readonly struct UTask(UniTask uniTask)
{
    public static implicit operator UTask(UniTask uniTask)
    {
        return new UTask(uniTask);
    }

    public Awaiter GetAwaiter()
    {
        return new Awaiter(uniTask.GetAwaiter());
    }

    public readonly struct Awaiter(UniTask.Awaiter uAwaiter) : INotifyCompletion
    {
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

public readonly struct UTask<T>(UniTask<T> uniTask)
{
    public static implicit operator UTask<T>(UniTask<T> uniTask)
    {
        return new UTask<T>(uniTask);
    }

    public Awaiter<T> GetAwaiter()
    {
        return new Awaiter<T>(uniTask.GetAwaiter());
    }

    public readonly struct Awaiter<UT>(UniTask<UT>.Awaiter uAwaiter) : INotifyCompletion
    {
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

    public static UniTask ToUniTask(this Task uniTask)
    {
        var pred = DelegateSupport.ConvertDelegate<Il2CppSystem.Func<bool>>(() => uniTask.IsCompleted);
        return UniTask.WaitUntil(pred, PlayerLoopTiming.Update, new CancellationToken(false));
    }

    public static IEnumerator Await<T>(this UniTask<T> uniTask, Action<T> onResult = null,
        Action<System.Exception> onException = null)
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