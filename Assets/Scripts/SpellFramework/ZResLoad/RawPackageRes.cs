namespace ZResLoad
{
    internal class RawPackageRes : PackageRes
    {
        protected RawPackageRes(string packageName) : base(packageName)
        {
        }

        public static new IResPackage Create(string packageName)
        {
            var package = new RawPackageRes(packageName);
            return package;
        }

        public static new bool HasPackageFile(string packageUrl, string packageName)
        {
            var path = string.Format("{0}/{1}", packageUrl, packageName);
            if (System.IO.File.Exists(path))
                return true;
            if (FileIOManager.CheckStreamingFileExist(packageName))
                return true;
            return false;
        }

        protected override string GetPackagePath()
        {
            return string.Format("{0}/{1}", PackageURL, PackageName);
        }

        protected override string GetPackageName()
        {
            return PackageName;
        }
    }
}

