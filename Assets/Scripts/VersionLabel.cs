using UnityEngine;
using UnityEngine.UI;

namespace Farmhollow
{
    // Setzt einen Text zur Laufzeit auf die echte Spielversion (statt fest reingeschrieben).
    [RequireComponent(typeof(Text))]
    public class VersionLabel : MonoBehaviour
    {
        public string prefix = "Version ";

        void Start()
        {
            var t = GetComponent<Text>();
            if (t != null) t.text = prefix + Application.version;
        }
    }
}
