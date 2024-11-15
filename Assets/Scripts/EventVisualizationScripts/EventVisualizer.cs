using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ParticleData
{
    public string particleName;

    public List<Vector3> points = new List<Vector3>();
    public List<float> times = new List<float>();

    public float minTime, maxTime;

    public void DetermineTimeRange()
    {
        //the times should already be sourted by the time this function gets called
        minTime = times[0];
        maxTime = times[times.Count - 1];
    }
}

[Serializable]
public class ParticleType
{
    public string particleName;
    public Texture particleTexture;
    public Gradient trailColor;
    [HideInInspector] public ParticleSystem ps;
    [HideInInspector] public ParticleSystem.Particle[] psParticles;
}

//Note: This script visualizes the particle events in local space
public enum VisualizationState { INACTIVE, LOADING, VISUALIZING }
public class EventVisualizer : MonoBehaviour
{
    [Header("General Settings")]
    public VisualizationState state;
    [SerializeField] List <VisualizationState> nextStates;
    [SerializeField] bool canSwitchState = true;
    [Space]
    public int overlayLayer = 31;
    [Space]
    public float playbackSpeed = 0f;
    public float currentTime;
    float eventStartTime = Mathf.Infinity;
    float eventEndTime = -Mathf.Infinity;

    [Space]
    public float scale = 0.01f;

    [Header("Appearance by Particle Type")]
    public ParticleType[] particleTypes;

    [Header("Particle Visualization")]
    public GameObject particleSystemTemplate; //A GameObject with a ParticleSystem that emit no particles, uses local simulation space and hierachy scaling mode
    public Material particleMat; //a Particles/Standard Unlit material set to Cutout. Texture will be applied at runtime
    [Space]
    public Texture errorTexture;
    ParticleSystem errorPS;
    ParticleSystem.Particle[] errorParticles;
    List<int> allTrackIDS = new List<int>();

    [Header("Trail Visualization")]
    public float trailLifeTime = 4f;
    public float trailWidth = 0.1f;
    public AnimationCurve trailShape;
    public Material trailMat;
    [Space]
    public Gradient errorTrailColor;
    Dictionary<int, LineRenderer> particleTrails = new Dictionary<int, LineRenderer>();

    [Header("Path Visualization")]
    public bool generatePaths;
    public Gradient lineColor;
    public float lineWidth = 1f;
    public Material lineMat;
    List<LineRenderer> pathRenderers = new List<LineRenderer>();

    [Header("File Management")]
    public string fileName;
    public List<string> extractedData = new List<string>();
    Dictionary<int, ParticleData> eventData = new Dictionary<int, ParticleData>();

    [Header("Testing")]
    public int testTrackID;
    public ParticleData testParticle;

    private void Start()
    {
        nextStates = new List<VisualizationState>();
    }

    private void Update()
    {
        //TESTING, can be removed later
        if (eventData.ContainsKey(testTrackID))
        {
            testParticle = eventData[testTrackID];
        }

        //switch states when all operations in a state is fully complete. Switching immediately can result in weird graphics errors, such as lire renders being left behind
        if (nextStates.Count > 0) 
        {
            if (canSwitchState)
            {
                state = nextStates[0];
                nextStates.RemoveAt(0);
                canSwitchState = false;
            }
        }

        if (state == VisualizationState.INACTIVE) 
        {
            ResetEventVisualization();
            canSwitchState = true;
        }
        else if (state == VisualizationState.LOADING) 
        {
            canSwitchState = false;
            //create a dictionary for the visualization
            eventStartTime = Mathf.Infinity;
            eventEndTime = -Mathf.Infinity;

            CreateParticleDictionary();
            CreateTrailDictionary();
            
            canSwitchState = true;
            nextStates.Add(VisualizationState.VISUALIZING);
        }
        else if (state == VisualizationState.VISUALIZING)
        {
            canSwitchState = true;
            //visualize event
            LoopTime();
            VisualizeEvent();
            VisualizePaths();
        }
    }

