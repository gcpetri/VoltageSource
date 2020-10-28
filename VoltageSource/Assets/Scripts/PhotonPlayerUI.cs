using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VoltageSource
{
    public class PhotonPlayerUI : MonoBehaviour, IOnEventCallback
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
        [SerializeField] private GameObject MissionUI;
        [SerializeField] private GameObject GameUI;
        [SerializeField] private GameObject PauseUI;
        [SerializeField] private GameObject InfoUI;
        [SerializeField] private GameObject TimerUI;
        [SerializeField] private TMP_Text InfoText;
        [SerializeField] private TMP_Text TimerText;
        
        private bool isPaused = false;
        private float currentTime = 10f;

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
        }

        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);
            _maxHealth = _target.Health;
            InfoUI.SetActive(true);
            InfoText.text = "Pre-round";
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

            if (TimerUI.activeSelf)
            {
                currentTime -= Time.deltaTime;
                TimerText.text = Math.Round(currentTime, 2).ToString() + " secs";
            }
            
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
            GameManager.Instance.LeaveRoom();
        }

        public bool TogglePause()
        {
            isPaused = isPaused != true;
            if (isPaused)
            {
                GameUI.SetActive(false);
                PauseUI.SetActive(true);
            }
            else
            {
                GameUI.SetActive(true);
                PauseUI.SetActive(false);
            }

            return isPaused;
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == (byte) EventManager.EventCodes.PlayerDied)
            {
                PauseUI.SetActive(false);
                GameUI.SetActive(false);
                InfoUI.SetActive(true);
                TimerUI.SetActive(true);
                currentTime = GameManager.Instance.endRoundTimer;
                InfoText.text = "Round Over";
            }
            else if (eventCode == (byte) EventManager.EventCodes.StartPreRound)
            {
                PauseUI.SetActive(false);
                GameUI.SetActive(true);
                InfoUI.SetActive(true);
                TimerUI.SetActive(true);
                MissionUI.SetActive(true);
                currentTime = GameManager.Instance.preRoundTimer;
                InfoText.text = "Pre-Round";
            }
            else if (eventCode == (byte) EventManager.EventCodes.StartRound)
            {
                PauseUI.SetActive(false);
                GameUI.SetActive(true);
                InfoUI.SetActive(false);
                TimerUI.SetActive(false);
                MissionUI.SetActive(false);
                InfoText.text = "";
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
        
    }

}
