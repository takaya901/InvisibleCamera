using System.Collections;
using UnityEngine;
using OpenCVForUnity;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Imgcodecs;

public class ShutterHandler : MonoBehaviour
{
	AudioSource _shutter;
	
	void Start ()
	{
		_shutter = GetComponent<AudioSource>();
	}
	
	public void OnClick()
	{
		_shutter.Play();
		var webCamMat = WebCamManager.GetWebCamMat();
		var invisible = InvisibleProcessor.ConvertToInvisible(webCamMat);
		cvtColor(invisible, invisible, COLOR_BGR2RGB);
		imwrite("pic.png", invisible);
	}
	
	//スクリーンショットを保存
	//ReadPixelsはWaitForEndOfFrameのあとで実行しなければいけないのでコルーチンで実行 https://qiita.com/su10/items/a8f3f825155835de3d2a
//	IEnumerator CaptureScreenshot()
//	{
//		var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
//		yield return new WaitForEndOfFrame();
//
//		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
//		texture.Apply();
//		var bytes = texture.EncodeToPNG();
//		File.WriteAllBytes("pic.png", bytes);
//		Destroy(texture);
//	}
}
