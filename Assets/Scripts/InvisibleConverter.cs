using OpenCVForUnity;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;
using Text = UnityEngine.UI.Text;

public class InvisibleConverter
{
    Mat _bgr;    //背景
    Text _text;
    static readonly Scalar SKIN_LOWER = new Scalar(0, 40, 60);
    static readonly Scalar SKIN_UPPER = new Scalar(20, 255, 255);
    static readonly Scalar HAIR_LOWER = new Scalar(0, 0, 0);    
    static readonly Scalar HAIR_UPPER = new Scalar(180, 120, 80);

    /// <summary>背景を保存したかどうか</summary>
    public bool HasSavedBgr { get; private set; }

    public InvisibleConverter(Text text)
    {
        _text = text;
    }

    /// <summary>背景を保存する</summary>
    /// <param name="webcamMat"></param>
    public void SaveBgr(Mat webcamMat)
    {
        _bgr = webcamMat.clone();
        int width = _bgr.width();
        int height = _bgr.height();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (_bgr.get(x, y)[0] != 0) {
                    HasSavedBgr = true;
                    x = width; y = height;
                }
            }
        }
    }
    
    /// <summary>肌と髪の領域を背景で置換する</summary>
    /// <param name="webcamMat">カメラからのRGBA画像</param>
    /// <returns>置換後のRGBA画像</returns>
    public Mat CvtToInvisible(Mat webcamMat)
    {
        //カメラ映像ををHSVに変換
        var blurred = new Mat();
        GaussianBlur(webcamMat, blurred, new Size(19, 19), 0);
        var hsv = new Mat();
        cvtColor(blurred, hsv, COLOR_RGBA2RGB);
        cvtColor(hsv, hsv, COLOR_RGB2HSV);
        
        //肌と髪の領域と同じ位置の背景領域を抽出
        var skinMask = new Mat();
        inRange(hsv, SKIN_LOWER, SKIN_UPPER, skinMask);
        morphologyEx(skinMask, skinMask, MORPH_OPEN, new Mat(), new Point(-1, -1), 3);
        
        var hairMask = new Mat();
        inRange(hsv, HAIR_LOWER, HAIR_UPPER, hairMask);
        morphologyEx(hairMask, hairMask, MORPH_OPEN, new Mat(), new Point(-1, -1), 3);
        
        var skinAndHairMask = new Mat();
        bitwise_or(skinMask, hairMask, skinAndHairMask);
        
        var bgrOnSkinAndHair = new Mat();
        bitwise_and(_bgr, _bgr, bgrOnSkinAndHair, skinAndHairMask);
        
        //肌と髪以外の領域を抽出
        bitwise_not(skinAndHairMask, skinAndHairMask);
        var withoutSkinAndHair = new Mat();
        bitwise_and(webcamMat, webcamMat, withoutSkinAndHair, skinAndHairMask);
        
        //カメラ映像の肌色領域を背景で置換
        var invisible = new Mat();
        bitwise_or(withoutSkinAndHair, bgrOnSkinAndHair, invisible);
        
        return invisible;
    }
}
