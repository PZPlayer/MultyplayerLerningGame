using UnityEngine;

namespace Thanks.Player.Inputs
{
    public class PCInput : MonoBehaviour
    {
        private IKeyCaller caller;

        private void Awake()
        {
            caller = GetComponent<IKeyCaller>();
        }


    }
}
