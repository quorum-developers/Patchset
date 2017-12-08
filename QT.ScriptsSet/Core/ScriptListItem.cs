using System;
using System.IO;
using System.Text.RegularExpressions;

namespace QT.ScriptsSet.Core
{
    /// <summary>
    /// Описывает элемент списка скриптов.
    /// </summary>
    public class ScriptListItem
    {
        public ScriptListItem(string fileName, string description = "")
        {
            Id = Guid.NewGuid();
            Description = description;
            SourceFileName = fileName;
        }

        /// <summary>
        /// Возвращает id файла.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Возвращает или задает пусть к файлу.
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// Возвращает исходное имя файла.
        /// </summary>
        public string SourceOnlyFileName => Path.GetFileName(SourceFileName);

        /// <summary>
        /// Возвращает виртуальное имя файла.
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
        /// Возвращает путь к исходному файлу.
        /// </summary>
        public string SourcePathName => Path.GetDirectoryName(SourceFileName);

        /// <summary>
        /// Возвращает или задает признак добавления нумерации перед именем файла.
        /// </summary>
        public bool AddNumber { get; set; } = true;

        public string Version { get; set; }

        public string Index { get; set; }
    }
}
