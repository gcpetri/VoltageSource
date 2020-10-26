using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VoltageSource
{
    public class PhotonPlayerUI : MonoBehaviour
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

        [SerializeField] private GameObject GameUI;
        [SerializeField] private GameObject PauseUI;
        
        private bool isPaused = false;

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
            _maxHealth = _target.Health;
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
                playerHealthSlider.value = (_maxHealth - _target.Health) == 0
                    ? _maxHealth
                    : (_maxHealth - _target.Health) / 10f;
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
        
    }

}
