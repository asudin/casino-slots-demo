using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITimerSubscriber
{
    void OnTimerStart();
    void OnTimerEnd();
    void OnTimerUpdate(float remainingTime, float progress);
}

public class Timer
{
    public event Action OnTimerStart;
    public event Action OnTimerEnd;
    public event Action<float, float> OnTimerUpdate;

    public float _duration;
    public bool _isEditorialEventAllowed = true;
    private float _updateInterval = 0.1f;
    private bool _isRunning;
    private CancellationTokenSource cancellationTokenSource;

    public Timer(float duration, float updateInterval = 0.1f)
    {
        _duration = duration;
        _updateInterval = updateInterval;
    }

    public void ResetTimer(float newDuration, float newUpdateInterval = 0.1f)
    {
        StopTimer();

        _duration = newDuration;
        _updateInterval = newUpdateInterval;
    }

    public async void StartTimer()
    {
        if (_isRunning)
        {
            await Task.Delay(100);
        }

        if (!_isRunning)
        {
            _isRunning = true;
#if !UNITY_EDITOR
            OnTimerStart?.Invoke();
#else
            if (_isEditorialEventAllowed)
            {
                OnTimerStart?.Invoke();
            }
#endif
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await TimerCoroutine(cancellationTokenSource.Token);
#if !UNITY_EDITOR
                OnTimerEnd?.Invoke();
#else
                if (_isEditorialEventAllowed)
                {
                    OnTimerEnd?.Invoke();
                }
#endif
            }
            catch (OperationCanceledException) { }

            _isRunning = false;
        }
    }

    public void StopTimer()
    {
        if (_isRunning)
        {
            cancellationTokenSource.Cancel();
        }
    }

    public async void RestartTimer()
    {

        StopTimer();

        await Task.Delay(100);

        cancellationTokenSource = null;
        StartTimer();
    }

    public bool IsTimerRunning() =>
        _isRunning;

    private async Task TimerCoroutine(CancellationToken cancellationToken)
    {
        float remainingTime = _duration;

        while (remainingTime > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(_updateInterval), cancellationToken);

            remainingTime -= _updateInterval;

            float progress = (remainingTime / _duration);
#if !UNITY_EDITOR
            OnTimerUpdate?.Invoke(remainingTime, progress);
#else
            if (_isEditorialEventAllowed)
            {
                OnTimerUpdate?.Invoke(remainingTime, progress);
            }
#endif
        }
    }
}