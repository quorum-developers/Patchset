using System;
using System.Text.RegularExpressions;

namespace ScriptList.Core
{
    public class ScriptListItem
    {
        /// <summary>
        /// Возвращает или задает id файла.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Возвращает или задает пусть к файлу.
        /// </summary>
        public string PathName { get; set; }

        /// <summary>
        /// Возвращает или задает исходное имя файла.
        /// </summary>
        public string SourceFileName { get; set; }        

        /// <summary>
        /// Возвращает или задает виртуальное имя файла.
        /// </summary>
        public string VirtualFileName {
            get
            {
                string virtualFileName = SourceFileName;

                Match match = Regex.Match(SourceFileName, @"[A-Za-zА-Юя-я]+");

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
    }
}
