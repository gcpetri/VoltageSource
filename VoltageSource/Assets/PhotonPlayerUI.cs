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

        private FP_Controller _target;
        private float _maxHealth;
        private Renderer _targetRenderer;
        private CanvasGroup _canvasGroup;
        private Vector3 _targetPosition;
        #endregion
        

        public void SetTarget(FP_Controller _target)
        {
            if (_target == null)
            {
                Debug.LogError("Player target doesn't exist");
                return;
            }

            this._target = _target;
            if (playerNameText != null)
            {
                playerNameText.text = this._target.photonView.Owner.NickName;
            }
            _targetRenderer = this._target.GetComponent<Renderer>();
            CharacterController characterController = this._target.GetComponent<CharacterController>();
            
        }

        // Start is called before the first frame update
        void Start()
        {
            _maxHealth = _target.Health;
            DontDestroyOnLoad(this);
        }

        private void Awake()
        {
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // Update is called once per frame
        void Update()
        {
            if(_target == null)
                Destroy(this);
            
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
        
    }

}
