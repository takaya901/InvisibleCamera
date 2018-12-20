using OpenCVForUnity;
using UnityEngine;
using static OpenCVForUnity.Imgproc;
using static OpenCVForUnity.Core;

public static class InvisibleProcessor
{
    static Mat _background;
    static double _screenArea;    //ノイズ除去に使用
    static readonly Scalar SKIN_LOWER = new Scalar(0, 40, 60);
    static readonly Scalar SKIN_UPPER = new Scalar(20, 255, 255);
    static readonly Scalar HAIR_LOWER = new Scalar(0, 0, 0);    
    static readonly Scalar HAIR_UPPER = new Scalar(180, 120, 80);

    static InvisibleProcessor()
    {
        _screenArea = Screen.width * Screen.height;
    }

    public static void SaveBackground(Mat webcamMat)
    {
        _background = webcamMat.clone();
    }

    /// <summary>背景を保存したかどうか</summary>
    public static bool HasSavedBackground()
    {
        return _background != null;
    }   
    
    /// <summary>肌と髪の領域を背景で置換する</summary>
    /// <param name="webcamMat">カメラからのRGBA画像</param>
    /// <returns>置換後のRGBA画像</returns>
    public static Mat ConvertToInvisible(Mat webcamMat)
    {
        //カメラ映像ををHSVに変換
        var blured = new Mat();
        GaussianBlur(webcamMat, blured, new Size(19, 19), 0);
        var hsv = new Mat();
        cvtColor(blured, hsv, COLOR_RGBA2RGB);
        cvtColor(hsv, hsv, COLOR_RGB2HSV);
        
        //肌と髪の領域と同じ位置の背景領域を抽出
        var skinMask = new Mat();
        inRange(hsv, SKIN_LOWER, SKIN_UPPER, skinMask);
        morphologyEx(skinMask, skinMask, MORPH_OPEN, new Mat(), new Point(-1, -1), 10);
        
        var hairMask = new Mat();
        inRange(hsv, HAIR_LOWER, HAIR_UPPER, hairMask);
        morphologyEx(hairMask, hairMask, MORPH_OPEN, new Mat(), new Point(-1, -1), 15);
        
        var skinAndHairMask = new Mat();
        bitwise_or(skinMask, hairMask, skinAndHairMask);
        
        var bgrOnSkinAndHair = new Mat();
        bitwise_and(_background, _background, bgrOnSkinAndHair, skinAndHairMask);
        
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
