using UnityEngine;

public class PlayerBoostrap : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] boostrapbleSricpt;
    [SerializeField] private bool getFromParent;

    private void Awake()
    {
        if (getFromParent)
        {
            boostrapbleSricpt = GetComponentsInParent<MonoBehaviour>();
        }

        foreach (var mono in boostrapbleSricpt)
        {
            if (mono is IBoostrapble bootstrapble)
            {
                bootstrapble.BoostrapAwake();
            }
        }
    }
}
