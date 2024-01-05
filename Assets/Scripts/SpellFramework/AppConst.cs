namespace SpellFramework
{
    public static class AppConst
    {
        /// <summary>
        /// 调试模式
        /// </summary>
        public static bool DebugMode = false;

        /// <summary>
        /// 游戏限帧的帧率
        /// </summary>
        public static int GameFrameRate = 60;

        /// <summary>
        /// 用户ID
        /// </summary>
        public static string UserId = string.Empty;

        /// <summary>
        /// 应用程序名称
        /// </summary>
        public static string AppName = "Demo";
        
        /// <summary>
        /// 资源包目录
        /// </summary>
        public static string AssetDirName = "StreamingAssets";

        public static string PersistentAbPath = "AB";

        public static string AbSourcePath = "Build";

        public static string StreamingAssetsAbOutPath = "AssetBundle";

        public static string AbmName = "AssetBundle";

        public static string FileListName = "files.txt";

        public static bool LoadFromAB = true;
    }
}