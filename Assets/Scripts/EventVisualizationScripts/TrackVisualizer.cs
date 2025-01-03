using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TrackData
{
    public string particleName;
    public List<Vector3> trackPoints = new List<Vector3>();
}

public class TrackVisualizer : MonoBehaviour
{
    [Header("General Settings")]
    public VisualizationState state;
    [SerializeField] List<VisualizationState> nextStates;
    [SerializeField] bool canSwitchState = true;
    [Space]
    public int overlayLayer = 31;

    [Header("Appearance by Particle Type")]
    public ParticleType[] particleTypes;

    [Header("Particle Visualization")]
    public GameObject particleSystemTemplate; //A GameObject with a ParticleSystem that emit no particles, uses local simulation space and hierachy scaling mode
    public Material particleMat; //a Particles/Standard Unlit material set to Cutout. Texture will be applied at runtime
    [Space]
    public Texture errorTexture;
    ParticleSystem errorPS;
    ParticleSystem.Particle[] errorParticles;

    [Header("Track Visualization")]
    public Material trackMat;
    public float trackWidth = 0.04f;
    public float scale = 1f;
    [Space]
    public Gradient errorTrackColor;
    [Space]
    public float reconstructionPoint;
    public float playbackSpeed = 0.25f;

    [Header("File Management")]
    public string fileDirectory = "Belle2Tracks/"; //folder in the Resources folder where the track data files are located
    public string fileName = "";
    public List<string> extractedData = new List<string>();
    Dictionary<int, TrackData> trackData = new Dictionary<int, TrackData>();
    Dictionary<int, LineRenderer> trackLines = new Dictionary<int, LineRenderer>();

    void Start()
    {
        nextStates = new List<VisualizationState>();

        if (transform.parent == null)
        {
            Debug.Log("Caution: TrackVisualizer on " + this.name + " requires a parent object");
        }
    }

    private void Update()
    {
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
            ResetTrackVisualization();

            canSwitchState = true;
        }
        else if (state == VisualizationState.LOADING)
        {
            CreateTrackDictionary();
            CreateParticleSystems();

            canSwitchState = true;
            nextStates.Add(VisualizationState.VISUALIZING);
        }
        else if (state == VisualizationState.VISUALIZING)
        {
            VisualizeTracks();

            canSwitchState = true;
        }
    }

    void CreateTrackDictionary()
    {
        TextAsset eventFile = (TextAsset)Resources.Load(fileDirectory + fileName);

        List<string> fileLines = new List<string>(eventFile.text.Split('\n'));
        int line = 1;

        //create list of positions
        trackData = new Dictionary<int, TrackData>();
        while (line < fileLines.Count - 1)
        {
            //extract data from line
            extractedData.Clear();
            extractedData = new List<string>(fileLines[line].Split(','));

            int id = int.Parse(extractedData[0]);

            string particleName = extractedData[1];

            Vector3 pos = new Vector3(
                    float.Parse(extractedData[2]),
                    float.Parse(extractedData[3]),
                    float.Parse(extractedData[4]));

            //check if the id already has an entry
            if (trackData.ContainsKey(id))
            {
                //update existing entry
                List<Vector3> h = trackData[id].trackPoints;
                h.Add(pos);
            }
            else
            {
                //create new entry
                TrackData td = new TrackData();
                td.particleName = particleName;
                td.trackPoints.Add(pos);
                trackData.Add(id, td);

                if (!trackLines.ContainsKey(id))
                {
                    //create a new line renderer
                    GameObject spawn = new GameObject("Lr " + particleName + ": " + id.ToString());
                    spawn.transform.parent = this.transform;
                    spawn.layer = overlayLayer; //place the line renderers on a layer, so that it can be overlayed on the screen
                    spawn.transform.localPosition = Vector3.zero;//Reset transform so that the visualization runs correctly in local space. 
                    spawn.transform.localRotation = Quaternion.identity;
                    spawn.transform.localScale = Vector3.one;

                    LineRenderer lr = spawn.AddComponent<LineRenderer>();
                    lr.material = trackMat;
                    lr.positionCount = 0;
                    lr.useWorldSpace = false;
                    lr.startWidth = trackWidth;
                    lr.endWidth = trackWidth;
                    lr.textureMode = LineTextureMode.Tile;

                    trackLines.Add(id, lr);
                }
            }

            line += 1;
        }
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
                ps.name = "TrackVisualizer PatSys: " + particleTypes[i].particleName;

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
            errorPS.name = "TrackVisualizer ErrorPatSys";

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

    void VisualizeTracks()
    {
        ResetParticles();

        //animate track length
        if(playbackSpeed != 0) 
        {
            reconstructionPoint += playbackSpeed * Time.deltaTime;
        }
        if (reconstructionPoint > 1f)
            reconstructionPoint = 0f;
        else if (reconstructionPoint < 0f)
            reconstructionPoint = 1f;

        for (int i = 0; i < trackData.Count; i++)
        {
            //create particle
            //check if the particle has a corresponding entry in particleTypes. Use error particles if no entry is found
            ParticleSystem ps = errorPS;
            ParticleSystem.Particle[] psParticles = errorParticles;
            Gradient trailCol = errorTrackColor; //Default color of the particle trail

            for (int j = 0; j < particleTypes.Length; j++)
            {
                if (particleTypes[j].particleName == trackData[i].particleName)
                {
                    ps = particleTypes[j].ps;
                    psParticles = particleTypes[j].psParticles;
                    trailCol = particleTypes[j].trailColor;

                    break;
                }
            }

            //spawn and move a particle from a particle system
            ps.Emit(1);

            //move particle to the end of the track
            int alivePat = ps.GetParticles(psParticles);
            psParticles[alivePat - 1].startLifetime = Mathf.Infinity;

            int count = (int)(trackData[i].trackPoints.Count * reconstructionPoint);
            int posIndex = Mathf.Clamp(count - 1, 0, 1000000000); //select last point in the list. Clamp to prevent index from becoming negative
            psParticles[alivePat - 1].position = trackData[i].trackPoints[posIndex] * scale;

            //apply changes to particle system
            ps.SetParticles(psParticles, alivePat);

            //create tracks
            trackLines[i].positionCount = count;
            trackLines[i].colorGradient = trailCol;

            for (int j = 0; j < count; j++)
            {
                trackLines[i].SetPosition(j, trackData[i].trackPoints[count - 1 - j] * scale);
            }
        }
    }

    void ResetParticles()
    {
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
    }

    void ResetTrackVisualization()
    {
        if (!canSwitchState)
        {
            Debug.Log("Resetting Path Visualization");

            reconstructionPoint = 0f;

            //delete particles
            ResetParticles();

            //reset line renderers
            foreach (LineRenderer lr in trackLines.Values)
            {
                lr.positionCount = 0;
            }
        }
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
}