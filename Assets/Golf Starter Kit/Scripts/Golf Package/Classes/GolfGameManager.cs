using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace MyApp.Golf
{
    public class GolfGameManager : MonoBehaviour
    {
        #region enum
        public enum State
        {
            Null = -1,
            None,
            GameOverByShoots,
            BallIsInHole
        }
        #endregion
        #region variable
        public bool infiniteShoot = true;
        [Min(1)] public int maxShoots = 2;
        #region events
        public UnityEvent gameOverByBallIsInHoleEvent;
        [Min(0)] public float gameOverByBallIsInHoleEventDelay = 0f;
        public UnityEvent gameOverByShootsIsOverEvent;
        [Min(0)] public float gameOverByShootsIsOverEventDelay = 0f;
        #endregion
        #region HideInInspector
        [HideInInspector] public GolfBall ball;
        [HideInInspector] public GolfHole targetHole;
        [HideInInspector] public int currentShoots = 0;
        public int PlayerShoots { get { return currentShoots; } }
        public bool GameIsOver { get; private set; }
        public State GameState { get; private set; }
        #region private
        private Vector3 firstBallPosition;
        private GolfBall.BallState lastBallState;
        #endregion
        #endregion
        #region editor 
#if UNITY_EDITOR
        [HideInInspector] public bool showParametersParts = true;
        [HideInInspector] public bool showGameEventsParts = true;
        [HideInInspector] public bool showInfoParts = true;
#endif
        #endregion
        #region private
        #endregion
        #endregion
        #region Functions
        private void Awake()
        {
            init();
        }
        private void Update()
        {
            if (GameIsOver) return;

            if (ball.ballState == GolfBall.BallState.Released)
            {
                if (lastBallState != GolfBall.BallState.Released)
                {
                    lastBallState = GolfBall.BallState.Released;
                    currentShoots++;
                }
            }
            else if (ball.ballState == GolfBall.BallState.Finish)
            {
                lastBallState = GolfBall.BallState.Finish;
                switch (ball.ballEvent)
                {
                    case GolfBall.BallEvent.BallIsInHole:
                        gameOver(State.BallIsInHole);
                        return;
                    case GolfBall.BallEvent.BallStopped:
                        if (!infiniteShoot)
                        {
                            if (currentShoots == maxShoots)
                            {
                                gameOver(State.GameOverByShoots);
                                return;
                            }
                        }
                        ball.ResetBall();
                        return;
                    case GolfBall.BallEvent.FarDistance:
                        ball.Position = firstBallPosition;
                        ball.ResetBall();
                        return;
                }
            }
        }
        #endregion
        #region functions
        #region init & constructors
        [RuntimeInitializeOnLoadMethod]
        public static void staticInit()
        {
            if (FindObjectOfType<GolfGameManager>() == null)
            {
                var node = new GameObject("_GolfGameManagerNode");
                node.transform.position = Vector3.zero;
                node.AddComponent<GolfGameManager>();
            }
        }
        public void GameManagerReset()
        {
            lastBallState = GolfBall.BallState.Null;
            currentShoots = 0; GameIsOver = false; GameState = State.None;
        }
        private void init()
        {
            ball = FindObjectOfType<GolfBall>();
            if (ball == null)
            {
                Debug.LogError("Ball not found.");
                Extensions.Quit();
                return;
            }
            firstBallPosition = ball.Position;
            GameManagerReset();
        }
        #endregion
        #region logic
        private void gameOver(State state)
        {
            GameState = state;
            GameIsOver = true;
            switch (state)
            {
                case State.BallIsInHole:
                    invokeEvent(gameOverByBallIsInHoleEvent, gameOverByBallIsInHoleEventDelay);
                    break;
                case State.GameOverByShoots:
                    invokeEvent(gameOverByShootsIsOverEvent, gameOverByShootsIsOverEventDelay);
                    break;
            }
        }
        #endregion
        #region Golf Hole
        public bool setTargetHole(GolfHole hole)
        {
            if (targetHole != null) return false;
            targetHole = hole;
            Debug.Log("ball in hole");
            return true;
        }
        #endregion
        #region delay
        public void invokeEvent(UnityEvent e, float delay)
        {
            if (e == null) return;
            if (delay > 0)
            {
                StartCoroutine(Coroutine(delay, e));
            }
            else
            {
                e.Invoke();
            }
        }
        private IEnumerator Coroutine(float seconds, UnityEvent e)
        {
            yield return new WaitForSeconds(seconds);
            if (e != null) e.Invoke();
        }
        #endregion
        #endregion
    }
}
