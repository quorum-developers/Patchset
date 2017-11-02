using System;
using System.IO;
using System.Text.RegularExpressions;

namespace QT.ScriptsSet.Core
{
    public class ScriptListItem
    {
        private string _sourceFileName;

        public ScriptListItem(string fileName, string description = "")
        {
            Id = Guid.NewGuid();
            Description = description;
            SourceFileName = fileName;
        }

        /// <summary>
        /// Возвращает или задает id файла.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Возвращает или задает пусть к файлу.
        /// </summary>
        public string PathName => Path.GetDirectoryName(SourceFileName);

        /// <summary>
        /// Возвращает или задает исходное имя файла.
        /// </summary>
        public string SourceOnlyFileName => Path.GetFileName(SourceFileName);

        /// <summary>
        /// Возвращает или задает виртуальное имя файла.
        /// </summary>
        public string TargetOnlyFileName
        {
            get
            {
                string virtualFileName = SourceOnlyFileName;

                Match match = Regex.Match(SourceOnlyFileName, @"[A-Za-zА-Юя-я]+");

                if (match.Success)
                {
                    virtualFileName = virtualFileName.Substring(match.Index);
                }

                if ((!string.IsNullOrWhiteSpace(Version)) && (!string.IsNullOrWhiteSpace(Index)))
                {

                    virtualFileName = $"{Version}.{Index.PadLeft(2, '0')}.{virtualFileName}";
                }

                return virtualFileName;
            }
        }

        /// <summary>
        /// Возвращает или задает описание файла.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Возвращает или задает признак добавления нумерации перед именем файла.
        /// </summary>
        public bool AddNumber { get; set; } = true;

        public string Version { get; set; }

        public string Index { get; set; }

        public string SourceFileName { get; set; }
    }
}
