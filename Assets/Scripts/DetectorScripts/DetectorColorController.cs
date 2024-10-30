using Leap.PhysicalHands;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

//this script is used to change the detector components colors
//Note: This script requires a PhysicalHandEvents script to be on the same gameobject. This script adds the PhysicalHandEvents as well as itself to the PhysicalHandEvents automatically
[RequireComponent(typeof(PhysicalHandEvents))]
public class DetectorColorController : MonoBehaviour
{
    public bool useStartColors = true; //uses the colors of the materials already on the object for the normal colors
    public List<Color> normalCol = new List<Color>();
    List<Color> currentObjectColors = new List<Color>(); //is just the normal color + some other color

    [Space]
    public Color defaultColor = Color.white;
    float highlightAmount; //goes from 0-1. Used to change between default color and normal color
    float highlightDelay = 0f; //highlight if this value is above 0
    
    [Space]
    public bool isHologram = true;
    public Color hologramCol;

    [Space]
    public GameObject MeshObj; //the Object whre the MeshRenderer is located

    [Space]
    public float changeSpeed = 3f; //speed at which the color switches

    Material[] objectMats;
    float changePercentage = 0f; //value used to lerp between normal and hologram color

    PhysicalHandEvents PHE;

    void Start()
    {
        //add itself to PhysicalHandEvent
        PHE = gameObject.GetComponent<PhysicalHandEvents>();
        if (PHE != null) 
        {
            //I couldn't find out how to pass a script with an interchangerble parameter into the PHE Events. For now, I'm using 3 different scripts
            PHE.onHover.AddListener(Highlight1);
            PHE.onContact.AddListener(Highlight2);
            PHE.onGrab.AddListener(Highlight3);
        }

        objectMats = MeshObj.GetComponent<MeshRenderer>().materials;
        
        for (int i = 0; i < objectMats.Length; i++)
        {
            normalCol.Add(objectMats[i].color);
        }

        currentObjectColors = new List<Color>(normalCol);
    }

    void Update()
    {
        //check if the object is highlighted
        highlightDelay -= Time.deltaTime;
        if (highlightDelay > 0f)
        {
            for (int i = 0; i < normalCol.Count; i++)
            {
                //change color depending on wether player is hovering, contacting or grabbing the object
                currentObjectColors[i] = highlightAmount * normalCol[i] + (1f - highlightAmount) * defaultColor;
            }
        }
        else 
        {
            //reset highlightAmount
            highlightAmount = 0.25f;

            for (int i = 0; i < normalCol.Count; i++)
            {
                currentObjectColors[i] = highlightAmount * normalCol[i] + (1f - highlightAmount) * defaultColor;
            }
        }

        //change color between hologram and non-hologram color
        if (isHologram)
        {
            changePercentage += changeSpeed * Time.deltaTime;
            changePercentage = Mathf.Clamp01(changePercentage);

            for (int i = 0; i < objectMats.Length; i++)
            {
                //change material from Opaque to Fade
                //Note: I found these settings here: https://discussions.unity.com/t/change-rendering-mode-via-script/667727/3 (21.10.2024)
                //https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/StandardShaderGUI.cs
                //I have no clue how they work
                objectMats[i].SetOverrideTag("RenderType", "Transparent");
                objectMats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                objectMats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                objectMats[i].SetInt("_ZWrite", 0);
                objectMats[i].DisableKeyword("_ALPHATEST_ON");
                objectMats[i].EnableKeyword("_ALPHABLEND_ON");
                objectMats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                objectMats[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                //change color
                objectMats[i].color = Color.Lerp(currentObjectColors[i], hologramCol, changePercentage);
            }
        }
        else
        {
            changePercentage -= changeSpeed * Time.deltaTime;
            changePercentage = Mathf.Clamp01(changePercentage);

            for (int i = 0; i < objectMats.Length; i++)
            {
                //change material from Fade to Opaque
                objectMats[i].SetOverrideTag("RenderType", "");
                objectMats[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                objectMats[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                objectMats[i].SetInt("_ZWrite", 1);
                objectMats[i].DisableKeyword("_ALPHATEST_ON");
                objectMats[i].DisableKeyword("_ALPHABLEND_ON");
                objectMats[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                objectMats[i].renderQueue = -1;

                //change color
                objectMats[i].color = Color.Lerp(currentObjectColors[i], hologramCol, changePercentage);
            }
        }
    }

    public void Highlight1(ContactHand hand) 
    {
        highlightDelay = 0.1f; //set the highlightdelay to a small number
        
        if(0.5f > highlightAmount)
            highlightAmount = 0.5f;
    }
    public void Highlight2(ContactHand hand)
    {
        highlightDelay = 0.1f; //set the highlightdelay to a small number

        if (0.75f > highlightAmount)
            highlightAmount = 0.75f;
    }
    public void Highlight3(ContactHand hand)
    {
        highlightDelay = 0.1f; //set the highlightdelay to a small number

        if (1f > highlightAmount)
            highlightAmount = 1f;
    }
}