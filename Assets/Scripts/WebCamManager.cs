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
    ToMatHelperManager _toMatHelperManager;
    InvisibleConverter _invCvtr;
    float _time = 5;
    
    void Start()
    {
        Input.backButtonLeavesApp = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        _invCvtr = new InvisibleConverter(_text);
        
        _fpsMonitor = GetComponent<FpsMonitor>();
        _texToMatHelper = GetComponent<WebCamTextureToMatHelper>();
        _toMatHelperManager = new ToMatHelperManager(gameObject, _texToMatHelper, _fpsMonitor);
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

    public void OnWebCamTextureToMatHelperInitialized(){
        _toMatHelperManager.OnWebCamTextureToMatHelperInitialized(ref _quadTex);
    }

    public void OnWebCamTextureToMatHelperDisposed (){
        _toMatHelperManager.OnWebCamTextureToMatHelperDisposed(ref _quadTex);
    }
    
    public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
        _toMatHelperManager.OnWebCamTextureToMatHelperErrorOccurred(errorCode);
    }
}