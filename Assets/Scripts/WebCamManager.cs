using OpenCVForUnity;
using OpenCVForUnityExample;
using UnityEngine;
using Text = UnityEngine.UI.Text;

[RequireComponent(typeof(WebCamTextureToMatHelper), typeof(FpsMonitor))]
public class WebCamManager : MonoBehaviour
{
    [SerializeField] Text _text;
    Texture2D _quadTex;
    WebCamTextureToMatHelper _texToMatHelper;
    FpsMonitor _fpsMonitor;
    InvisibleConverter _invCvtr;
    float _time = 5;
    
    void Start()
    {
        _invCvtr = new InvisibleConverter(_text);
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _fpsMonitor = GetComponent<FpsMonitor>();
        _texToMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _texToMatHelper.Initialize();
    }
    
    void Update()
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        if (_time > 0) {
            _time -= Time.deltaTime;
            return;
        }
        #endif
        if (!_texToMatHelper.IsPlaying() || !_texToMatHelper.DidUpdateThisFrame()) return;

        //背景を保存する
        if (!_invCvtr.HasSavedBgr) {
            _invCvtr.SaveBgr(_texToMatHelper.GetMat());
        }
        var webCamMat = _invCvtr.CvtToInvisible(_texToMatHelper.GetMat());
        Utils.fastMatToTexture2D(webCamMat, _quadTex);
    }
    
    public void OnWebCamTextureToMatHelperInitialized()
    {
        Debug.Log ("OnWebCamTextureToMatHelperInitialized");

        var webCamTextureMat = _texToMatHelper.GetMat();
        _quadTex = new Texture2D (webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = _quadTex;

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
    }

    public void OnWebCamTextureToMatHelperDisposed ()
    {
        Debug.Log ("OnWebCamTextureToMatHelperDisposed");
        if (_quadTex != null) {
            Destroy(_quadTex);
            _quadTex = null;
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