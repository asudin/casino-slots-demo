using UnityEngine;
using UnityEngine.Events;

public interface ISwipeSubscriber
{
    void SubscribeToSwipe(SwipeData swipeData);
}

public class SwipeController : MonoBehaviour
{
    [SerializeField]
    private bool _isInteractable = true;
    [SerializeField]
    private bool _isSwipeDetectedOnlyAfterRelease = false;
    [SerializeField]
    private float _minSwipeDistanceX = 20f;
    [SerializeField]
    private float _minSwipeDistanceY = 20f;

    public bool IsLastSwipeIgnored;

    private static SwipeController _instance;
    private Vector2 _fingerUpPosition;
    private Vector2 _fingerDownPosition;
    private bool _isSwipeCompleted = true;
    private SwipeDirection _lastSwipeDirection;

    public UnityEvent<SwipeData> OnSwipe = new UnityEvent<SwipeData>();


    public static SwipeController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SwipeController>();
            }

            return _instance;
        }
    }

    private void Update()
    {
        if (_isInteractable)
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                HandleTouch();
            }
            else
            {
                HandleMouseInput();
            }
        }   
    }

    private void HandleTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _fingerDownPosition = touch.position;
                    _fingerUpPosition = touch.position;
                    break;

                case TouchPhase.Moved:
                    if (!_isSwipeDetectedOnlyAfterRelease)
                    {
                        _fingerUpPosition = touch.position;
                        DetectSwipe();
                    }
                    break;

                case TouchPhase.Ended:
                    _fingerUpPosition = touch.position;
                    DetectSwipe();
                    _isSwipeCompleted = true;
                    break;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _fingerUpPosition = Input.mousePosition;
            _fingerDownPosition = Input.mousePosition;
        }

        if (!_isSwipeDetectedOnlyAfterRelease && Input.GetMouseButton(0))
        {
            _fingerUpPosition = Input.mousePosition;
            DetectSwipe();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _fingerUpPosition = Input.mousePosition;
            DetectSwipe();
            _isSwipeCompleted = true;
        }
    }

    private void DetectSwipe()
    {
        if (SwipeDistanceCheckMet())
        {
            if (IsVerticalSwipe())
            {
                var direction = _fingerUpPosition.y - _fingerDownPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                if (_isSwipeCompleted || _lastSwipeDirection != direction || IsLastSwipeIgnored)
                {
                    _isSwipeCompleted = false;
                    _lastSwipeDirection = direction;
                    SendSwipe(direction);
                }
            }
            else
            {
                var direction = _fingerUpPosition.x - _fingerDownPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                if (_isSwipeCompleted || _lastSwipeDirection != direction || IsLastSwipeIgnored)
                {
                    _isSwipeCompleted = false;
                    _lastSwipeDirection = direction;
                    SendSwipe(direction);
                }
            }
            _fingerDownPosition = _fingerUpPosition;
        }
    }

    private void SendSwipe(SwipeDirection direction)
    {
        SwipeData swipeData = new SwipeData()
        {
            Direction = direction,
            StartPosition = _fingerUpPosition,
            EndPosition = _fingerDownPosition
        };
        OnSwipe?.Invoke(swipeData);
    }

    private bool IsVerticalSwipe() =>
        VerticalMovementDistance() > HorizontalMovementDistance();

    private bool SwipeDistanceCheckMet()
    {
        return VerticalMovementDistance() > _minSwipeDistanceY
            || HorizontalMovementDistance() > _minSwipeDistanceX;
    }

    private float VerticalMovementDistance()
    {
        return Mathf.Abs(_fingerUpPosition.y - _fingerDownPosition.y);
    }

    private float HorizontalMovementDistance()
    {
        return Mathf.Abs(_fingerUpPosition.x - _fingerDownPosition.x);
    }
}

public struct SwipeData
{
    public Vector2 StartPosition;
    public Vector2 EndPosition;
    public SwipeDirection Direction;
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right
}