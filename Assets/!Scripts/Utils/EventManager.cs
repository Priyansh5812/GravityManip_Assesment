using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class EventManager
{
    public static ActionEvent OnGameStarted
    {
        get; private set;
    } = new();

    public static ActionEvent OnGameEnded
    {
        get; private set;
    } = new();

    public static ActionEvent OnCubeCollected
    {
        get; private set;
    } = new();



}

public class ActionEvent
{
    private event Action baseAction;

    public void Invoke() => baseAction?.Invoke();

    public void AddListener(Action action) => baseAction += action;

    public void RemoveListener(Action action) => baseAction -= action;
}
public class ActionEvent<T1>
{
    private event Action<T1> baseAction;

    public void Invoke(T1 value) => baseAction?.Invoke(value);

    public void AddListener(Action<T1> action) => baseAction += action;

    public void RemoveListener(Action<T1> action) => baseAction -= action;


}
public class ActionEvent<T1, T2>
{
    private event Action<T1, T2> baseAction;

    public void Invoke(T1 value_1 , T2 value_2) => baseAction?.Invoke(value_1 , value_2);
    public void AddListener(Action<T1, T2> action) => baseAction += action;

    public void RemoveListener(Action<T1, T2> action) => baseAction -= action;
}

public class FuncEvent<T1>
{
    private event Func<T1> baseFunc;

    public T1 Invoke() => baseFunc.Invoke();

    public void AddListener(Func<T1> action) => baseFunc += action;

    public void RemoveListener(Func<T1> action) => baseFunc -= action;

}

public class FuncEvent<T1 , T2>
{
    private event Func<T1 , T2> baseFunc;

    public T2 Invoke(T1 value) => baseFunc.Invoke(value);

    public void AddListener(Func<T1 , T2> action) => baseFunc += action;

    public void RemoveListener(Func<T1 , T2> action) => baseFunc -= action;

}

public class FuncEvent<T1, T2, T3>
{
    private event Func<T1, T2, T3> baseFunc;

    public T3 Invoke(T1 value_1, T2 value_2) => baseFunc.Invoke(value_1, value_2);

    public void AddListener(Func<T1, T2, T3> action) => baseFunc += action;

    public void RemoveListener(Func<T1, T2, T3> action) => baseFunc -= action;

}




