using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// EventManager
// Centralized static event hub used by the project to broadcast simple game events.
// It exposes strongly-typed event wrapper instances (ActionEvent / FuncEvent)
// which allow subscribing/unsubscribing and safe invocation.
public static class EventManager
{
    // Invoked when the game starts. Subscribers should register a method with no args.
    public static ActionEvent OnGameStarted
    {
        get; private set;
    } = new();

    // Invoked when the game ends. Subscribers should register a method with no args.
    public static ActionEvent OnGameEnded
    {
        get; private set;
    } = new();

    // Invoked when a collectible cube is collected.
    public static ActionEvent OnCubeCollected
    {
        get; private set;
    } = new();
}

// Simple wrapper around an Action event. Provides AddListener/RemoveListener/Invoke
// to keep usage consistent across the codebase and to avoid direct event exposure.
public class ActionEvent
{
    private event Action baseAction;

    public void Invoke() => baseAction?.Invoke();

    public void AddListener(Action action) => baseAction += action;

    public void RemoveListener(Action action) => baseAction -= action;
}

// Generic Action event for one parameter.
public class ActionEvent<T1>
{
    private event Action<T1> baseAction;

    public void Invoke(T1 value) => baseAction?.Invoke(value);

    public void AddListener(Action<T1> action) => baseAction += action;

    public void RemoveListener(Action<T1> action) => baseAction -= action;
}

// Generic Action event for two parameters.
public class ActionEvent<T1, T2>
{
    private event Action<T1, T2> baseAction;

    public void Invoke(T1 value_1 , T2 value_2) => baseAction?.Invoke(value_1 , value_2);
    public void AddListener(Action<T1, T2> action) => baseAction += action;

    public void RemoveListener(Action<T1, T2> action) => baseAction -= action;
}

// Generic Func event with a return type (no parameters).
public class FuncEvent<T1>
{
    private event Func<T1> baseFunc;

    public T1 Invoke() => baseFunc.Invoke();

    public void AddListener(Func<T1> action) => baseFunc += action;

    public void RemoveListener(Func<T1> action) => baseFunc -= action;
}

// Generic Func event taking one parameter and returning a value.
public class FuncEvent<T1 , T2>
{
    private event Func<T1 , T2> baseFunc;

    public T2 Invoke(T1 value) => baseFunc.Invoke(value);

    public void AddListener(Func<T1 , T2> action) => baseFunc += action;

    public void RemoveListener(Func<T1 , T2> action) => baseFunc -= action;
}

// Generic Func event taking two parameters and returning a value.
public class FuncEvent<T1, T2, T3>
{
    private event Func<T1, T2, T3> baseFunc;

    public T3 Invoke(T1 value_1, T2 value_2) => baseFunc.Invoke(value_1, value_2);

    public void AddListener(Func<T1, T2, T3> action) => baseFunc += action;

    public void RemoveListener(Func<T1, T2, T3> action) => baseFunc -= action;
}




