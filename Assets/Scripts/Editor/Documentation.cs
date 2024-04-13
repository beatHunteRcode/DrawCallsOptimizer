using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Documentation
{
    public const string COMBINE_OBJECTS_BY_POLYGONS_TEXT = "Combines all objects in each chunk if sum of all objects polygons (triangles) are greater than Chunk Objects Polygons Threshold";
    public const string COMBINE_OBJECTS_BY_MATERIALS_TEXT = "In each chunk all objects with specific materials will combine in one GameObject";
    public const string COMBINE_OBJECTS_BY_TAGS_TEXT = "In each chunk all objects with specific tag will combine in one GameObject";
    public const string COMBINE_OBJECTS_BY_DISTANCE_TEXT = "Combines all child-objects IN SPECIFIC COLLECTION OF OBJECTS TO COMBINE if distances between all child-objects pivots in collection are lesser or equals Objects Distance Threshold\n\n" +
        "If number of Collections of Objects to Combine = 0, than combines all objects IN CHUNK if distances between all objects pivots in chunk are lesser or equals Objects Distance Threshold ";
}
