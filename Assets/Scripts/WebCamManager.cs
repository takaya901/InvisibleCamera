using System.Collections.Generic;
using OpenCVForUnity;
using OpenCVForUnityExample;
using UnityEngine;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;

[RequireComponent(typeof(WebCamTextureToMatHelper), typeof(FpsMonitor))]
public class WebCamManager : MonoBehaviour
{
    Texture2D _texture;
    static WebCamTextureToMatHelper _webCamTextureToMatHelper;
    FpsMonitor _fpsMonitor;
//    InvisibleProcessor _invisibleProcessor = new InvisibleProcessor();
    float time = 5;
    bool _hasSavedBackground;
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _fpsMonitor = GetComponent<FpsMonitor>();
        _webCamTextureToMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _webCamTextureToMatHelper.Initialize();
    }
    
    void Update()
    {
        if (time > 0) {
            time -= Time.deltaTime;
            return;
        }
        
        if (!_webCamTextureToMatHelper.IsPlaying() || !_webCamTextureToMatHelper.DidUpdateThisFrame()) return;

        if (!InvisibleProcessor.HasSavedBackground()) {
            InvisibleProcessor.SaveBackground(_webCamTextureToMatHelper.GetMat());
        }
//        var rgbaMat = _webCamTextureToMatHelper.GetMat();
        var rgbaMat = InvisibleProcessor.ConvertToInvisible(_webCamTextureToMatHelper.GetMat());
        Utils.fastMatToTexture2D(rgbaMat, _texture);
    }

    public static Mat GetWebCamMat()
    {
        return _webCamTextureToMatHelper.GetMat();
    }
    
    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log ("OnWebCamTextureToMatHelperInitialized");

        var webCamTextureMat = _webCamTextureToMatHelper.GetMat();
        _texture = new Texture2D (webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = _texture;

        Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        if (_fpsMonitor != null){
            _fpsMonitor.Add ("width", webCamTextureMat.width().ToString());
            _fpsMonitor.Add ("height", webCamTextureMat.height().ToString());
            _fpsMonitor.Add ("orientation", Screen.orientation.ToString());
        }
        
        float width = webCamTextureMat.width();
        float height = webCamTextureMat.height();
                                
        float widthScale = Screen.width / width;
        float heightScale = Screen.height / height;
        if (widthScale < heightScale) {
            Camera.main.orthographicSize = (width * Screen.height / Screen.width) / 2;
        } else {
            Camera.main.orthographicSize = height / 2;
        }
        
        //Quadを画面いっぱいにリサイズ
        ////https: //blog.narumium.net/2016/12/11/unityでスマホカメラを全面表示する/
        var quadHeight = Camera.main.orthographicSize * 2;
        var quadWidth = quadHeight * Camera.main.aspect;
        transform.localScale = new Vector3(quadWidth, quadHeight, 1);
        
//        _invisibleProcessor.SaveBackground(_webCamTextureToMatHelper.GetMat());
    }

    public void OnWebCamTextureToMatHelperDisposed ()
    {
        Debug.Log ("OnWebCamTextureToMatHelperDisposed");
        if (_texture != null) {
            Destroy(_texture);
            _texture = null;
        }
    }
    
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
        Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }
    
    void OnDestroy()
    {
        _webCamTextureToMatHelper.Dispose ();
    }
}