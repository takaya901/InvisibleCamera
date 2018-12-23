using UnityEngine;
using GoogleMobileAds.Api;

public class AdMob : MonoBehaviour 
{
	void Start()
	{
		// アプリID、 これはテスト用
		string appId = "ca-app-pub-3940256099942544~3347511713";

		// Initialize the Google Mobile Ads SDK.
		MobileAds.Initialize(appId);

		RequestBanner();
	}

	void RequestBanner()
	{
		// 広告ユニットID これはテスト用
		string adUnitId = "ca-app-pub-3940256099942544/6300978111"; 

		// Create a 320x50 banner at the top of the screen.
		var bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

		// Create an empty ad request.
		var request = new AdRequest.Builder().Build();

		// Load the banner with the request.
		bannerView.LoadAd(request);

		// Create a 320x50 banner at the top of the screen.
		//bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
	}
}