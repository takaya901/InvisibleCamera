using OpenCVForUnity;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;
using Text = UnityEngine.UI.Text;

public class InvisibleConverter
{
    Mat _bg, _invisible = new Mat();    //背景
    Text _text; //for debug
    static readonly Scalar SKIN_LOWER = new Scalar(0, 30, 88);
    static readonly Scalar SKIN_UPPER = new Scalar(25, 173, 255);
    static readonly Scalar HAIR_LOWER = new Scalar(0, 0, 0);    
    static readonly Scalar HAIR_UPPER = new Scalar(255, 255, 90);

    /// <summary>背景を保存したかどうか</summary>
    public bool IsSavedBg { get; set; }

    public InvisibleConverter(Text text)
    {
        _text = text;
    }

    public void SaveBgr(Mat webcamMat)
    {
        _bg = webcamMat.clone();
        IsSavedBg = true;
    }
    
    /// <summary>肌と髪の領域を背景で置換する</summary>
    /// <param name="webcamMat">カメラからのRGBA画像</param>
    /// <returns>置換後のRGBA画像</returns>
    public Mat CvtToInvisible(Mat webcamMat)
    {       
        using (var withoutSkinAndHair = new Mat())
        using (var bgOnSkinAndHair = new Mat()) 
        using (var skinAndHairMask = new Mat()) 
        using (var hairMask = new Mat()) 
        using (var skinMask = new Mat()) 
        using (var skinMask2 = new Mat()) 
        using (var hsv = new Mat()) 
        using (var blurred = new Mat())
        {
            //肌と髪の領域と同じ位置の背景領域を抽出
            GaussianBlur(webcamMat, blurred, new Size(7, 7), 0);
            cvtColor(blurred, hsv, COLOR_RGBA2RGB);
            cvtColor(hsv, hsv, COLOR_RGB2HSV);
            inRange(hsv, SKIN_LOWER, SKIN_UPPER, skinMask);
            inRange(hsv, new Scalar(160, 30, 88), new Scalar(180, 173, 255), skinMask2);
            bitwise_or(skinMask, skinMask2, skinMask);
            inRange(hsv, HAIR_LOWER, HAIR_UPPER, hairMask);
        
            bitwise_or(skinMask, hairMask, skinAndHairMask);
            bitwise_and(_bg, _bg, bgOnSkinAndHair, skinAndHairMask);
        
            //カメラ映像から肌と髪以外の領域を抽出
            bitwise_not(skinAndHairMask, skinAndHairMask);
            bitwise_and(webcamMat, webcamMat, withoutSkinAndHair, skinAndHairMask);
        
            //カメラ映像の肌と髪の領域を背景で置換
            bitwise_or(withoutSkinAndHair, bgOnSkinAndHair, _invisible);
        }
            
        return _invisible;
    }

    public Size GetImgSize()
    {
        return _bg.size();
    }
}
