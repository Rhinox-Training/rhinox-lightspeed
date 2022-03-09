using UnityEngine;

namespace Rhinox.Lightspeed
{
    /// <summary>
    /// A struct used to keep track of something you only wish to execute every X ticks
    /// </summary>
    public struct TickDelay
    {
        private bool _returnedTrue; // For first time 
        private int _currentTick;

        public int DefaultDelay;

        public TickDelay(int defaultDelay = 1)
        {
            DefaultDelay = defaultDelay;
            _currentTick = 0;
            _returnedTrue = false;
        }

        /// <summary>
        /// Returns whether <DefaultDelay> ticks have passed since the last time this function returned true
        /// </summary>
        public bool Tick() => Tick(DefaultDelay);

        /// <summary>
        /// Returns whether <delay> ticks have passed since the last time this function returned true
        /// </summary>
        public bool Tick(int delay)
        {
            if (!_returnedTrue)
            {
                _returnedTrue = true;
                return true;
            }
            
            if (_currentTick <= delay)
            {
                ++_currentTick;
                return false;
            }

            Reset();
            return true;
        }
        
        public void Reset() => _currentTick = 0;
    }
    
    /// <summary>
    /// A struct used to keep track of something you only wish to execute every X amount of seconds
    /// </summary>
    public struct TimeDelay
    {
        private bool _returnedTrue; // For first time 
        private float _lastTime;
        
        public float DefaultDelay;

        public TimeDelay(float defaultDelay = .1f)
        {
            DefaultDelay = defaultDelay;
            _lastTime = 0;
            _returnedTrue = false;
        }

        /// <summary>
        /// Returns whether <DefaultDelay> seconds has passed since the last time this function returned true
        /// </summary>
        public bool Tick() => Tick(DefaultDelay);

        /// <summary>
        /// Returns whether <delay> seconds has passed since the last time this function returned true
        /// </summary>
        public bool Tick(float delay)
        {
            if (!_returnedTrue)
            {
                _returnedTrue = true;
                return true;
            }
            
            if (Time.time - _lastTime <= delay)
                return false;

            Reset();
            return true;
        }
        
        public void Reset() => _lastTime = Time.time;
    }
}