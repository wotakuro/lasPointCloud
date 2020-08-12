using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PointCloud.LasFormat
{

    public class LasLoader
    {
        public static LasLoadBehaviour Instanciate(string name)
        {
            GameObject gmo = new GameObject(name);
            var lasLoader = gmo.AddComponent<LasLoadBehaviour>();
            return lasLoader;
        }

        public static LasLoadBehaviour InstanciateAndAsyncLoad(string path, Material mat, int reductionParam = 1, MeshGenerator.Config conf = default)
        {
            GameObject gmo = new GameObject(GetFilename(path));
            var lasLoader = gmo.AddComponent<LasLoadBehaviour>();
            lasLoader.SetMaterial(mat);
            lasLoader.LoadDataAsync(path, ref conf, reductionParam);
#if DEBUG
            lasLoader.HeaderInfo.DebugWriteToFile(path + "_header.txt");
#endif
            return lasLoader;
        }

        public static string GetFilename(string path)
        {
            int separatorSlash = path.LastIndexOf('/');
            int separatorBs = path.LastIndexOf('\\');

            int separator = separatorBs;
            if (separatorBs < separatorSlash)
            {
                separator = separatorSlash;
            }
            int extention = path.LastIndexOf('.');
            if (extention < separator)
            {
                extention = path.Length;
            }

            return path.Substring(separator + 1, extention - separator - 1);

        }
    }
}