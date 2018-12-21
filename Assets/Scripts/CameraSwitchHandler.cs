using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitchHandler : MonoBehaviour 
{
	void Start () 
	{
		
	}

	public void OnClick()
	{
		
		SceneManager.LoadScene(0);
	}
}