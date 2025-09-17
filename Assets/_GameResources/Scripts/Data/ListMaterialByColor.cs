using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace Geckout
{

    [CreateAssetMenu(fileName = "ListMaterialByColor", menuName = "ScriptableObjects/ListMaterialByColor", order = 1)]
    public class ListMaterialByColor : ScriptableObject
    {
        public SerializedDictionary<ColorType, Material> listMaterial;
    }
}
