using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
   
   [SerializeField] private float swipeThreshold = 0.5f; // Minimum distance for a swipe
   public static InputHandler Instance { get; private set; }
   private PlayerInputActions inputActions;
   
   public delegate void SwipeAction(Vector2 direction);
   public event SwipeAction OnSwipe;

   public delegate void ClickAction(Vector2 position);
   public event ClickAction OnClick;

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
    private void OnMouseDown()
    {
   
        // Convert the mouse position to world coordinates
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
    }

    private void OnMouseUp()
    {
    

        // Convert the final mouse position to world coordinates
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      

        CalculateAngle();
        
    }

    void CalculateAngle()
    {
        float swipeDistance = Vector2.Distance(finalTouchPosition, firstTouchPosition);

        if (swipeDistance < swipeThreshold)
        {
            return; // Treat this as a click, not a swipe
        }

        // If the swipe distance is valid, calculate the swipe angle
        float swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                                        finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

        Debug.Log($"Swipe Angle: {swipeAngle}");

        // Trigger the swipe event
        OnSwipe?.Invoke(finalTouchPosition - firstTouchPosition);
    }

 
    private void OnEnable()
    {
        inputActions.Enable();
    
    }

    private void OnDisable()
    {
       
        inputActions.Disable();
    }

    private void HandleSwipe(InputAction.CallbackContext callbackContext)
    {
        Vector2 swipeDelta = callbackContext.ReadValue<Vector2>();
        OnSwipe?.Invoke(swipeDelta);
    }

    private void HandleClick(InputAction.CallbackContext callbackContext)
    {
        Vector2 clickPosition = Mouse.current.position.ReadValue();
        OnClick?.Invoke(clickPosition);
    }
}
