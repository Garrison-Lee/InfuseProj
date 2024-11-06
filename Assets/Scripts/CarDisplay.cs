using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CarDisplay
{
    public int ID;
    public string Name;
    public int Year;
    public string ImageURL;
    public string ThumbURL;
    public string Price;
    public string Location;
    public float Latitude;
    public float Longitude;
    public string ModelURL;

    public Texture displayTexture;

    public CarDisplay(CarEntry car)
    {
        ID = car.id;
        Name = car.name;
        Year = car.year;
        ImageURL = car.imageURL;
        ThumbURL = car.thumbURL;
        Price = car.price;
        Location = car.location;
        Latitude = car.latitude;
        Longitude = car.longitude;
        ModelURL = car.modelURL;
    }
}
