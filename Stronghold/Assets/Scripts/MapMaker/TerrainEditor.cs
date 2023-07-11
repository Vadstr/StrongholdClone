using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;

public class TerrainEditor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Range(0, 0.001f)]
    [SerializeField]  
    private float Sensivity;
    [Range(1, 15)]
    [SerializeField]  
    private int brushSize = 10;
    [SerializeField]  
    private Terrain map;
    [SerializeField] 
    private bool isCircle;
    
    private delegate void ActionOnMap();
    private event ActionOnMap ActionOnMapEvent;
    private float[,] heights;
    private float _sensivity;
    private Vector3 previousMousePosition;
    
    private Nullable<bool> isDown = null;
    private Nullable<bool> isMax = null;
    
    
    private void Awake()
    {
        map = this.GetComponent<Terrain>();
    }

    private void FixedUpdate()
    {
        if(IsMouseMoove() && ActionOnMapEvent != null)
            ActionOnMapEvent.Invoke();
    }

    private bool IsMouseMoove()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != previousMousePosition)
        {
            previousMousePosition = currentMousePosition;
            Debug.Log("true");
            return true;
        }
        else
        {
            previousMousePosition = currentMousePosition;
            Debug.Log("false");
            return false;
        }
    }

    private void UpdateHeights()
    {
        int xres = map.terrainData.alphamapWidth;
        int yres = map.terrainData.alphamapHeight;

        heights = map.terrainData.GetHeights(0,0, xres, yres);

        Vector3 terrainPosition = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == map.gameObject)
            {

                Vector3 samplePosition = hit.point / 2; // Позиція, для якої отримуємо координати

                // Отримуємо координати на терені, пов'язані з висотою
                terrainPosition = new Vector3(samplePosition.x, 0, samplePosition.z);
            }
        }

        Debug.Log(heights[(int)Math.Round(terrainPosition.z), (int)Math.Round(terrainPosition.x)]);
        
        //if (heights[(int)Math.Round(terrainPosition.z) / 2, (int)Math.Round(terrainPosition.x) / 2] + Sensivity >= 0.03f)
            //heights[(int)Math.Round(terrainPosition.z) / 2, (int)Math.Round(terrainPosition.x) / 2] = 0.03f;
        //else
            //heights[(int)Math.Round(terrainPosition.z)/2, (int)Math.Round(terrainPosition.x)/2] += Sensivity;
        
        ComputeHeightValues(terrainPosition);
        
        map.terrainData.SetHeights(0,0, heights);
    }
    
    private void ComputeHeightValues(Vector3 terrainPosition)
    {
        for (int i = Math.Max((int)Math.Round(terrainPosition.x - brushSize),2); i < Math.Min(terrainPosition.x + brushSize, heights.GetLength(0) - 3); i++)
        {
            for (int j = Math.Max((int)Math.Round(terrainPosition.z - brushSize),2); j < Math.Min(terrainPosition.z + brushSize, heights.GetLength(1) - 3); j++)
            {
                if(isCircle && Math.Pow(i - terrainPosition.x, 2) + Math.Pow(j - terrainPosition.z , 2) > Math.Pow(brushSize, 2))
                    continue;

                if (isMax != null)
                {
                    if (isMax.Value)
                        heights[j, i] = 0.03f;
                    else
                        heights[j, i] = 0.00f;
                    
                    continue;
                }

                if (isDown != null)
                {
                    if (heights[j, i] + _sensivity >= 0.03f)
                        heights[j, i] = 0.03f;
                    else if (heights[j, i] + _sensivity <= 0.00f)
                        heights[j, i] = 0.00f;
                    else
                        heights[j, i] += _sensivity;
                }
            }
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(eventData.button);
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ActionOnMapEvent += UpdateHeights;
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(eventData.button);
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ActionOnMapEvent -= UpdateHeights;
        }
    }

    public void UpOrDown(bool isDown)
    {
        this.isDown = isDown;
        isMax = null;

        _sensivity = isDown ? -Sensivity : Sensivity;
    }
    
    public void MinMax(bool isMax)
    {
        isDown = null;
        this.isMax = isMax;
    }
    
    public void CircluSquare(bool isCircle)
    {
        this.isCircle = isCircle;
    }

    public void UpdateRadius(bool makeBigger)
    {
        brushSize = makeBigger ? Math.Min(brushSize + 1, 15) : Math.Max(brushSize - 1, 1);
    }

    public void UpdateSensivity(bool makeBigger)
    {
        Sensivity = makeBigger ? Math.Min(Sensivity + 0.0001f, 0.001f) : Math.Max(Sensivity - 0.0001f, 0.0001f);

        if (isDown != null)
        {
            UpOrDown(isDown.Value);
        }
    }
}
