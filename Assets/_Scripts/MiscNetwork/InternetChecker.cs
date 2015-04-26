using UnityEngine;

public class InternetChecker : MonoBehaviour
{
	private const bool allowCarrierDataNetwork = true;
	private const string pingAddress = "8.8.8.8"; // Google Public DNS server
	private const float waitingTime = 2.0f;
	
	protected LoginIndex li;
	private Ping ping;
	private float pingStartTime;
	private bool checkingConection = true;
	
	public void OnEnable()
	{		
		li = GetComponent<LoginIndex>();

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
			if (ping.isDone)
			{
				ping = null;
				InternetAvailable();
			}

			else if (Time.time - pingStartTime < waitingTime) 
			{
				if (!li.hasInternet)	CheckingConection();
			}

			else
			{
				InternetIsNotAvailable();
				ping = new Ping(pingAddress);
				pingStartTime = Time.time;
			}

		}
	
	}
	
	private void InternetIsNotAvailable()
	{
		UILabel labelWarning = li.errorMessage;
		li.ShowErrorMessage("no internet conection");
		li.hasInternet = false;
	}

	private void CheckingConection()
	{
		UILabel labelWarning = li.errorMessage;
		li.ShowErrorMessage("checking conection");
		li.hasInternet = false;
	}
	
	private void InternetAvailable()
	{
		li.hasInternet = true;
		enabled = false;
	}
}