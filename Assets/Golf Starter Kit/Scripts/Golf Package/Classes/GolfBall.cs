using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MyApp.Golf
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class GolfBall : MonoBehaviour
    {
        #region enum
        public enum BallEvent { Null = -1, None, FarDistance, BallStopped, BallIsInHole }
        public enum BallState { Null = -1, Idle, Touched, Released, Finish }
        public enum ShadowMode { ByTime, ByDistance }
        #endregion
        #region variable
        #region const
        public const float Zero = 0f;
        #endregion
        public bool enable = true;
        //[Header("Parameter(s)")]
        [Min(0)] public float minForceDistance = 1.0f;
        [Min(0)] public float maxForceDistance = 1.0f;
        [Min(0)] public float maxMovementDistance = 10f;

        //[Header("Force")]
        [Min(0)] public float potentialForceCoefficient = 1.0f;
        [Min(0)] public float potentialForceSensitivity = 1.0f;
        public ForceMode forceMode = ForceMode.Force;
        public bool forceToStop = false;
        [Min(0)] public float velocityThreshold = .1f;
        [Min(0)] public float stopVelocityLerp = .01f;

        //[Header("UI")]
        public GameObject sliderCanvas;
        public Slider slider;
        public Image sliderFillAreaImage;
        public bool solidColor = false;
        public Color color1 = Color.green;
        public Color color2 = Color.red;

        //[Header("Shadow")]
        public bool shadowEnable;
        public GameObject shadowPrefab;
        public Vector3 shadowScale = Vector3.one;
        public ShadowMode shadowMode = ShadowMode.ByDistance;
        public float shadowTTL = 1f;
        public float shadowInstantiateRate = .5f;
        public float shadowInstantiateDistance = 1f;

        #region HideInInspector
        public Vector3 PotentialForceDirection { get; protected set; }
        public float PotentialEnergy { get; protected set; }
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        [HideInInspector] public Camera _camera;
        [HideInInspector] public Rigidbody _rb;
        [HideInInspector] public Collider _collider;
        [HideInInspector] public RectTransform _sliderRT;
        [HideInInspector] public BallState ballState = BallState.Null;
        [HideInInspector] public BallEvent ballEvent = BallEvent.Null;

        public BallState CurrentBallState { get { return ballState; } }
        public BallEvent CurrentBallEvent { get { return ballEvent; } }
        #endregion
        #region private
        private bool secondTouch = false;
        private bool potentialForceReleased;

        private Vector3 startPosition = Vector3.zero;
        #region shadow
        private float _shadowInstantiateRate = 0f;
        private Vector3 lastShadowPosition = Vector3.zero;
        #endregion
        private Vector3 p2;
        #endregion
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showInfoParts = true;
        [HideInInspector] public bool showForceParts = true;
        [HideInInspector] public bool showUIParts = true;
        [HideInInspector] public bool showShadowParts = true;
#endif
        #endregion
        #endregion
        #region Functions
        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
        }
        private void Awake()
        {
            init();
        }
        private void Update()
        {
            if (!enable) return;
            switch (ballState)
            {
                case BallState.Touched:
                    ballState_Touched();
                    break;
                case BallState.Released:
                    if (forceToStop)
                    {
                        if (_rb.velocity != Vector3.zero && _rb.velocity.magnitude < velocityThreshold)
                        {
                            _rb.velocity = Vector3.Lerp(Vector3.zero, _rb.velocity, stopVelocityLerp);
                        }
                    }
                    ballState_Released();
                    break;
                case BallState.Finish:
                    ballState_Finish();
                    break;
                default:
                    ballState_Idle();
                    break;
            }
        }
        private void FixedUpdate()
        {
            if (!enable) return;
            if (secondTouch)
            {
                calculateForce(Position, p2, minForceDistance);
            }
        }
        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //draw Potential Energy
            Gizmos.color = Color.white;
            if (ballState == BallState.Touched || ballState == BallState.Released)
            {
                Gizmos.DrawWireSphere(transform.position, PotentialEnergy);
            }
            //draw velocity
            Debug.DrawRay(Position, _rb.velocity);
            //Draw movement flow
            if (ballState == BallState.Touched && PotentialForceDirection != Vector3.zero)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(Position, PotentialForceDirection * maxMovementDistance);
            }
        }
        private void OnDrawGizmosSelected()
        {
            //force distance
            if (ballState == BallState.Null || ballState == BallState.Idle)
            {
                Handles.color = Color.blue;
                Handles.DrawWireDisc(Position, Vector3.up, minForceDistance);
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(Position, Vector3.up, maxForceDistance);
            }
            //Max Distance
            Handles.color = Color.red;
            Handles.DrawWireDisc(Extensions.isPlaying() ? startPosition : Position, Vector3.up, maxMovementDistance);
        }
