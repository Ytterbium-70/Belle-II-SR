using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticlePathData
{
    public string particleName;

    public List<Vector3> points = new List<Vector3>();
    public List<float> times = new List<float>();

    public float PathLength() 
    {
        float l = 0f;
        for (int i = 0; i < points.Count - 1; i++) 
        {
            float dis = Vector3.Distance(points[i], points[i + 1]);
            l += dis;
        }
        return l;
    }
}

public class PathVisualizer : MonoBehaviour
{
    [Header("General Settings")]
    public VisualizationState state;
    [SerializeField] List<VisualizationState> nextStates;
    [SerializeField] bool canSwitchState = true;
    [Space]
    public int overlayLayer = 31;

    [Space]
    public float scale = 0.01f;
    float lastScale;

    bool changeVisualization = true;

    [Header("Appearance by Particle Type")]
    public ParticleType[] particleTypes;

    [Header("Particle Visualization")]
    List<string> particlesToVisualize = new List<string> { "e-", "e+", "mu+", "mu-", "pi+", "pi-", "proton", "deuteron", "sigma+", "kaon-", "kaon+", "anti_proton", "k_star-", "rho-", "rho+", "D-", "Ds-", "k_star+", "D+", "B+", "B-", "tau+", "alpha", "omega", "triton", "a1(1260)+" };
    public GameObject particleSystemTemplate; //A GameObject with a ParticleSystem that emit no particles, uses local simulation space and hierachy scaling mode
    public Material particleMat; //a Particles/Standard Unlit material set to Cutout. Texture will be applied at runtime
    [Space]
    public Texture errorTexture;
    ParticleSystem errorPS;
    ParticleSystem.Particle[] errorParticles;
    List<int> allTrackIDS = new List<int>();

    [Header("Trail Visualization")]
    public float trailWidth = 0.1f;
    public AnimationCurve trailShape;
    public Material trailMat;
    [Space]
    public Gradient errorTrailColor;
    Dictionary<int, LineRenderer> particleTrails = new Dictionary<int, LineRenderer>();

    [Header("File Management")]
    public string fileDirectory = "Belle2ParticleEvents/"; //folder in the Resources folder where the track data files are located
    public string fileName;
    public List<string> extractedData = new List<string>();
    Dictionary<int, ParticlePathData> eventData = new Dictionary<int, ParticlePathData>();

    private void Start()
    {
        nextStates = new List<VisualizationState>();
        lastScale = scale;

        if (transform.parent == null) 
        {
            Debug.Log("Caution: PathVisualizer on " + this.name + " requires a parent object");
        }
    }

