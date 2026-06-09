using UnityEngine;

namespace Farmhollow
{
    // Verwaltet das Geld des Spielers (in Dollar).
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance;
        public int money = 1000;          // Startkapital
        public System.Action OnChanged;   // UI hört darauf

        void Awake() { Instance = this; }

        public bool CanAfford(int amount) { return money >= amount; }

        public bool TrySpend(int amount)
        {
            if (money < amount) return false;
            money -= amount;
            if (OnChanged != null) OnChanged();
            return true;
        }

        public void Earn(int amount)
        {
            money += amount;
            if (OnChanged != null) OnChanged();
        }
    }
}
