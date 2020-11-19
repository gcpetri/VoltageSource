using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace VoltageSource
{
    public class PhotonPlayerUI : MonoBehaviour, IOnEventCallback, IPunObservable
    {

        #region Private fields

        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TMP_Text pingCounter;

        private FpController _target;
        private float _maxHealth;
        private Renderer _targetRenderer;
        private CanvasGroup _canvasGroup;
        private Vector3 _targetPosition;
        #endregion
        
        [SerializeField] private GameObject missionUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject pauseUI;
        [SerializeField] private GameObject infoUI;
        [SerializeField] private GameObject timerUI;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] public GameObject miniMap;
        
        private bool _isPaused = false;
        private float _currentTime = 10f;
        private bool _playedOnce = false;

        public void SetTarget(FpController targetR)
        {
            if (targetR == null)
            {
                Debug.LogError("Player target doesn't exist");
                return;
            }
            this._target = targetR;
            if (playerNameText != null)
            {
                playerNameText.text = this._target.photonView.Owner.NickName;
            }
            _targetRenderer = this._target.GetComponent<Renderer>();
            _maxHealth = _target._maxHealth;
        }

        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);
            if(_target)
                _maxHealth = _target.Health;
            
            infoUI.SetActive(true);
            infoText.text = "Pre-round";
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_target == null)
            {
                Destroy(this.gameObject); 
                return;
            }
            
            if (playerHealthSlider != null)
            {
                float healthDiff = _maxHealth - _target.Health;
                playerHealthSlider.value = (_maxHealth - _target.Health) == 0
                    ? _maxHealth
                    : (_maxHealth - healthDiff) / 100f;
            }

            if (timerUI.activeSelf)
            {
                _currentTime -= Time.deltaTime;
                timerText.text = Mathf.Clamp((float)Math.Round(_currentTime, 2), 0, 100).ToString() + " secs";
            }

            miniMap.SetActive(Input.GetKey(KeyCode.Tab));
        }

        private void LateUpdate()
        {
            if (_targetRenderer != null)
            {
                _canvasGroup.alpha = _targetRenderer.isVisible ? 1f : 0f;
            }

            if (pingCounter != null)
            {
                pingCounter.text = "Ping: " + GameManager.Instance.GetPing().ToString();
            }
        }

        public void LeaveGame()
        {
            Debug.Log("Button pressed");
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(0); // Go back to main menu
            }
            else
            {
                SceneManager.LoadScene(0);
            }
            GameManager.Instance.LeaveRoom();
        }

        public bool TogglePause()
        {
            _isPaused = _isPaused != true;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Confined ? CursorLockMode.Locked : CursorLockMode.Confined;
            if (_isPaused)
            {
                pauseUI.SetActive(true);
            }
            else
            {
                pauseUI.SetActive(false);
            }

            return _isPaused;
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (GameManager.Instance.IsGameOver())
                                return;
            
            if (eventCode == (byte) EventManager.EventCodes.PlayerDied)
            {
                pauseUI.SetActive(false);
                gameUI.SetActive(false);
                infoUI.SetActive(true);
                timerUI.SetActive(true);
                _currentTime = GameManager.Instance.endRoundTimer;
                infoText.text = "Round Over";
            }
            else if (eventCode == (byte) EventManager.EventCodes.StartPreRound)
            {
                pauseUI.SetActive(false);
                gameUI.SetActive(true);
                infoUI.SetActive(true);
                timerUI.SetActive(true);
                if (_playedOnce)
                    missionUI.SetActive(true);
                _currentTime = GameManager.Instance.preRoundTimer;
                infoText.text = "Pre-Round";
                _playedOnce = true;
            }
            else if (eventCode == (byte) EventManager.EventCodes.StartRound)
            {
                pauseUI.SetActive(false);
                gameUI.SetActive(true);
                infoUI.SetActive(false);
                timerUI.SetActive(false);
                missionUI.SetActive(false);
                infoText.text = "";
            }
        }
        
        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext((float) _currentTime);
            }
            else
            {
                _currentTime = (float) stream.ReceiveNext();
            }
        }
    }

}
