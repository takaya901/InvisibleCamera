using UnityEngine;

public class Preferences : MonoBehaviour
{
    [SerializeField] GameObject _mainCanvas;
    const string URL = "https://docs.google.com/document/d/1WyfIlz-2rUEvcTMleHY4mqOiOoNUPEkx8s7rUyGmpcE/edit?usp=sharing";

    public void OnPrivacyPolicySelected()
    {
        Application.OpenURL(URL);
    }

    public void OnBackSelected()
    {
        _mainCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
