using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPSLocation : MonoBehaviour
{
    public Text GPSStatus;
    public Text latitudeValue;
    public Text longitudeValue;
    public Text altitudeValue;

    public GameObject savedPrefab;
    public CanvasGroup createGeoTagCanvas;
    public GameObject InputObject;
    public InputField geoTagName;

    // Local Position variables
    private Vector3 location;
    [SerializeField] private GameObject ARCamera;
    private List<GeoTag> GeoTags = new List<GeoTag>();

    // Distance variables
    private float r, lat1, lat2, deltaLat, deltaLong, long1, long2;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GPSLoc());
    }

    IEnumerator GPSLoc()
    {
        // Check if location service is active
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start location service
        Input.location.Start();

        // Wait until service starts
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't start
        if (maxWait < 1)
        {
            GPSStatus.text = "Timed out";
            yield break;
        }

        // Connection fails
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            GPSStatus.text = "Unable to Connect";
            yield break;
        }
        else
        {
            // Access granted
            GPSStatus.text = "Running";
            InvokeRepeating("UpdateGPSData", 0.5f, 1f);
        }
    }

    private void UpdateGPSData()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            // Access granted to GPS and it is on
            GPSStatus.text = "Running";
            latitudeValue.text = Input.location.lastData.latitude.ToString();
            longitudeValue.text = Input.location.lastData.longitude.ToString();
            altitudeValue.text = Input.location.lastData.altitude.ToString();
        }
        else
        {
            // Service is stopped
            GPSStatus.text = "Stopped";
        }
    }

    public void SavedLocation()
    {
        InputObject.SetActive(true);
        // Create geotag
        GeoTag geotag = new GeoTag();
        geotag.latitude = Input.location.lastData.latitude;
        geotag.longitude = Input.location.lastData.longitude;
        geotag.altitude = Input.location.lastData.altitude;
        geotag.name = geoTagName.text;

        // Set Variables for GetDistance()
        lat2 = geotag.latitude;
        long2 = geotag.longitude;

        // Add to GeoTags List
        GeoTags.Add(geotag);

        // Display saved location
        latitudeValue.text = "Latitude: " + geotag.latitude.ToString();
        longitudeValue.text = "Longitude: " + geotag.longitude.ToString();
        altitudeValue.text = "Altitude: " + geotag.altitude.ToString();
        location = GPSEncoder.GPSToUCS(geotag.latitude, geotag.longitude);
        Debug.Log("Unity Local Position: " + location.ToString());

        SpawnPrefab(Camera.main.transform.position, geotag);
    }

    public void EndInput()
    {
        InputObject.SetActive(false);
    }

    private void SpawnPrefab(Vector3 position, GeoTag geotag)
    {
        GameObject instantiatedObj = Instantiate(savedPrefab, position + (Camera.main.transform.forward * 1), Quaternion.identity);
        instantiatedObj.transform.LookAt(Camera.main.transform);
        instantiatedObj.GetComponent<GeoTagDisplay>().Initialize(geotag);
    }

    public void GetDistance()
    {
        // Radius of earth in kilometers. Use 3956 for miles
        r = 6371;
        // Convert to radians
        lat1 = lat2 * Mathf.PI / 180;
        lat2 = lat2 * Mathf.PI / 180;

        // Haversine Formula
        deltaLat = (lat2 - lat1) * Mathf.PI / 180;
        deltaLong = (long2 - long1) * Mathf.PI / 180;
        float a = Mathf.Sin(deltaLat / 2) * Mathf.Sin(deltaLat / 2) + Mathf.Cos(lat1) * Mathf.Cos(lat2) * Mathf.Sin(deltaLong / 2) * Mathf.Sin(deltaLong / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        float d = r * c;
        Debug.Log("Distance: " + d.ToString() + " KM");
    }
}
