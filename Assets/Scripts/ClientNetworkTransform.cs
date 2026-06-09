using Unity.Netcode.Components;
using UnityEngine;

namespace Farmhollow
{
    // Owner-autoritatives NetworkTransform: der Besitzer bewegt sich lokal,
    // seine Position wird zu den anderen Clients synchronisiert.
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
