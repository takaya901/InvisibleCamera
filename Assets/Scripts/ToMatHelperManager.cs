using OpenCVForUnityExample;
using UnityEngine;

/// <summary>
/// WebCamTextureToMatHelperの初期化・破棄を行う．
/// WebCamManagerに書くと長いので分けた
/// </summary>
public class ToMatHelperManager
{
    GameObject _quad;
    WebCamTextureToMatHelper _texToMatHelper;
    FpsMonitor _fpsMonitor;

    public ToMatHelperManager(GameObject quad, WebCamTextureToMatHelper texToMatHelper, FpsMonitor fpsMonitor)
    {
        _quad = quad;
        _texToMatHelper = texToMatHelper;
        _fpsMonitor = fpsMonitor;
    }
    
    public void OnWebCamTextureToMatHelperInitialized(ref Texture2D quadTex)
    {
        Debug.Log ("OnWebCamTextureToMatHelperInitialized");

        var webCamTextureMat = _texToMatHelper.GetMat();
        quadTex = new Texture2D (webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        _quad.GetComponent<Renderer>().material.mainTexture = quadTex;

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
        //https: //blog.narumium.net/2016/12/11/unityでスマホカメラを全面表示する/
        var quadHeight = Camera.main.orthographicSize * 2;
        var quadWidth = quadHeight * Camera.main.aspect;
        _quad.transform.localScale = new Vector3(quadWidth, quadHeight, 1);
    }

    public void OnWebCamTextureToMatHelperDisposed(ref Texture2D quadTex)
    {
        Debug.Log ("OnWebCamTextureToMatHelperDisposed");
        if (quadTex != null) {
            Object.Destroy(quadTex);
            quadTex = null;
        }
    }
    
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
        Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
    }
    
    void OnDestroy()
    {
        _texToMatHelper.Dispose ();
    }
}
