using TMPro;
using UnityEngine;

namespace Player
{
    public class PlayerUI : MonoBehaviour
    {
        public TMP_Text speedText;
        public TMP_Text staminaText;
        public TMP_Text tiredText;
        public TMP_Text jumpText;
    
        void Start()
        {
            var textComponents = GetComponents<TMP_Text>();
            Debug.Log("Text Components: " + textComponents.Length);
            
            if (textComponents.Length < 4)
            {
                return;
            }
            
            speedText = textComponents[0];
            staminaText = textComponents[1];
            tiredText = textComponents[2];
            jumpText = textComponents[3];
        }
    
        public void UpdateSpeedText(float speed)
        {
            speedText.text = "Speed: " + speed;
        }
    
        public void UpdateStaminaText(float stamina)
        {
            staminaText.text = "Stamina: " + (int)stamina;
        }
    
        public void UpdateTiredText(bool tired)
        {
            tiredText.text = tired ? "Tired" : "Not Tired";
        }
    
        public void UpdateJumpText(bool canJump)
        {
            jumpText.text = canJump ? "Can Jump" : "Can't Jump";
        }
    }
}