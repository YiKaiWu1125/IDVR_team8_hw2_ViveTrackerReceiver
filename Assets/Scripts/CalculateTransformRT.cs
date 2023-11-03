using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class CalculateTransformRT : MonoBehaviour
{
    DenseMatrix Rt;
    AutoSwtichToNewCoordinate autoSwitchCoordinate;
    [Header("(Not Change) Follow Oculus Coordinate empty obj")]
    public GameObject followerObj;
    private bool isFind = false;

    private void Awake()
    {
        isFind = false;
        autoSwitchCoordinate = GetComponent<AutoSwtichToNewCoordinate>();
    }


    public void CalculateHomography(Vector3[] sourcePoints, Vector3[] destinationPoints)
    {
        Rt = FindRT2(sourcePoints, destinationPoints);

        CreateFollowerObject(Rt);

        isFind = true;

    }

    private DenseMatrix FindRT2(Vector3[] sourcePoints, Vector3[] destinationPoints)
    {
        var denseMat = DenseMatrix.Create(3, 4, 0);
        var sMat = DenseMatrix.Create(sourcePoints.Length, 3, 0);
        var dMat = DenseMatrix.Create(destinationPoints.Length, 3, 0);
        var sCenter = CreateVector.Dense<double>(new double[3]);
        var dCenter = CreateVector.Dense<double>(new double[3]);


        // create the matrix
        SetParameters(sourcePoints, ref sMat, ref sCenter);
        SetParameters(destinationPoints, ref dMat, ref dCenter);

        // 
        var hsvd = sMat.Transpose().Multiply(dMat).Transpose().Svd();
        var R = hsvd.VT.Transpose().Multiply(hsvd.U.Transpose()).Transpose();

        if(R.Determinant() < 0)
        {
            var rsvd = R.Svd();
            var rVT = rsvd.VT;
            rVT.SetRow(2, rVT.Row(2).Multiply(-1));
            R = rVT.Transpose().Multiply(rsvd.U.Transpose()).Transpose();
        }

        denseMat.SetSubMatrix(0, 0, R);
        denseMat.SetColumn(3, 0, 3, dCenter - R.Multiply(sCenter));

        return denseMat;
    }

    private void SetParameters(Vector3[] vecs, ref DenseMatrix mat, ref Vector<double> vec)
    {
        for (int i = 0; i < vecs.Length; i++)
            mat.SetRow(i, 0, 3, CreateVector.Dense<double>(new double[] { vecs[i].x, vecs[i].y, vecs[i].z }));

        for(int i = 0; i < 3; i++)
        {
            vec[i] = mat.Column(i).Sum() / vecs.Length;
            mat.SetColumn(i, mat.Column(i).Subtract(vec[i]));
        }
    }

    private void CreateFollowerObject(DenseMatrix rt)
    {
        Debug.Log(rt);
        followerObj.transform.localRotation = Quaternion.Euler(
            Mathf.Atan2(-(float)rt[1, 2], Mathf.Sqrt(1 - Mathf.Pow((float)rt[1, 2], 2))) * Mathf.Rad2Deg,
            Mathf.Atan2((float)rt[0, 2], (float)rt[2, 2]) * Mathf.Rad2Deg,
            Mathf.Atan2((float)rt[1, 0], (float)rt[1, 1]) * Mathf.Rad2Deg);
        followerObj.transform.localPosition = new Vector3((float)rt[0, 3], (float)rt[1, 3], (float)rt[2, 3]);

        // auto set trakers to the new coordinate
        autoSwitchCoordinate.SwtichToNewCoordinate();
    }
    
}
