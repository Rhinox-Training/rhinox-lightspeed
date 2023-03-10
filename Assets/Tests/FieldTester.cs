using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using UnityEngine;


public class FieldTester : MonoBehaviour
{
    [SerializeReference]
    public CustomCollection<string> Test;

    [SerializeReference]
    public SceneReference Scene;
}
