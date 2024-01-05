namespace ZResLoad
{
    public class VersionFile
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// MD5
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        public VersionFile()
        {

        }

        public static VersionFile PasrseFrom(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }
            var dd = data.Split('|');
            if (dd.Length >= 3)
            {
                return new VersionFile()
                {
                    Path = dd[0],
                    Hash = dd[1],
                    Version = dd[2],
                };
            }
            return null;
        }

        public override string ToString()
        {
            return string.Join("|", new[] { Path, Hash, Version });
        }
    }
}