    void CreateParticleDictionary()
    {
        float s = Time.realtimeSinceStartup; //Value for debugging

        //generate a list with all the lines of a file
        TextAsset eventFile = (TextAsset)Resources.Load("Belle2ParticleEvents/" + fileName);
        List<string> fileLines = new List<string>(eventFile.text.Split('\n'));

        int line = 1;  //skip 1st line

        //create a dictionary of the particles in a event. Use Track ID as key for the dictionary
        eventData = new Dictionary<int, ParticleData>();
        allTrackIDS.Clear();
        while (line < fileLines.Count - 1)
        {
            //extract data from line
            extractedData.Clear();

            for (int i = 0; i < fileLines[line].Length; i++)
            {
                string temp = "";
                while (fileLines[line][i] != ',')
                {
                    temp += fileLines[line][i];
                    i += 1;

                    if (i == fileLines[line].Length)
                        break;
                }
                extractedData.Add(temp);
            }

            //check if particle already has an entry in the dictionary
            int trackID = int.Parse(extractedData[0]);
            if (eventData.ContainsKey(trackID))
            {
                //change existing particleData entry
                ParticleData pd = eventData[trackID];

                Vector3 preP = new Vector3(
                    float.Parse(extractedData[4]),
                    float.Parse(extractedData[5]),
                    float.Parse(extractedData[6]));
                float preT = float.Parse(extractedData[7]);

                Vector3 postP = new Vector3(
                    float.Parse(extractedData[8]),
                    float.Parse(extractedData[9]),
                    float.Parse(extractedData[10]));
                float postT = float.Parse(extractedData[11]);

                //check for duplicates (It's probably not necessary to check both time and position)
                if (pd.times.Contains(preT)) //check time
                {
                    //get index of duplicate time and check the corresponding point
                    Vector3 pos = pd.points[pd.times.IndexOf(preT)];
                    if (pos != preP)
                    {
                        //add to list since it's not a duplicate
                        pd.points.Add(preP); //add prePoints
                        pd.times.Add(preT); //add prePointT
                    }
                }
                else
                {
                    pd.points.Add(preP); //add prePoints
                    pd.times.Add(preT); //add prePointT
                }

                if (pd.times.Contains(postT)) //check time
                {
                    //get index of duplicate time and check the corresponding point
                    Vector3 pos = pd.points[pd.times.IndexOf(postT)];
                    if (pos != postP)
                    {
                        //add to list since it's not a duplicate
                        Debug.Log("Wasn't a duplicate");
                        pd.points.Add(postP); //add postPoints
                        pd.times.Add(postT); //add postPointT
                    }
                }
                else
                {
                    pd.points.Add(postP); //add postPoints
                    pd.times.Add(postT); //add postPointT
                }

                eventData[trackID] = pd;

                //update start and end time of the event
                if (preT < eventStartTime)
                    eventStartTime = preT;
                if (postT > eventEndTime)
                    eventEndTime = postT;

                currentTime = eventStartTime;
            }
            else
            {
                //create new entry
                allTrackIDS.Add(trackID);
                ParticleData pd = new ParticleData();

                pd.particleName = extractedData[2];

                Vector3 preP = new Vector3(
                    float.Parse(extractedData[4]),
                    float.Parse(extractedData[5]),
                    float.Parse(extractedData[6]));
                float preT = float.Parse(extractedData[7]);

                Vector3 postP = new Vector3(
                    float.Parse(extractedData[8]),
                    float.Parse(extractedData[9]),
                    float.Parse(extractedData[10]));
                float postT = float.Parse(extractedData[11]);

                pd.points.Add(preP); //add prePoints
                pd.points.Add(postP); //add postPoints
                pd.times.Add(preT); //add prePointT
                pd.times.Add(postT); //add postPointT

                eventData.Add(trackID, pd);

                //update start and end time of the event
                if (preT < eventStartTime)
                    eventStartTime = preT;
                if (postT > eventEndTime)
                    eventEndTime = postT;
            }

            //go to the next line
            line += 1;
        }

        //sort particle events in time
        for (int i = 0; i < allTrackIDS.Count; i++)
        {
            //bubble sort
            int breakPoint = eventData[allTrackIDS[i]].times.Count - 1;
            for (int j = 0; j < eventData[allTrackIDS[i]].times.Count; j++)
            {
                bool hasNotSwitched = true;

                if (eventData[allTrackIDS[i]].times[j] > eventData[allTrackIDS[i]].times[j + 1])
                {
                    hasNotSwitched = false;

                    //switch time
                    float tempTime = eventData[allTrackIDS[i]].times[j];
                    eventData[allTrackIDS[i]].times[j] = eventData[allTrackIDS[i]].times[j + 1];
                    eventData[allTrackIDS[i]].times[j + 1] = tempTime;

                    //switch coordinate
                    Vector3 tempPos = eventData[allTrackIDS[i]].points[j];
                    eventData[allTrackIDS[i]].points[j] = eventData[allTrackIDS[i]].points[j + 1];
                    eventData[allTrackIDS[i]].points[j + 1] = tempPos;
                }

                breakPoint -= 1;

                if (hasNotSwitched)
                    break;
            }
        }

        //Calculate Time Range
        for (int i = 0; i < allTrackIDS.Count; i++)
        {
            eventData[allTrackIDS[i]].DetermineTimeRange();
        }

        //create a Particle System for each particle type and change its appearance
        for (int i = 0; i < particleTypes.Length; i++)
        {
            //check if a particle system already exists
            if (particleTypes[i].ps == null) 
            {
                ParticleSystem ps = Instantiate(particleSystemTemplate, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
                ps.transform.parent = this.transform;
                ps.transform.localPosition = Vector3.zero; //Reset transform so that the visualization runs correctly in local space. Not doing so means ou can only move the visulization after starting itw
                ps.transform.localRotation = Quaternion.identity;
                ps.transform.localScale = Vector3.one;

                Material mat = new Material(particleMat);
                mat.mainTexture = particleTypes[i].particleTexture;
                ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
                psr.sharedMaterial = mat;

                particleTypes[i].ps = ps;
            }
        }

        //create Error ParticleSystem
        //check if a error particle system already exists
        if (errorPS == null) 
        {
            errorPS = Instantiate(particleSystemTemplate, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            errorPS.transform.parent = this.transform;
            errorPS.transform.localPosition = Vector3.zero;//Reset transform so that the visualization runs correctly in local space. 
            errorPS.transform.localRotation = Quaternion.identity;
            errorPS.transform.localScale = Vector3.one;

            Material errorMat = new Material(particleMat);
            errorMat.mainTexture = errorTexture;
            ParticleSystemRenderer errorPsr = errorPS.GetComponent<ParticleSystemRenderer>();
            errorPsr.sharedMaterial = errorMat;
        }

        Debug.Log("Creating the particle dictionary took " + (Time.realtimeSinceStartup - s) + "s");
    }

    void CreateTrailDictionary()
    {
        float s = Time.realtimeSinceStartup; //value for debugging

        //create a dictionary of the particle trails
        for (int i = 0; i < allTrackIDS.Count; i++)
        {
            //check wether the trail already exists or not
            if (!particleTrails.ContainsKey(allTrackIDS[i]))
            {
                ParticleData pd = eventData[allTrackIDS[i]];

                //create line renderer
                GameObject spawn = new GameObject(pd.particleName + ", ID:" + allTrackIDS[i].ToString());
                spawn.transform.parent = this.transform;
                spawn.layer = overlayLayer; //place the line renderers on a layer, so that it can be overlayed on the screen
                spawn.transform.localPosition = Vector3.zero;//Reset transform so that the visualization runs correctly in local space. 
                spawn.transform.localRotation = Quaternion.identity;
                spawn.transform.localScale = Vector3.one;

                //change settings on line renderer
                LineRenderer lr = spawn.AddComponent<LineRenderer>();
                lr.material = trailMat;
                lr.positionCount = 0;
                lr.useWorldSpace = false;
                lr.sortingOrder = -1;

                //add to dictionary
                particleTrails.Add(allTrackIDS[i], lr);
            }
        }

        Debug.Log("Creating the trail dictionary took " + (Time.realtimeSinceStartup - s) + "s");
    }

    void VisualizePaths() 
    {
        //visualize particle paths
        if (generatePaths)
        {
            generatePaths = false;

            //delete old lineRenderers
            //This part could be replaced by a pooling system
            foreach (LineRenderer lr in pathRenderers)
            {
                Destroy(lr.gameObject);
            }
            pathRenderers = new List<LineRenderer>();

            //create new lineRenderers and reconstruct paths
            for (int i = 0; i < allTrackIDS.Count + 1; i++)
            {
                ParticleData pd = eventData[allTrackIDS[i]];

                //create LineRenderer
                GameObject spawn = new GameObject(pd.particleName + "path");
                spawn.transform.parent = this.transform;
                spawn.transform.localPosition = Vector3.zero;//Reset transform so that the visualization runs correctly in local space. 
                spawn.transform.localRotation = Quaternion.identity;
                spawn.transform.localScale = Vector3.one;

                LineRenderer lr = spawn.AddComponent<LineRenderer>();
                pathRenderers.Add(lr);

                lr.material = lineMat;
                lr.colorGradient = lineColor;
                lr.useWorldSpace = false;

                float lWidth = lineWidth * transform.parent.localScale.x;
                lr.startWidth = lWidth;
                lr.endWidth = lWidth;

                lr.sortingOrder = -2;

                //Visualize Path
                lr.positionCount = pd.points.Count;
                for (int j = 0; j < pd.points.Count; j++)
                {
                    lr.SetPosition(j, pd.points[j] * scale);
                }
            }
            Debug.Log("Creating the dictionary and line renderers took " + Time.realtimeSinceStartup.ToString() + "s");
        }
        else 
        {
            //adjust width of existing lineRenderers
            foreach (LineRenderer lr in pathRenderers)
            {
                float lWidth = lineWidth * transform.parent.localScale.x;
                lr.startWidth = lWidth;
                lr.endWidth = lWidth;
            }
        }
    }

    void VisualizeEvent()
    {
        for (int i = 0; i < particleTypes.Length; i++)
        {
            //update particles
            if (particleTypes[i].psParticles == null || particleTypes[i].psParticles.Length < particleTypes[i].ps.main.maxParticles)
            {
                particleTypes[i].psParticles = new ParticleSystem.Particle[particleTypes[i].ps.main.maxParticles];
            }

            //Clear all existing particles
            int alivePat = particleTypes[i].ps.GetParticles(particleTypes[i].psParticles);
            for (int j = 0; j < alivePat; j++)
            {
                particleTypes[i].psParticles[j].startLifetime = 0f;
                particleTypes[i].psParticles[j].remainingLifetime = 0f;
            }

            //apply changes to particle system
            particleTypes[i].ps.SetParticles(particleTypes[i].psParticles, alivePat);
        }

        //update error particles
        if (errorParticles == null || errorParticles.Length < errorPS.main.maxParticles)
        {
            errorParticles = new ParticleSystem.Particle[errorPS.main.maxParticles];
        }
        //Clear all existing error particles
        int errorAlivePat = errorPS.GetParticles(errorParticles);
        for (int j = 0; j < errorAlivePat; j++)
        {
            errorParticles[j].startLifetime = 0f;
            errorParticles[j].remainingLifetime = 0f;
        }
        //apply changes to particle system
        errorPS.SetParticles(errorParticles, errorAlivePat);

        //go through each particle and check if they exist at currentTime
        for (int i = 0; i < allTrackIDS.Count; i++)
        {
            //visualize particle
            ParticleData pd = eventData[allTrackIDS[i]];

            if (pd.minTime < currentTime && currentTime <= pd.maxTime)
            {
                //find the 2 closest times (1 higher and 1 lower than currentTime) and their corresponding points
                float smallestTime = pd.minTime;
                float largestTime = pd.maxTime;

                Vector3 smallPoint = pd.points[0];
                Vector3 largePoint = pd.points[0];

                int timeIndex = 0; //later used for creating the trail of a particle

                //times are already sorted (ascending order), so it should be fine just going through the list
                for (int j = 0; j < pd.times.Count; j++)
                {
                    if (currentTime < pd.times[j])
                    {
                        smallestTime = pd.times[j - 1];
                        largestTime = pd.times[j];

                        smallPoint = pd.points[j - 1];
                        largePoint = pd.points[j];

                        timeIndex = j - 1;

                        break;
                    }
                }

                //calculate particle pos
                float p = (currentTime - smallestTime) / (largestTime - smallestTime); //lerp between the 2 previously determined positions
                Vector3 pos = Vector3.Lerp(smallPoint, largePoint, p) * scale;

                //check if the particle has a corresponding entry in particleTypes. Use error particles if no entry is found
                ParticleSystem ps = errorPS;
                ParticleSystem.Particle[] psParticles = errorParticles;
                Gradient trailCol = errorTrailColor; //Color of the particle trail

                for (int j = 0; j < particleTypes.Length; j++)
                {
                    if (particleTypes[j].particleName == pd.particleName)
                    {
                        ps = particleTypes[j].ps;
                        psParticles = particleTypes[j].psParticles;
                        trailCol = particleTypes[j].trailColor;

                        break;
                    }
                }

                //spawn and move a particle from a particle system
                ps.Emit(1);

                int alivePat = ps.GetParticles(psParticles);
                psParticles[alivePat - 1].startLifetime = Mathf.Infinity;
                psParticles[alivePat - 1].position = pos;

                //apply changes to particle system
                ps.SetParticles(psParticles, alivePat);

                //visualize trail
                LineRenderer lr = particleTrails[allTrackIDS[i]];
                lr.colorGradient = trailCol; //change trail color according to particle type
                lr.widthCurve = CurveScaler(trailShape, trailWidth * transform.parent.localScale.x);

                List<Vector3> trailPos = new List<Vector3>();
                trailPos.Add(pos);

                for (int j = timeIndex; j >= 0; j--)
                {
                    if (pd.times[j] > currentTime - trailLifeTime)
                    {
                        trailPos.Add(pd.points[j] * scale);
                    }
                    else
                    {
                        //Lerp between the last points before and after (currentTime - trailLifetime)
                        float sTime = pd.times[j];
                        float lTime = pd.times[j + 1];

                        float q = (currentTime - trailLifeTime - sTime) / (lTime - sTime);
                        Vector3 tPos = Vector3.Lerp(pd.points[j], pd.points[j + 1], q) * scale;

                        trailPos.Add(tPos);

                        break;
                    }
                }

                lr.positionCount = trailPos.Count;
                for (int k = 0; k < trailPos.Count; k++)
                {
                    lr.SetPosition(k, trailPos[k]);
                }
            }
            else
            {
                //deactivate trails of non-visible particles
                LineRenderer lr = particleTrails[allTrackIDS[i]];
                lr.positionCount = 0;
            }
        }
    }

    void ResetEventVisualization() 
    {
        if (!canSwitchState) 
        {
            Debug.Log("Resetting Event Visualization");
            //delete currently active visualization
            //delete paths
            foreach (LineRenderer lr in pathRenderers)
            {
                Destroy(lr.gameObject);
            }

            //delete particles
            for (int i = 0; i < particleTypes.Length; i++)
            {
                if (particleTypes[i].ps != null)
                {
                    //update particles
                    if (particleTypes[i].psParticles == null || particleTypes[i].psParticles.Length < particleTypes[i].ps.main.maxParticles)
                    {
                        particleTypes[i].psParticles = new ParticleSystem.Particle[particleTypes[i].ps.main.maxParticles];
                    }

                    //Clear all existing particles
                    int alivePat = particleTypes[i].ps.GetParticles(particleTypes[i].psParticles);
                    for (int j = 0; j < alivePat; j++)
                    {
                        particleTypes[i].psParticles[j].startLifetime = 0f;
                        particleTypes[i].psParticles[j].remainingLifetime = 0f;
                    }

                    //apply changes to particle system
                    particleTypes[i].ps.SetParticles(particleTypes[i].psParticles, alivePat);
                }
            }

            if (errorPS != null)
            {
                //update error particles
                if (errorParticles == null || errorParticles.Length < errorPS.main.maxParticles)
                {
                    errorParticles = new ParticleSystem.Particle[errorPS.main.maxParticles];
                }
                //Clear all existing error particles
                int errorAlivePat = errorPS.GetParticles(errorParticles);
                for (int j = 0; j < errorAlivePat; j++)
                {
                    errorParticles[j].startLifetime = 0f;
                    errorParticles[j].remainingLifetime = 0f;
                }
                //apply changes to particle system
                errorPS.SetParticles(errorParticles, errorAlivePat);
            }

            //delete trails
            foreach (LineRenderer lr in particleTrails.Values)
            {
                lr.positionCount = 0;
            }
        }
    }
    void LoopTime()
    {
        currentTime += playbackSpeed * Time.deltaTime;

        if (currentTime > eventEndTime)
        {
            currentTime = eventStartTime;
        }
        if (currentTime < eventStartTime)
        {
            currentTime = eventEndTime;
        }
    }

    AnimationCurve CurveScaler(AnimationCurve ac, float scale)
    {
        //I just copied this script from here: https://discussions.unity.com/t/scaling-an-animationcurve-variable/738755/3
        //I don't really understand how it works

        AnimationCurve scaledCurve = new AnimationCurve();

        for (int i = 0; i < ac.keys.Length; i++)
        {
            Keyframe keyframe = ac.keys[i];
            keyframe.value = ac.keys[i].value * scale;
            keyframe.time = ac.keys[i].time * scale;
            keyframe.inTangent = ac.keys[i].inTangent * scale;
            keyframe.outTangent = ac.keys[i].outTangent * scale;

            scaledCurve.AddKey(keyframe);
        }
        return scaledCurve;
    }

    public void ChangeState(VisualizationState stateToChangeTo) 
    {

        if (nextStates.Count > 0)
        {
            if(stateToChangeTo != nextStates[nextStates.Count - 1])
                nextStates.Add(stateToChangeTo);
        }
        else 
        {
            nextStates.Add(stateToChangeTo);
        }
    }
}
