using System.IO;
using UnityEngine;
using UnityEngine.Rendering.UI;

namespace ZResLoad
{
    public class PackageFactory
    {
        private static string _RootResURL = "";
        private static string _AppContentURL = "";

        public static void Init(string resURL, string appContentURL)
        {
            _RootResURL = resURL;
            _AppContentURL = appContentURL;
        }

        public static IResPackage CreatePackage(string packageName)
        {
            var resURL = TryGetPackageFilePath(packageName);
            if (string.IsNullOrEmpty(resURL))
                return null;

            var package = PackageRes.Create(packageName);
            package.SetResURL(resURL);
            package.LoadPackage();
            package.IncRef();

            return package;
        }

        private static string TryGetPackageFilePath(string packageName)
        {
            if (PackageRes.HasPackageFile(_RootResURL, packageName))
            {
                return _RootResURL;
            }
            if (PackageRes.HasPackageFile(_AppContentURL, packageName))
            {
                return _AppContentURL;
            }
            return null;
        }

        public static IResPackage CreateRawPackge(string packageName)
        {
            var resURL = TryGetRawPackageFilePath(packageName);
            if (string.IsNullOrEmpty(resURL))
                return null;

            var package = RawPackageRes.Create(packageName);
            package.SetResURL(resURL);
            package.LoadPackage();

            return package;
        }

        private static string TryGetRawPackageFilePath(string packageName)
        {
            if (RawPackageRes.HasPackageFile(_RootResURL, packageName))
                return _RootResURL;
            if (RawPackageRes.HasPackageFile(_AppContentURL, packageName))
                return _AppContentURL;
            return null;
        }

    }
}

