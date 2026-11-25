using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DistanceUnits
{
    None,
    Meters,
    Feet,
    Units
}

public class Waypoints : MonoBehaviour
{
    public Image image;
    public Transform target;
    public TextMeshProUGUI distance;
    public DistanceUnits distanceUnits;
    

    private void Update()
    {
        float minx = image.GetPixelAdjustedRect().width / 2;
        float maxx = Screen.width - minx;
        
        float miny = image.GetPixelAdjustedRect().height / 2;
        float maxy = Screen.height - miny;
        Vector2 pos = Camera.main.WorldToScreenPoint(target.position);

        if (Vector3.Dot(target.position - transform.position, transform.forward) < 0)
        {
            //behind
            if (pos.x < Screen.width / 2)
            {
                pos.x = maxx;
            }
            else
            {
                pos.x = minx;
            }
        }
        
        
        pos.x = Mathf.Clamp(pos.x, minx, maxx);
        pos.y = Mathf.Clamp(pos.y, miny, maxy);
        image.transform.position = pos;

        float distToTarget = Vector3.Distance(target.position, transform.position);
        if (distanceUnits == DistanceUnits.Feet)
        {
            distToTarget = distToTarget.MetersToFeet();
            distance.text = distToTarget.ToString("N0") + "ft";
        }
        else if (distanceUnits == DistanceUnits.Units)
        {
            distance.text = distToTarget.ToString("N0") + "u";
        }
        else if (distanceUnits == DistanceUnits.Meters)
        {
            if(distToTarget <= 1000)
                distance.text = distToTarget.ToString("N0") + "m";
            else
            {
                distToTarget = distToTarget.MetersToKilometers();
                distance.text = distToTarget.ToString("N0") + "km";
            }        
        }
        else
        {
            distance.text = distToTarget.ToString("N0");
        }
        
        
    }
}
