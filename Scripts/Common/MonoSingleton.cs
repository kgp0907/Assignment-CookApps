using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool isFound;
    private static bool createMissingInstance;

    static MonoSingleton()
    {
        isFound = false;
        _instance = null;
    }

    public MonoSingleton(bool createNewInstanceIfNeeded = true)
    {
        createMissingInstance = createNewInstanceIfNeeded;
    }

    public virtual void OnDestroy()
    {
        _instance = null;
    }

    public static T Instance
    {
        get
        {
            if (isFound && _instance)
            {
                return _instance;
            }
            else
            {
                UnityEngine.Object[] objects = GameObject.FindObjectsOfType(typeof(T));
                if (objects.Length > 0)
                {
                    if (objects.Length > 1)
                        Debug.LogWarning(objects.Length + " " + typeof(T).Name + "s were found! Make sure to have only one at a time!");
                    isFound = true;
                    _instance = (T)System.Convert.ChangeType(objects[0], typeof(T));
                    return _instance;
                }
                else
                {
                    if (createMissingInstance)
                    {
                        return _instance;
                    }
                    else
                    {
                        isFound = false;

                        Debug.LogError(typeof(T).Name + " was not created");

                        return null; // or default(T)
                    }
                }
            }
        }
    }
}