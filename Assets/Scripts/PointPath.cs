using PathCreation;
using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class PointPath
{
    private Vector3[] mainPoints;
    private Vector3 lastClosePoint;

    private int numOfPoints;
    private int lastIndex = 0;
    private float currentIndexDistance = float.MaxValue;
    private float nextIndexDistance;
    private DateTime lastTime;

    public int NumOfPoints { get => numOfPoints;}
    public Vector3[] MainPoints { get => mainPoints;}
    public int CurrentIndex { get => lastIndex;}
    public Vector3 LastClosePoint { get => lastClosePoint;}

    public delegate void TestDelegate();

    public IEnumerator CreateWithDistance(VertexPath vertexPath, float distanceBetweenPoints, TestDelegate completedMethod,  bool exactLength = true)
    {
        int passer = 0;

        if (exactLength)
        {
            if(vertexPath.length % distanceBetweenPoints == 0) numOfPoints = (int)(vertexPath.length / distanceBetweenPoints) + 1;
            else numOfPoints = (int)(vertexPath.length / distanceBetweenPoints) + 2;
        }
        else numOfPoints = (int)(vertexPath.length / distanceBetweenPoints) + 1;

        float distanceUnit = exactLength ? distanceBetweenPoints : (vertexPath.length / (numOfPoints - 1));

        mainPoints = new Vector3[numOfPoints];
        mainPoints[0] = vertexPath.GetPoint(0);
        mainPoints[numOfPoints - 1] = vertexPath.GetPoint(vertexPath.NumPoints - 1);

        lastTime = DateTime.Now;
        for(int i = 1; i < numOfPoints - 1; i++)
        {
            mainPoints[i] = vertexPath.GetPointAtDistance(distanceUnit * i);
            if((DateTime.Now.Millisecond - lastTime.Millisecond) >= Time.deltaTime*1000)
            {
                lastTime = DateTime.Now;
                passer++;
                yield return null;
            }
        }
        completedMethod();
    }

    //public int ClosestPointInitial(Vector3 position)
    //{
    //    int i;
    //    currentIndexDistance = 7;
    //    for (i = lastIndex; i < numOfPoints - 1; i++)
    //    {
    //        nextIndexDistance = Vector3.Distance(position, mainPoints[i + 1]);
    //        if (nextIndexDistance > currentIndexDistance)
    //        {
    //            lastClosePoint = mainPoints[i];
    //            break;
    //        }
    //    }
    //    lastIndex = i;
    //    Debug.Log(lastIndex);
    //    return lastIndex;
    //}

    public int ClosestPointIndex(Vector3 position)
    {
        int i;
        currentIndexDistance = Vector3.Distance(position, mainPoints[lastIndex]);
        for (i = lastIndex; i < numOfPoints - 1; i++)
        {
            nextIndexDistance = Vector3.Distance(position, mainPoints[i + 1]);
            if (nextIndexDistance > currentIndexDistance)
            {
                lastClosePoint = mainPoints[i];
                break;
            }
            else currentIndexDistance = nextIndexDistance;
        }
        lastIndex = i;
        return lastIndex;
    }

    public void ResetPointInfos()
    {
        lastIndex = 0;
    }
}
