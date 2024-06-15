using System.Collections;
using Mirror;
using PlayerScripts;
using Tasks.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tasks
{
    public class Interactable : NetworkBehaviour
    {
        public string interactableName;
        public string interactableDescription;
        
        public virtual void Use(Player player)
        {
            Debug.Log("Default use method called, interactable name: " + interactableName);
        }

        public virtual IEnumerator Complete()
        {
            Debug.Log("Default complete method called, interactable name: " + interactableName);
            yield return null;
        }
    }
}