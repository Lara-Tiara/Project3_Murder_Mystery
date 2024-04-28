using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour {
    public static T Instance { get; private set; }

    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }
}
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour {
    protected override void Awake() {
        if (Instance != null) Destroy(gameObject);
        else base.Awake();
    }
}

public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour {
    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
public abstract class StaticInstanceCallback<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks {
    public static T Instance { get; private set; }

    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit() {
        Instance = null;
        Destroy(gameObject);
    }
}

public abstract class SingletonCallback<T> : StaticInstanceCallback<T> where T : MonoBehaviourPunCallbacks {
    protected override void Awake() {
        if (Instance != null) Destroy(gameObject);
        else base.Awake();
    }
}

public abstract class PersistentSingletonCallback<T> : SingletonCallback<T> where T : MonoBehaviourPunCallbacks {
    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}

