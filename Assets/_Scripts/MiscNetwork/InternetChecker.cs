using UnityEngine;

public class InternetChecker : MonoBehaviour
{
	private const bool allowCarrierDataNetwork = true;
	private const string pingAddress = "8.8.8.8"; // Google Public DNS server
	private const float waitingTime = 2.0f;
	
	protected LoginIndex li;
	private Ping ping;
	private float pingStartTime;
	
	public void Start()
	{		
		li = GetComponentInChildren<LoginIndex>();
		bool internetPossiblyAvailable;
		switch (Application.internetReachability)
		{
		case NetworkReachability.ReachableViaLocalAreaNetwork:
			internetPossiblyAvailable = true;
			break;
		case NetworkReachability.ReachableViaCarrierDataNetwork:
			internetPossiblyAvailable = allowCarrierDataNetwork;
			break;
		default:
			internetPossiblyAvailable = false;
			break;
		}
		if (!internetPossiblyAvailable)
		{
			InternetIsNotAvailable();
			return;
		}
		ping = new Ping(pingAddress);
		pingStartTime = Time.time;
	}
	
	public void Update()
	{
		if (ping != null)
		{
			bool stopCheck = true;
			if (ping.isDone)
				InternetAvailable();
			else if (Time.time - pingStartTime < waitingTime)
				stopCheck = false;
			else
				InternetIsNotAvailable();
			if (stopCheck)
				ping = null;
		}
	}
	
	private void InternetIsNotAvailable()
	{
		UILabel labelWarning = li.errorMessage;
		li.ShowErrorMessage("no internet conection");
		li.CanLogin = false;
	}
	
	private void InternetAvailable()
	{
		li.CanLogin = true;
	}
}