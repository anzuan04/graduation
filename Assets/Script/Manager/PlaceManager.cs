using System.Collections.Generic;
using UnityEngine;

public class PlaceManager : MonoBehaviour
{
    public static PlaceManager Instance;
    public List<Dominate> allPlaces = new List<Dominate>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlace(Dominate place)
    {
        allPlaces.Add(place);
    }

    public void UnregisterPlace(Dominate place)
    {
        allPlaces.Remove(place);
    }

    public Vector3[] GetAllPositions()
    {
        Vector3[] positions = new Vector3[allPlaces.Count];
        for (int i = 0; i < allPlaces.Count; i++)
        {
            positions[i] = allPlaces[i].transform.position;
        }
        return positions;
    }
}