using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSwtichToNewCoordinate : MonoBehaviour
{
    [Header("(Not Change) Follow Oculus Coordinate empty obj.")]
    public GameObject CalibCoordinateTransform;
    [Header("Step3-1: (Must) Put Your all Trackers in the List.")]
    public GameObject[] TrackerList;

    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Z)) SwtichToNewCoordinate();
        // if(Input.GetKeyDown(KeyCode.X)) CancelParentRelationInCoordinate();
    }

    public void SwtichToNewCoordinate()
    {
        foreach(var traker in TrackerList) traker.transform.SetParent(CalibCoordinateTransform.transform);
    }

    public void CancelParentRelationInCoordinate()
    {
        foreach(var traker in TrackerList) traker.transform.SetParent(CalibCoordinateTransform.transform.parent);
    }

}
