using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TwoReferencePoint : MonoBehaviour
{
    public enum CalibInput{
        PC_Keyboard, 
        Oculus_Controller
    }
    [Header("Step1-1: Set the Transform Method. (Use Keyboard or Controller)")]
    public CalibInput calibInput;
    [Header("Step1-2: (Optional) Start Keyboard Key.")]
    public KeyCode StartCalibrationKey = KeyCode.Q;
    [Header("Step1-3: (Must) Put Real Ref Point truly on the controller object.")]
    public GameObject realRefObj;
    [Header("(Not Change) Virtual Ref Point on the unity controller.")]
    public GameObject virtualRefObj;

    [Header("Step2-1: (Optional) Set Calibration Position Number.")]
    public int calibPositionNumSet = 5; private int originCalibPositionNumSet = 0;
    private int positionCollect = 0;
    [Header("Step2-2: (Optional) Set Store Reference Process time.")]
    public float calibProcessTimeSet = 5.0f;
    private float calibClock = 0.0f;

    private List<Vector3> RealRefPointsList = new List<Vector3>();
    private List<Vector3> VirtualRefPointsList = new List<Vector3>();

    private bool isStartCalib = false;
    private bool triggerPressed = false;

    private CalculateTransformRT transformRT;
    private AutoSwtichToNewCoordinate autoSwitchCoordinate;

    public TMP_Text debugMessage;
    public Button calibrationButton;

    [Header("Step5: Active / InActive Calibration Process.")]
    [SerializeField] private bool isCalibration = false;

    [SerializeField] private List<GameObject> ShowCalibrationSphere = new List<GameObject>();

    
   

    private void Awake()
    {
        originCalibPositionNumSet = calibPositionNumSet;
        RealRefPointsList.Clear(); VirtualRefPointsList.Clear();
        transformRT = GetComponent<CalculateTransformRT>();
        autoSwitchCoordinate = GetComponent<AutoSwtichToNewCoordinate>();
        
    }

    private void Start()
    {
        ControlCalibrationTime();
    }

    private void Update()
    {
        if (!isCalibration) return;

        if(calibInput == CalibInput.PC_Keyboard) PC_CoordinateTransformProcess();
        else if(calibInput == CalibInput.Oculus_Controller) OculusController_CorrdinateTransformProcess();
    }

    public void ControlCalibrationTime()
    {
        isCalibration = !isCalibration;
        var background = calibrationButton.GetComponent<Image>();
        var textTmp = calibrationButton.transform.GetChild(0).GetComponent<TMP_Text>();


        if (isCalibration) { background.color = Color.red; textTmp.text = "ON - Calibration "; Debug.Log("=== Turn ON Calibration System ==="); }
        else { background.color = Color.white; textTmp.text = "OFF - Calibration"; foreach (var ele in ShowCalibrationSphere) ele.SetActive(false); Debug.Log("=== Turn OFF Calibration System ==="); }
    }

    private void PC_CoordinateTransformProcess()
    {
        if (Input.GetKeyDown(StartCalibrationKey)) {
            ShowCalibrationSphere[0].SetActive(false);
            Debug.Log("Press Key Q to calibration.");

            if (calibPositionNumSet < 1) {
                Debug.LogWarning("Reset the process time. Please press again to Calibration");
                calibPositionNumSet = originCalibPositionNumSet;
                return;
            }
            else
            {
                if (calibPositionNumSet == originCalibPositionNumSet) autoSwitchCoordinate.CancelParentRelationInCoordinate();
                StartCoroutine(GetTwoReferencePoints());
                isStartCalib = true;
            }

        }
        if (isStartCalib) CollectTwoReferencePointsPosition();
    }

    private void OculusController_CorrdinateTransformProcess()
    {
        if (OVRPlugin.userPresent)
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0 && !triggerPressed)
            {
                Debug.Log("Start Load Two Points Position");
                triggerPressed = true;
                if (positionCollect == 0) { autoSwitchCoordinate.CancelParentRelationInCoordinate();  RealRefPointsList.Clear(); VirtualRefPointsList.Clear(); }

                positionCollect += 1;
                calibClock = 0.0f;
                DebugUIMessage(debugMessage, "");
                ShowCalibrationSphere[0].SetActive(false);
            }
            else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0 && triggerPressed)
            {
                calibClock += Time.deltaTime;
                Debug.Log(calibClock);
                if (calibClock >= calibProcessTimeSet)
                { 
                    DebugUIMessage(debugMessage, "Complete! Remain process Times: " + (calibPositionNumSet - positionCollect).ToString());
                    ShowCalibrationSphere[0].SetActive(true);
                }
                
                CollectTwoReferencePointsPosition();
            }
            else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) == 0 && triggerPressed)
            {
                Debug.Log("Collect Process " + positionCollect.ToString());
                if (positionCollect == calibPositionNumSet)
                {
                    DebugUIMessage(debugMessage, "Calculate Transform Coordinate...");
                    CalculateCoordinateTransform();

                    positionCollect = 0;
                }
                triggerPressed = false;
            }

        }else Debug.LogWarning("Not found!");
    }


    private IEnumerator GetTwoReferencePoints()
    {
        
        float downTime = calibProcessTimeSet;
        while(downTime > 0)
        {
            downTime--;
            DebugUIMessage(debugMessage, ("Time Remain: " + (downTime + 1).ToString()));
            yield return new WaitForSeconds(1);
        }
        calibPositionNumSet--;
        DebugUIMessage(debugMessage, "Complete! Remain process Times: " + calibPositionNumSet);
        ShowCalibrationSphere[0].SetActive(true);

        isStartCalib = false;

        if (calibPositionNumSet < 1)
            CalculateCoordinateTransform();
        
    }


    private void CollectTwoReferencePointsPosition()
    {
        RealRefPointsList.Add(realRefObj.transform.position);
        VirtualRefPointsList.Add(virtualRefObj.transform.position);
    }

    private void CalculateCoordinateTransform()
    {
        transformRT.CalculateHomography(RealRefPointsList.ToArray(), VirtualRefPointsList.ToArray());
        DebugUIMessage(debugMessage, "");
        Debug.Log("=== Complete All Calibration Process ===");

        ShowCalibrationSphere[0].SetActive(false);
        ShowCalibrationSphere[1].SetActive(true);

        ControlCalibrationTime();               // if complete all calibration process, auto inactive the calibration system
    }


    private void DebugUIMessage(TMP_Text tmpText, string _message)
    {
        tmpText.SetText(_message);
    }

}
