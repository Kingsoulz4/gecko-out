using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public enum MechanicNames
    {
        Freeze,
        KeyLock,
        Hidden,
        DoubleColor
    }

    [CreateAssetMenu(fileName = "MechanicsReferences", menuName = "ScriptableObjects/MechanicsReferences")]

    public class MechanicsReferences : ScriptableObject
    {
        public SerializedDictionary<MechanicNames, MechanicRendererBase> listMechanicRenderer;

    }

    
}
