using System.Collections.Generic;
using UnityEngine;


public class TextureUndoHistory : UndoHistory<Texture2D>
{
    private TexturePool _pool;
    
    public TextureUndoHistory(int capacity, int width, int height)
    {
        Capacity = capacity;
        _pool = new TexturePool(capacity, width, height);
    }

    protected override void DeleteState(Texture2D state)
    {
        _pool.Return(state);
    }

    protected override Texture2D CreateState()
    {
        return _pool.Get();
    }
}


public abstract class UndoHistory<T> where T : Object
{
    private List<T> _states = new List<T>();
    private int _position = -1;

    public bool CanUndo() => _position > 0;
    public bool CanRedo() => _position < _states.Count - 1;
    public int Capacity = 10;

    protected abstract void DeleteState(T state);

    protected abstract T CreateState();
    
    public T RecordState()
    {
        if (_position == Capacity - 1)
        {
            DeleteState(_states[0]);
            _states.RemoveAt(0);
            _position--;
        }

        // remove all future steps
        if (_position > -1 && CanRedo())
        {
            for (int i = _states.Count - 1; i > _position; i--)
            {
                DeleteState(_states[i]);
                _states.RemoveAt(i);
            }
        }

        _position++;
        var state = CreateState();
        _states.Add(state);
        return state;
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
        foreach (var state in _states)
        {
            DeleteState(state);
        }
        _states.Clear();
        _position = -1;
    }
}