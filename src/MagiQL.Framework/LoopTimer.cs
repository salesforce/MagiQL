using System;
using MagiQL.Framework.Interfaces;

namespace MagiQL.Framework
{
    public class LoopTimer<T> : ILoopTimer
    {
        private DateTime _startDate; 
        private int _intervalMilliseconds;
        private Action<T> _action;
        private T _value;

        public LoopTimer(DateTime startDate, int intervalMilliseconds, Action<T> action, T value)
        {
            _startDate = startDate;
            _intervalMilliseconds = intervalMilliseconds;
            _action = action;
            _value = value;
        }

        public void Loop()
        {
            if (DateTime.Now.Subtract(_startDate).TotalMilliseconds > _intervalMilliseconds)
            {
                _startDate = DateTime.Now;
                _action.Invoke(_value);
            }
        }
    }
}