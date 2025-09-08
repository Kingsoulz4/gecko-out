using AYellowpaper.SerializedCollections;
using Geckout.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    [CreateAssetMenu(fileName = "ColorAndMaterialData", menuName = "ScriptableObjects/ColorAndMaterialData", order = 1)]
    public class ColorAndMaterialData : ScriptableObject
    {
        public SerializedDictionary<ColorType, Color> listColor;

        public SerializedDictionary<ColorType, Material> listDogMaterial;

        public SerializedDictionary<ColorType, Material> listPortalMaterial;
    }
}
