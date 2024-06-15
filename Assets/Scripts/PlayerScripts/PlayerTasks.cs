using System.Collections.Generic;
using Mirror;
using Tasks;
using UnityEngine;

namespace PlayerScripts
{
    public class PlayerTasks : NetworkBehaviour
    {
        internal bool HasDownloadedData;
        internal readonly List<TaskInteractable> Tasks = new();
        
        public void AddTask(TaskInteractable task)
        {
            Tasks.Add(task);
        }
        
        public void RemoveTask(TaskInteractable task)
        {
            Tasks.Remove(task);
        }
    }
}