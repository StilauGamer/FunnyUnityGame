using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Player = PlayerScripts.Player;
using Random = System.Random;

namespace Tasks
{
    public class UnlockManifoldsTask : TaskInteractable
    {
        public GameObject taskUI;
        public Color selectedColor;
        
        private bool _isComplete;
        private Player _activePlayer;
        private readonly List<int> _lastClicked = new();
        private readonly Dictionary<int, int> _correctOrder = new();

        public void Awake()
        {
            RandomizeOrder();
        }
        
        public override void Use(Player player)
        {
            if (isTaskCompleted)
            {
                return;
            }
            
            _activePlayer = player;
            
            _activePlayer.playerCam.SetCanTurn(false);
            _activePlayer.playerCam.ToggleInput(true);
            _activePlayer.playerUI.SendUIEffect(10, taskUI);
            for (var i = 1; i < 11; i++)
            {
                var buttonId = i;
                player.playerUI.SetupButtonListener(10, $"Button{buttonId}", () => OnButtonClick(buttonId));
            }

            UpdateButtonText();
        }
        
        public override IEnumerator Complete()
        {
            isTaskCompleted = true;
            Debug.Log("Task completed!");

            yield return new WaitForSeconds(2f);

            _activePlayer.playerUI.ClearUIEffect(10);
            _activePlayer.playerCam.SetCanTurn(true);
            _activePlayer.playerCam.ToggleInput(false);
        }

        private void OnButtonClick(int buttonId)
        {
            var clickedButtonValue = _correctOrder[buttonId];
            if (_lastClicked.Count >= _correctOrder.Count || clickedButtonValue != _lastClicked.Count + 1)
            {
                _activePlayer.StartCoroutine(ResetGame());
                return;
            }

            _lastClicked.Add(clickedButtonValue);
            _activePlayer.playerUI.SendUIEffectImageColor<Image>(10, "Button" + buttonId, selectedColor);

            if (_lastClicked.Count == _correctOrder.Count)
            {
                _activePlayer.StartCoroutine(Complete());
            }
        }

        private IEnumerator ResetGame()
        {
            RandomizeOrder();
            UpdateButtonText();
            _lastClicked.Clear();
            
            foreach (var button in _correctOrder)
            {
                _activePlayer.playerUI.SendUIEffectImageColor<Image>(10, "Button" + button.Key, Color.white);
                yield return null;
            }
        }
        
        private void RandomizeOrder()
        {
            var random = new Random();
            for (var i = 1; i < 11; i++)
            {
                var randomNum = random.Next(1, 11);
                if (_correctOrder.ContainsValue(randomNum))
                {
                    i--;
                    continue;
                }
                
                _correctOrder.Add(i, randomNum);
            }
        }

        private void UpdateButtonText()
        {
            foreach (var orderButton in _correctOrder)
            {
                _activePlayer.playerUI.SendUIEffectText(10, $"Button{orderButton.Key}:Text", GetNumberText(orderButton.Value));
            }
        }

        private string GetNumberText(int number)
        {
            return number switch
            {
                1 => "One",
                2 => "Two",
                3 => "Three",
                4 => "Four",
                5 => "Five",
                6 => "Six",
                7 => "Seven",
                8 => "Eight",
                9 => "Nine",
                10 => "Ten",
                _ => "Unknown"
            };
        }
    }
}