using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    private PlayerInputActions inputActions;

    [SerializeField] private float swipeThreshold = 0.5f; // Minimum distance for a swipe
   
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.Swipe.performed += OnSwipe;
    }
    private void OnDisable()
    {
        inputActions.Gameplay.Swipe.performed -= OnSwipe;
        inputActions.Gameplay.Disable();
    }

    private void OnSwipe(InputAction.CallbackContext obj)
    {
        Debug.Log("Swipeddd");
        Vector2 swipeDelta = obj.ReadValue<Vector2>();

        if (swipeDelta.magnitude > swipeThreshold)
        {
            float angle = Mathf.Atan2(swipeDelta.y, swipeDelta.x) * Mathf.Rad2Deg;

            if (angle > -45 && angle <= 45)
                Debug.Log("Swipe Right");
            else if (angle > 45 && angle <= 135)
                Debug.Log("Swipe Up");
            else if (angle > 135 || angle <= -135)
                Debug.Log("Swipe Left");
            else if (angle < -45 && angle >= -135)
                Debug.Log("Swipe Down");
        }
    }

  
}
