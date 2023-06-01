using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeoTagDisplay : MonoBehaviour
{
    public Text title;

    public void Initialize(GeoTag geotag)
    {
        this.title.text = geotag.name;
    }
}