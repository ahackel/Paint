using System.Collections.Generic;
using UnityEngine;

public class UndoHistory<T> where T : Object
{
    private List<T> _states = new List<T>();
    private int _position = -1;

    public bool CanUndo() => _position > 0;
    public bool CanRedo() => _position < _states.Count - 1;
    public int Capacity = 10;
    
    public void RecordState(T state)
    {
        if (_position == Capacity - 1)
        {
            _states.RemoveAt(0);
            _position--;
        }

        // remove all future steps
        if (_position > -1 && CanRedo())
        {
            _states.RemoveRange(_position + 1, _states.Count - _position - 1);
        }

        _position++;
        _states.Add(state);
    }

    public T Undo()
    {
        if (!CanUndo())
        {
            return null;
        }

        _position--;
        return _states[_position];
    }

    public T Redo()
    {
        if (!CanRedo())
        {
            return null;
        }

        _position++;
        return _states[_position];
    }

    public T GetCurrentState()
    {
        return _position > -1 && _position < _states.Count ? _states[_position] : null;
    }

    public void Clear()
    {
        _states.Clear();
        _position = -1;
    }
}