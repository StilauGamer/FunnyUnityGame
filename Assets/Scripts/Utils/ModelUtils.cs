using UnityEngine;

namespace Utils
{
    public class ModelUtils
    {
        public static GameObject GetModel(GameObject gameObjectInternal, string nameInternal)
        {
            if (gameObjectInternal.name == nameInternal)
            {
                return gameObjectInternal;
            }
                
            foreach (Transform child in gameObjectInternal.transform)
            {
                var model = GetModel(child.gameObject, nameInternal);
                if (model)
                {
                    return model;
                }
            }
                
            return null;
        }
    }
}