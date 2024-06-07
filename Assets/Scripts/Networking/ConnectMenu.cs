using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class ConnectMenu : MonoBehaviour
    {
        public GameObject backgroundImage;
        
        public TMP_InputField usernameField;
        public TMP_InputField ipAddressField;
        public Button joinButton;
        public Button hostButton;
        
        public CustomNetworkManager networkManager;

        private void Start()
        {
            joinButton.onClick.AddListener(OnJoinButtonClicked);
            hostButton.onClick.AddListener(OnHostButtonClicked);
        }


        private void OnJoinButtonClicked()
        {
            var username = usernameField.text;
            var ipAddress = ipAddressField.text;
            
            networkManager.playerUsername = username;
            networkManager.networkAddress = ipAddress;
            networkManager.StartClient();
            
            backgroundImage.gameObject.SetActive(false);
        }
        
        private void OnHostButtonClicked()
        {
            var username = usernameField.text;
            var ipAddress = ipAddressField.text;
            
            networkManager.playerUsername = username;
            networkManager.networkAddress = ipAddress;
            networkManager.StartHost();
            
            backgroundImage.gameObject.SetActive(false);
        }
    }
}