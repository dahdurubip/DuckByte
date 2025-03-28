using System;
using System.Linq;
using UnityEditor.PackageManager;

namespace UnionAvatars.Editor.Utils
{
    public class PackageUtilities
    {
        public static string GetPackageVersion(string packageName)
        {
            PackageInfo[] packageJsons = UnityEditor
                .AssetDatabase
                .FindAssets("package")
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Where(x => x.Contains(packageName) && x.Contains("package.json"))
                .Select(PackageInfo.FindForAssetPath)
                .ToArray();

            if (packageJsons.Length == 0)
                throw new InvalidOperationException(
                    "Couldn't find package: " + packageName + ". Please try to perform a clean install"
                );
            if (packageJsons.Length > 1)
                throw new InvalidOperationException(
                    "Found more than one package with name: " + packageName + ". Please try to perform a clean install"
                );

            return packageJsons[0].version;
        }
    }
}
