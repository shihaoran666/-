using System;
using UnityEngine;

namespace FoglightIsles
{
    public sealed class Interactable : MonoBehaviour
    {
        public string Title;
        public string Prompt;
        public string CompleteMessage;
        public bool IsComplete { get; private set; }
        public Action<PlayerController> OnInteracted;

        public void Interact(PlayerController player)
        {
            IsComplete = true;
            if (OnInteracted != null)
            {
                OnInteracted.Invoke(player);
                return;
            }
            PrototypeHUD.SetMessage(CompleteMessage);
        }
    }
}
