#pragma strict

var terrain : Terrain;

var resolution : int = 513;

var doAll : boolean = false;

 

function Start ()
{

    FixResolution ();

    var tdata = new TerrainData();

    var size : float= 100.0 / resolution * 32.0;

    tdata.size = new Vector3(size, 20, size);

    tdata.heightmapResolution = resolution;

    var t = Camera.main.transform;

    t.position = Vector3(120,31,-5);

    t.localEulerAngles = Vector3(27,310,0);

    var lo = new GameObject();

    var l : Light= lo.AddComponent(Light);

    l.type = LightType.Directional;

    lo.transform.localEulerAngles = Vector3(40,20,0);

}

 

function Update ()
{

    var tdata : TerrainData = terrain.terrainData;

    var width = tdata.heightmapWidth;

    var height = tdata.heightmapHeight;

    if(!doAll){

        width /= 8;

        height /= 8;

    }

    var heights : float[,]=new float[height, width];

    for(var y=0; y<height; y++){

        for(var x=0; x<width; x++){

            heights[y,x]=Random.value;

        }

    }

    tdata.SetHeights(0,0,heights);

}

 

function FixResolution ()
{

    var c = 0;

    while(resolution > 0){

        resolution = resolution >> 1;

        c++;

    }

    resolution =Mathf.Pow(2, c - 1) + 1;

    if(resolution > 513) resolution = 513;

    if(resolution < 33) resolution = 33;

}