#endif
        #endregion
        #endregion
        #region functions
        #region BallState
        private void ballState_Idle()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (secondTouch = isHit())
                {
                    p2 = Position;
                    setSliderActive(true);
                    setSliderPosition(Position);
                    setBallState(BallState.Touched);
                }
            }
        }
        private void ballState_Released()
        {
            if (potentialForceReleased)
            {
                bool b1 = _rb.IsSleeping() || _rb.velocity == Vector3.zero;
                bool b2 = Vector3.Distance(startPosition, Position) > maxMovementDistance;
                if (b1 || b2)
                {
                    if (b1)
                    {
                        ballEvent = BallEvent.BallStopped;
                    }
                    else if (b2)
                    {
                        ballEvent = BallEvent.FarDistance;
                    }
                    potentialForceReleased = false;
                    setBallState(BallState.Finish);
                }
                else
                {
                    if (shadowEnable && isShadowInstantiateRateCompleted(Position))
                    {
                        createShadow(Position, transform.rotation);
                    }
                }
            }
            else
            {
                var force = getReleasedPotentialForce();
                if (force == Vector3.zero)
                {
                    potentialForceReleased = false;
                    setBallState(BallState.Idle);
                }
                else
                {
                    startPosition = Position;
                    AddForce(force);
                    PotentialEnergy = Zero;
                    potentialForceReleased = true;
                }
            }
        }
        private void ballState_Touched()
        {
            if (Input.GetMouseButton(0))
            {
                if (mousePositionOnPlane(out p2, maxForceDistance))
                {

                }
                else
                {
                    p2 = Position;
                }
            }
            else
            {
                if (secondTouch)
                {
                    setBallState(BallState.Released);
                    setSliderActive(false);
                    secondTouch = false;
                }
                else
                {
                    PotentialEnergy = Zero;
                    setBallState(BallState.Idle);
                }
            }
        }
        private void ballState_Finish()
        {
            //throw new System.Exception("Not handled.");
        }
        #endregion
        #region init & constructors
        private void init()
        {
            ballEvent = BallEvent.None;
            lastShadowPosition = startPosition = Position;
            setBallState(BallState.Idle);
            potentialForceReleased = false;
            PotentialEnergy = _shadowInstantiateRate = Zero;
            setSliderActive(false);
            if (slider != null)
            {
                _sliderRT = slider.GetComponent<RectTransform>();
            }
            if (_camera == null) _camera = Camera.main;
            if (_camera == null)
            {
                Debug.LogError("Camera not found.");
                Extensions.Quit();
            }
            if (_rb == null)
            {
                Debug.LogError("Ball Rigidbody not found.");
                Extensions.Quit();
            }
            if (_collider == null)
            {
                Debug.LogError("Ball Collider not found.");
                Extensions.Quit();
            }
            {
                var ik = _rb.isKinematic;
                _rb.isKinematic = true;
                _rb.velocity = Vector3.zero;
                _rb.isKinematic = ik;
            }
            if (maxForceDistance < minForceDistance)
            {
                Extensions.Swap<float>(ref maxForceDistance, ref minForceDistance);
            }
        }
        #endregion
        public void ResetBall()
        {
            init();
        }
        public void AutoCalculateMinForceDistance()
        {
            minForceDistance = (_collider.bounds.size.x + _collider.bounds.size.z) / 4;
        }
        #region logic
        private void setBallState(BallState state)
        {
            switch (ballState = state)
            {
                case BallState.Null:
                case BallState.Touched:
                case BallState.Idle:
                    _rb.useGravity = false;
                    break;
                case BallState.Released:
                    _rb.useGravity = true;
                    break;
            }
        }
        public void setBallIsInHole()
        {
            ballEvent = BallEvent.BallIsInHole;
            setBallState(BallState.Finish);
        }
        #region force
        private Vector3 getReleasedPotentialForce()
        {
            return potentialForceCoefficient * PotentialEnergy * PotentialForceDirection;
        }
        private void calculateForce(Vector3 point1, Vector3 point2, float minD)
        {
            var distance = Vector3.Distance(point2, point1);
            //calc PotentialEnergy 
            {
                var dis = distance - minD;
                if (dis < 0) dis = 0;
                PotentialEnergy = Mathf.Clamp(dis * potentialForceSensitivity, Zero, maxForceDistance);

            }
            setSliderValue(PotentialEnergy, maxForceDistance - minD);
            //var realForce = Mathf.Clamp(distance * potentialForceSensitivity, Zero, maxForceDistance);

            //calc PotentialEnergy Direction
            Vector3 potentialForceHitPosition;
            if (mousePositionOnPlane(out potentialForceHitPosition, PotentialEnergy))
            {
                PotentialForceDirection = (Position - potentialForceHitPosition).normalized;
                if (sliderCanvas != null && PotentialForceDirection != Vector3.zero)
                {
                    sliderCanvas.transform.rotation = Quaternion.LookRotation(PotentialForceDirection);
                }
            }
            else
            {
                PotentialForceDirection = Vector3.zero;
            }
        }
        public void AddForce(Vector3 force)
        {
            _rb.AddForce(force, forceMode);
        }
        #endregion
        #region physic
        private bool isHit()
        {
            Vector3 v;
            return isHit(out v);
        }
        private bool isHit(out Vector3 hitPosition)
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (_collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                hitPosition = hit.point;
                return true;
            }
            hitPosition = default;
            return false;
        }
        #endregion
        #region vectors
        public bool mousePositionOnPlane(out Vector3 hitPosition, float d)
        {
            Plane plane = new Plane(Vector3.up, Position);

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;
            if (plane.Raycast(ray, out enter))
            {
                //Get the point that is clicked
                var result = ray.GetPoint(enter);
                if (Vector3.Distance(result, Position) > d)
                {
                    result = (result - Position).normalized * d + Position;
                }
                hitPosition = result;
                return true;
            }
            hitPosition = default;
            return false;
        }
        public Vector3 getMouse2dPosition()
        {
            return _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _camera.transform.position.z));
        }
        #endregion
        #endregion
        #region UI
        private void setSliderActive(bool val)
        {
            if (sliderCanvas == null) return;
            sliderCanvas.SetActive(val);
        }
        private void setSliderPosition(Vector3 position)
        {
            if (sliderCanvas == null) return;
            sliderCanvas.transform.position = position;
        }
        private void setSliderValue(float val, float max, float min = 0)
        {
            if (slider == null) return;
            float result = 0f;
            {
                var delta = max - min;
                if (delta != 0)
                {
                    result = (val - min) / delta;
                }
            }
            slider.value = result;


            if (sliderFillAreaImage != null)
            {
                if (result > 0)
                {
                    sliderFillAreaImage.enabled = true;
                    sliderFillAreaImage.color = solidColor ? color1 : Color.Lerp(color1, color2, result);
                }
                else
                {
                    sliderFillAreaImage.enabled = false;
                }
            }
        }
        #endregion
        #region shadow
        private void createShadow(Vector3 position, Quaternion q)
        {
            if (shadowPrefab == null) return;
            var node = Object.Instantiate(shadowPrefab, position, q);
            node.transform.localScale = shadowScale;
            node.hideFlags = HideFlags.HideAndDontSave;
            Object.Destroy(node, shadowTTL);
        }
        private bool isShadowInstantiateRateCompleted(Vector3 position)
        {
            if (shadowMode == ShadowMode.ByTime)
            {
                if (_shadowInstantiateRate > shadowInstantiateRate)
                {
                    _shadowInstantiateRate = Zero;
                    return true;
                }
                _shadowInstantiateRate += Time.deltaTime;
            }
            else if (shadowMode == ShadowMode.ByDistance)
            {
                if (Vector3.Distance(lastShadowPosition, Position) > shadowInstantiateDistance)
                {
                    lastShadowPosition = Position;
                    return true;
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}