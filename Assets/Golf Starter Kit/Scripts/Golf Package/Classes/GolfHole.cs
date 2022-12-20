using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyApp.Golf
{
    [RequireComponent(typeof(SphereCollider))]
    public class GolfHole : MonoBehaviour
    {
        #region variable
        public bool enable = true;
        public bool gravityEnable = true;

        [SerializeField, Min(0)] public float gravityForce = 50f;
        [Min(0)] public float safeRadius = .2f;
        [Min(0)] public float maxContactTime = 2f;
        [Range(0, 1)] public float convergenceCoefficient = .05f;
        public float vortexStrength = 100f;


        #region HideInInspector
        [HideInInspector] public float vortexRadius = 1f;
        [HideInInspector] public float vortexIntensity;
        [HideInInspector] public SphereCollider _sphereCollider;
        [HideInInspector] public float ContactTime;
        [HideInInspector] public GolfGameManager golfGameManager;
        [HideInInspector] public Vector3 Position { get { return _sphereCollider.transform.position; } }

        private float radius;
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showInfoParts = true;
#endif
        #endregion
        #endregion
        #endregion
        #region Functions
        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_sphereCollider.transform.position, safeRadius);
        }
        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(Position, Vector3.up, _sphereCollider.radius);
        }
#endif
        #endregion
        private void OnValidate()
        {
            _sphereCollider = GetComponent<SphereCollider>();
        }
        private void Start()
        {
            golfGameManager = FindObjectOfType<GolfGameManager>();
            if (golfGameManager == null)
            {
                Debug.LogError("Golf GameManager not found.");
                enable = false;
                Extensions.Quit();
                return;
            }
            _sphereCollider.isTrigger = true;
            radius = _sphereCollider.radius == 0 ? 1 : _sphereCollider.radius;
        }
        #region Trigger
        void OnTriggerEnter(Collider other)
        {
            if (!enable ) return;
            GolfBall ball = checkForResetContactTime(other);
            if (ball != null && gravityEnable)
            {
                ball._rb.useGravity = false;
            }
            vortexIntensity = 0;
        }
        private void OnTriggerExit(Collider other)
        {
            if (!enable) return;
            GolfBall ball = checkForResetContactTime(other);
            if (ball != null && gravityEnable)
            {
                ball._rb.useGravity = true;
            }
            vortexIntensity = 0;
        }
        private void OnTriggerStay(Collider other)
        {
            if (!enable) return;
            if (other == _sphereCollider) return;
            if (other.gameObject == null) return;
            var ball = other.gameObject.GetComponent<GolfBall>();
            if (ball == null) return;
            if (Vector3.Distance(Position, ball.Position) < safeRadius)
            {
                ContactTime += Time.deltaTime;
                if (ContactTime > maxContactTime)
                {
                    if (gravityEnable) ball._rb.velocity = Vector3.zero;
                    ball.setBallIsInHole();
                    golfGameManager.setTargetHole(this);
                    return;
                }
            }
            if (gravityEnable)
            {
                Vector3 ballHoleVector = (Position - ball.Position);
                var ballHoleDistance = Vector3.Distance(Position, ball.Position);
                addGravityForce(ball, ballHoleVector, ballHoleDistance);
                addCirculationForce(ball, ballHoleVector, ballHoleDistance);
            }
        }
        #endregion
        #endregion
        #region functions
        private GolfBall checkForResetContactTime(Collider other)
        {
            if (other == _sphereCollider) return null;
            GolfBall ball;
            if (other.gameObject == null || (ball = other.gameObject.GetComponent<GolfBall>()) == null) return null;
            ContactTime = 0;
            return ball;
        }
        private void addGravityForce(GolfBall ball, Vector3 ballHoleVector, float ballHoleDistance)
        {
            if (ball == null) return;
            var direction = ballHoleVector.normalized;

            var forceValue = (ballHoleDistance / radius) // Gravity Intensity
                * ball._rb.mass
                * gravityForce
                * (1 + ContactTime)
                * Time.smoothDeltaTime;
            var force = direction * forceValue;

            ball.transform.position += ballHoleDistance * convergenceCoefficient * direction;

            ball.AddForce(force);
            Debug.DrawRay(ball.Position, ballHoleVector);
            //var force = (Position - ball.Position) * gravityForce ;
            //if (force.magnitude < safeDistance) f *= ball._rb.velocity.magnitude;
        }
        private void addCirculationForce(GolfBall ball, Vector3 ballHoleVector, float ballHoleDistance)
        {
            if (ball == null) return;
            vortexIntensity = (vortexStrength * ((radius - ballHoleDistance) / radius));
            var force = ballHoleVector.normalized
                * Time.smoothDeltaTime
                * vortexIntensity
                * ball._rb.mass
                * (1 + ContactTime);
            ball.AddForce(force);

            Debug.DrawRay(ball.Position, force, Color.red);
        }
        #endregion
    }
}