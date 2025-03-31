using TMPro;
using UnityEngine;

namespace TapEmpire.UI
{
    public class FPSMeterUIView : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI _displayText;

        [SerializeField]
        private float _updateInterval = 0.1f;

        private int _frames;
        private float _timePassed;
        private float _sessionTime;
        private int _sessionFrames;

        private float _currentFPS;
        private float _averageFPS;

        private void Update()
        {
            _frames++;
            _sessionFrames++;
            _timePassed += Time.deltaTime;
            _sessionTime += Time.deltaTime;

            _currentFPS = 1.0f / Time.deltaTime;

            if (_timePassed >= _updateInterval)
            {
                _averageFPS = _sessionFrames / _sessionTime;

                _timePassed = 0;
                _frames = 0;

                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (_displayText != null)
            {
                _displayText.text = string.Format(
                    "FPS: {0:F1}\nAvg FPS: {1:F1}",
                    _currentFPS, _averageFPS);
            }
        }
    }
}