    private void Update()
    {
        UpdateVisualization();

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
            CreateParticleDictionary();
            CreateTrailDictionary();

            canSwitchState = true;
            changeVisualization = true;
            nextStates.Add(VisualizationState.VISUALIZING);
        }
        else if (state == VisualizationState.VISUALIZING)
        {
            canSwitchState = true;
            //visualize event
            VisualizeEvent();
        }
    }

    void CreateParticleDictionary()
    {
        float s = Time.realtimeSinceStartup; //Value for debugging

        //generate a list with all the lines of a file
        TextAsset eventFile = (TextAsset)Resources.Load(fileDirectory + fileName);
        List<string> fileLines = new List<string>(eventFile.text.Split('\n'));

        int line = 1;  //skip 1st line

        //create a dictionary of the particles in a event. Use Track ID as key for the dictionary
        eventData = new Dictionary<int, ParticlePathData>();
        allTrackIDS.Clear();
        while (line < fileLines.Count - 1)
        {
            //extract data from line
            extractedData.Clear();
            extractedData = new List<string>(fileLines[line].Split(','));

            //check if particle already has an entry in the dictionary
            int trackID = int.Parse(extractedData[0]);
            if (eventData.ContainsKey(trackID))
            {
                //change existing particleData entry
                ParticlePathData ppd = eventData[trackID];

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

                //check for duplicates
                if (!ppd.times.Contains(preT))
                {
                    ppd.points.Add(preP); //add prePoints
                    ppd.times.Add(preT); //add prePointT
                }

                //check for duplicates
                if (!ppd.times.Contains(postT))
                {
                    ppd.points.Add(postP); //add postPoints
                    ppd.times.Add(postT); //add postPointT
                }

                eventData[trackID] = ppd;
            }
            else
            {
                //create new entry if it's in the list of particles to visualize
                if (particlesToVisualize.Contains(extractedData[2]))
                {
                    //ignore negative trackID's, since those are the e- and e+ used in the collider
                    if (trackID >= 0)
                    {
                        allTrackIDS.Add(trackID);
                        ParticlePathData ppd = new ParticlePathData();

                        ppd.particleName = extractedData[2];

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

                        ppd.points.Add(preP); //add prePoints
                        ppd.points.Add(postP); //add postPoints
                        ppd.times.Add(preT); //add prePointT
                        ppd.times.Add(postT); //add postPointT

                        eventData.Add(trackID, ppd);
                    }
                }
            }

            //go to the next line
            line += 1;
        }

        //sort particle events in time (ascending order)
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

        CreateParticleSystems();

        Debug.Log("Creating the particle dictionary took " + (Time.realtimeSinceStartup - s) + "s");
    }

    void CreateParticleSystems()
    {
        //create a Particle System for each particle type and change its appearance
        for (int i = 0; i < particleTypes.Length; i++)
        {
            //check if a particle system already exists
            if (particleTypes[i].ps == null)
            {
                ParticleSystem ps = Instantiate(particleSystemTemplate, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
                ps.name = "PathVisualizer PatSys: " + particleTypes[i].particleName;

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
            errorPS.name = "PathVisualizer ErrorPatSys";

            errorPS.transform.parent = this.transform;
            errorPS.transform.localPosition = Vector3.zero;//Reset transform so that the visualization runs correctly in local space. 
            errorPS.transform.localRotation = Quaternion.identity;
            errorPS.transform.localScale = Vector3.one;

            Material errorMat = new Material(particleMat);
            errorMat.mainTexture = errorTexture;
            ParticleSystemRenderer errorPsr = errorPS.GetComponent<ParticleSystemRenderer>();
            errorPsr.sharedMaterial = errorMat;
        }
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
                ParticlePathData ppd = eventData[allTrackIDS[i]];

                //create line renderer
                GameObject spawn = new GameObject(ppd.particleName + ", ID:" + allTrackIDS[i].ToString());
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

    void VisualizeEvent()
    {
        if (changeVisualization)
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

            //Reset Trails
            for (int i = 0; i < allTrackIDS.Count; i++) 
            {
                particleTrails[allTrackIDS[i]].positionCount = 0;
            }

            //go through each particle and draw the trails
            for (int i = 0; i < allTrackIDS.Count; i++)
            {
                //visualize particle
                ParticlePathData ppd = eventData[allTrackIDS[i]];

                //filer ionization e-, which have a short path
                if (ppd.PathLength() > 50f) 
                {
                    //set particle position to the last position in time
                    Vector3 pos = ppd.points[ppd.points.Count - 1] * scale;

                    //check if the particle has a corresponding entry in particleTypes. Use error particles if no entry is found
                    ParticleSystem ps = errorPS;
                    ParticleSystem.Particle[] psParticles = errorParticles;
                    Gradient trailCol = errorTrailColor; //Color of the particle trail

                    for (int j = 0; j < particleTypes.Length; j++)
                    {
                        if (particleTypes[j].particleName == ppd.particleName)
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

                    lr.positionCount = ppd.points.Count;

                    for (int j = 0; j < ppd.points.Count; j++)
                    {
                        //Flipping the order here, since the EventVisualizer also does it in reverse order. Otherwise, the gradient would go in opposite directions
                        lr.SetPosition(j, ppd.points[ppd.points.Count - 1 - j] * scale);
                    }
                }
            }


            changeVisualization = false;
        }
    }

    void ResetEventVisualization()
    {
        if (!canSwitchState)
        {
            Debug.Log("Resetting Path Visualization");
            //delete currently active visualization

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
            if (stateToChangeTo != nextStates[nextStates.Count - 1])
                nextStates.Add(stateToChangeTo);
        }
        else
        {
            nextStates.Add(stateToChangeTo);
        }
    }

    void UpdateVisualization()
    {
        //only change visualization when a setting is changed in order to avoid lag
        if (lastScale != scale)
        {
            changeVisualization = true;
            lastScale = scale;
        }
    }
}