using UnityEngine;
using Photon.Pun;

namespace TimeCrax.Managers
{
    public class TurnManager : MonoBehaviourPunCallbacks
    {
        private static TurnManager _instance;
        public static TurnManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindFirstObjectByType<TurnManager>();
                return _instance;
            }
        }

        private GameManager gameManager;

        // Propriedades delegando ao GameManager (fonte única de verdade)
        public int CurrentRound => gameManager != null ? gameManager.CurrentRound : 0;
        public int CurrentTime  => gameManager != null ? gameManager.CurrentTime  : 0;
        public PlayerScript[] OrderedPlayers => gameManager?.OrderedPlayers;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        public PlayerScript GetCurrentTurnPlayer()
        {
            var ordered = OrderedPlayers;
            int t = CurrentTime;
            if (ordered == null || t < 0 || t >= ordered.Length) return null;
            return ordered[t];
        }

        public int GetLastPlayerIndex()
        {
            var ordered = OrderedPlayers;
            if (ordered == null) return 3;
            for (int i = ordered.Length - 1; i >= 0; i--)
            {
                if (ordered[i] != null)
                    return ordered[i].index;
            }
            return 3;
        }

        public bool IsLastTurnOfRound()
        {
            return CurrentTime == GetLastPlayerIndex();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
